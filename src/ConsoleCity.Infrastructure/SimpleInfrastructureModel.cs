using System.Collections.Generic;
using System.Linq;
using ConsoleCity.Core;

namespace ConsoleCity.Infrastructure;

public sealed class SimpleInfrastructureModel : IInfrastructureModel
{
    private readonly Dictionary<string, bool> lastOperationalStates = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<UtilityType, bool> lastShortageStates = new();

    public InfrastructureSnapshot Snapshot { get; private set; }

    public SimpleInfrastructureModel(InfrastructureSnapshot? snapshot = null)
    {
        Snapshot = snapshot ?? InfrastructureSnapshot.Empty;
        InitializeHistory(Snapshot);
    }

    public void SetConnections(IReadOnlyList<UtilityConnection> connections)
    {
        Snapshot = Snapshot.WithConnections(connections);
    }

    public void RegisterConnection(string consumerId, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(consumerId);
        ArgumentNullException.ThrowIfNull(nodeId);
        var conns = Snapshot.Connections.ToList();
        var existing = conns.FirstOrDefault(c => c.ConsumerId == consumerId);
        if (existing is not null) conns.Remove(existing);
        conns.Add(new UtilityConnection(consumerId, nodeId));
        Snapshot = Snapshot.WithConnections(conns);
    }

    public void UnregisterConnection(string consumerId)
    {
        ArgumentNullException.ThrowIfNull(consumerId);
        var conns = Snapshot.Connections.Where(c => c.ConsumerId != consumerId).ToList();
        Snapshot = Snapshot.WithConnections(conns);
    }

    public UtilityConnection? GetConnection(string consumerId)
    {
        if (string.IsNullOrWhiteSpace(consumerId)) return null;
        return Snapshot.Connections.FirstOrDefault(c => c.ConsumerId == consumerId);
    }

    public IReadOnlyList<UtilityConnection> GetConnectionsForNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId)) return Array.Empty<UtilityConnection>();
        return Snapshot.Connections.Where(c => c.NodeId == nodeId).ToList();
    }

    public string? GetNodeIdForConsumer(string consumerId)
    {
        var conn = GetConnection(consumerId);
        return conn?.NodeId;
    }

    public IReadOnlyList<string> GetConsumersForNode(string nodeId)
    {
        return GetConnectionsForNode(nodeId).Select(c => c.ConsumerId).ToList();
    }

    public void SetNodeDemand(string nodeId, decimal demand)
    {
        if (demand < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(demand));
        }

        Snapshot = Snapshot.WithNode(nodeId, node => new UtilityNode(node.Id, node.UtilityType, node.Position, node.Role, node.Capacity, demand, node.Supplied, node.IsOperational));
    }

    public void SetNodeOperational(string nodeId, bool isOperational)
        => Snapshot = Snapshot.WithNode(nodeId, node => new UtilityNode(node.Id, node.UtilityType, node.Position, node.Role, node.Capacity, node.Demand, node.Supplied, isOperational));

    public void SetEdgeOperational(string edgeId, bool isOperational)
        => Snapshot = Snapshot.WithEdge(edgeId, edge => new UtilityEdge(edge.Id, edge.FromNodeId, edge.ToNodeId, edge.Capacity, edge.Flow, isOperational));

    public InfrastructureAdvanceResult Advance(SimulationTime currentTime)
    {
        var events = new List<InfrastructureEvent>();
        var updatedNetworks = new List<UtilityNetwork>();

        foreach (var network in Snapshot.Networks)
        {
            var computed = RecomputeNetwork(network, currentTime);
            updatedNetworks.Add(computed.Network);
            events.AddRange(computed.Events);
        }

        Snapshot = new InfrastructureSnapshot(currentTime, updatedNetworks, events, Snapshot.Connections);
        RefreshHistory(Snapshot);
        return new InfrastructureAdvanceResult(Snapshot, events);
    }

    private void InitializeHistory(InfrastructureSnapshot snapshot)
    {
        foreach (var network in snapshot.Networks)
        {
            foreach (var node in network.Nodes)
            {
                lastOperationalStates[node.Id] = node.IsOperational;
            }

            foreach (var edge in network.Edges)
            {
                lastOperationalStates[edge.Id] = edge.IsOperational;
            }

            lastShortageStates[network.UtilityType] = false;
        }

        // Ensure connections exist in history map for stability (no-op currently)
        foreach (var conn in snapshot.Connections)
        {
            // placeholder: could store connection history if needed
        }
    }

    private void RefreshHistory(InfrastructureSnapshot snapshot)
    {
        foreach (var network in snapshot.Networks)
        {
            foreach (var node in network.Nodes)
            {
                lastOperationalStates[node.Id] = node.IsOperational;
            }

            foreach (var edge in network.Edges)
            {
                lastOperationalStates[edge.Id] = edge.IsOperational;
            }

            lastShortageStates[network.UtilityType] = network.Shortage > 0m;
        }

        // connections do not affect operational history directly but track presence
        foreach (var conn in snapshot.Connections)
        {
            // placeholder
        }
    }

    private (UtilityNetwork Network, IReadOnlyList<InfrastructureEvent> Events) RecomputeNetwork(UtilityNetwork network, SimulationTime currentTime)
    {
        var operationalNodes = network.Nodes.ToDictionary(node => node.Id, node => node);
        var operationalEdges = network.Edges.ToDictionary(edge => edge.Id, edge => edge);
        var pathCapacities = ComputePathCapacities(network, operationalNodes, operationalEdges);
        var consumerNodes = network.Nodes.Where(node => node.Role == UtilityNodeRole.Consumer).ToList();
        var totalDemand = consumerNodes.Sum(node => Math.Max(0m, node.Demand));
        var feasibleDemand = consumerNodes.ToDictionary(node => node.Id, node => Math.Min(Math.Max(0m, node.Demand), pathCapacities.GetValueOrDefault(node.Id)));
        var totalFeasibleDemand = feasibleDemand.Values.Sum();
        var totalSourceCapacity = network.Nodes.Where(node => node.Role == UtilityNodeRole.Source && node.IsOperational).Sum(node => Math.Max(0m, node.Capacity));
        var totalSupply = Math.Min(totalSourceCapacity, totalFeasibleDemand);
        var scale = totalFeasibleDemand <= 0m ? 0m : totalSupply / totalFeasibleDemand;
        var suppliedByConsumer = feasibleDemand.ToDictionary(pair => pair.Key, pair => pair.Value * scale);
        var updatedNodes = network.Nodes.Select(node =>
        {
            var supplied = node.Role == UtilityNodeRole.Consumer && suppliedByConsumer.TryGetValue(node.Id, out var consumerSupply) ? consumerSupply : 0m;
            return new UtilityNode(node.Id, node.UtilityType, node.Position, node.Role, node.Capacity, node.Demand, supplied, node.IsOperational);
        }).ToList();

        var pathByConsumer = ComputeBestPaths(network, pathCapacities, operationalNodes, operationalEdges);
        var edgeFlows = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in suppliedByConsumer)
        {
            if (pair.Value <= 0m)
            {
                continue;
            }

            if (!pathByConsumer.TryGetValue(pair.Key, out var path))
            {
                continue;
            }

            foreach (var edgeId in path)
            {
                edgeFlows[edgeId] = edgeFlows.GetValueOrDefault(edgeId) + pair.Value;
            }
        }

        var updatedEdges = network.Edges.Select(edge => new UtilityEdge(edge.Id, edge.FromNodeId, edge.ToNodeId, edge.Capacity, edgeFlows.GetValueOrDefault(edge.Id), edge.IsOperational)).ToList();
        var updatedNetwork = new UtilityNetwork(network.UtilityType, updatedNodes, updatedEdges, totalDemand, totalSupply);
        var events = BuildEvents(network, updatedNetwork, currentTime);
        return (updatedNetwork, events);
    }

    private IReadOnlyDictionary<string, decimal> ComputePathCapacities(
        UtilityNetwork network,
        IReadOnlyDictionary<string, UtilityNode> nodes,
        IReadOnlyDictionary<string, UtilityEdge> edges)
    {
        var capacities = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<(string NodeId, decimal Capacity)>();

        foreach (var source in network.Nodes.Where(node => node.Role == UtilityNodeRole.Source && node.IsOperational && node.Capacity > 0m))
        {
            capacities[source.Id] = Math.Max(capacities.GetValueOrDefault(source.Id), source.Capacity);
            queue.Enqueue((source.Id, source.Capacity));
        }

        while (queue.Count > 0)
        {
            var (nodeId, capacity) = queue.Dequeue();
            foreach (var edge in network.GetOutgoingEdges(nodeId).Where(edge => edge.IsOperational && edges.ContainsKey(edge.Id)))
            {
                if (!nodes.TryGetValue(edge.ToNodeId, out var nextNode) || !nextNode.IsOperational)
                {
                    continue;
                }

                var nextCapacity = Math.Min(capacity, edge.Capacity);
                if (nextNode.Capacity > 0m)
                {
                    nextCapacity = Math.Min(nextCapacity, nextNode.Capacity);
                }

                if (nextCapacity <= capacities.GetValueOrDefault(nextNode.Id))
                {
                    continue;
                }

                capacities[nextNode.Id] = nextCapacity;
                queue.Enqueue((nextNode.Id, nextCapacity));
            }
        }

        return capacities;
    }

    private IReadOnlyDictionary<string, IReadOnlyList<string>> ComputeBestPaths(
        UtilityNetwork network,
        IReadOnlyDictionary<string, decimal> capacities,
        IReadOnlyDictionary<string, UtilityNode> nodes,
        IReadOnlyDictionary<string, UtilityEdge> edges)
    {
        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>();
        var previousNode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var previousEdge = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in network.Nodes.Where(node => node.Role == UtilityNodeRole.Source && node.IsOperational && node.Capacity > 0m))
        {
            queue.Enqueue(source.Id);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var edge in network.GetOutgoingEdges(current).Where(edge => edge.IsOperational && edges.ContainsKey(edge.Id)))
            {
                if (!nodes.TryGetValue(edge.ToNodeId, out var nextNode) || !nextNode.IsOperational)
                {
                    continue;
                }

                if (previousNode.ContainsKey(nextNode.Id))
                {
                    continue;
                }

                previousNode[nextNode.Id] = current;
                previousEdge[nextNode.Id] = edge.Id;
                queue.Enqueue(nextNode.Id);
            }
        }

        foreach (var consumer in network.Nodes.Where(node => node.Role == UtilityNodeRole.Consumer && capacities.ContainsKey(node.Id) && capacities[node.Id] > 0m))
        {
            var path = new List<string>();
            var cursor = consumer.Id;
            while (previousNode.TryGetValue(cursor, out var prevNode) && previousEdge.TryGetValue(cursor, out var prevEdge))
            {
                path.Add(prevEdge);
                cursor = prevNode;
            }

            path.Reverse();
            result[consumer.Id] = path;
        }

        return result;
    }

    private IReadOnlyList<InfrastructureEvent> BuildEvents(UtilityNetwork previous, UtilityNetwork current, SimulationTime currentTime)
    {
        var events = new List<InfrastructureEvent>();

        foreach (var node in current.Nodes)
        {
            var previousState = lastOperationalStates.TryGetValue(node.Id, out var wasOperational) ? wasOperational : node.IsOperational;
            if (previousState != node.IsOperational)
            {
                events.Add(new InfrastructureEvent(
                    node.IsOperational ? InfrastructureEventType.OutageRecovered : InfrastructureEventType.OutageStarted,
                    current.UtilityType,
                    node.Id,
                    Snapshot.CapturedAt,
                    node.IsOperational ? 1m : 0m,
                    node.IsOperational
                        ? $"{current.UtilityType} node {node.Id} recovered."
                        : $"{current.UtilityType} node {node.Id} failed."));
            }
        }

        foreach (var edge in current.Edges)
        {
            var previousState = lastOperationalStates.TryGetValue(edge.Id, out var wasOperational) ? wasOperational : edge.IsOperational;
            if (previousState != edge.IsOperational)
            {
                events.Add(new InfrastructureEvent(
                    edge.IsOperational ? InfrastructureEventType.OutageRecovered : InfrastructureEventType.OutageStarted,
                    current.UtilityType,
                    edge.Id,
                    Snapshot.CapturedAt,
                    edge.IsOperational ? 1m : 0m,
                    edge.IsOperational
                        ? $"{current.UtilityType} link {edge.Id} recovered."
                        : $"{current.UtilityType} link {edge.Id} failed."));
            }
        }

        var shortageWasActive = lastShortageStates.TryGetValue(current.UtilityType, out var shortageState) && shortageState;
        var shortageIsActive = current.Shortage > 0m;
        if (shortageWasActive != shortageIsActive)
        {
            events.Add(new InfrastructureEvent(
                shortageIsActive ? InfrastructureEventType.CapacityShortageStarted : InfrastructureEventType.CapacityShortageRecovered,
                current.UtilityType,
                current.UtilityType.ToString(),
                currentTime,
                current.Shortage,
                shortageIsActive
                    ? $"{current.UtilityType} capacity shortage started."
                    : $"{current.UtilityType} capacity shortage recovered."));
        }

        return events;
    }
}
