using ConsoleCity.Core;

namespace ConsoleCity.Infrastructure;

public sealed record class InfrastructureSnapshot
{
    public SimulationTime CapturedAt { get; }

    public IReadOnlyList<UtilityNetwork> Networks { get; }

    public IReadOnlyList<InfrastructureEvent> Events { get; }

    public IReadOnlyList<UtilityConnection> Connections { get; }

    public InfrastructureSnapshot(SimulationTime capturedAt, IReadOnlyList<UtilityNetwork> networks, IReadOnlyList<InfrastructureEvent> events, IReadOnlyList<UtilityConnection>? connections = null)
    {
        ArgumentNullException.ThrowIfNull(networks);
        ArgumentNullException.ThrowIfNull(events);
        CapturedAt = capturedAt;
        Networks = networks;
        Events = events;
        Connections = connections ?? Array.Empty<UtilityConnection>();
    }

    public static InfrastructureSnapshot Empty { get; } = new(new SimulationTime(0), Array.Empty<UtilityNetwork>(), Array.Empty<InfrastructureEvent>(), Array.Empty<UtilityConnection>());

    public InfrastructureSnapshot WithNetworks(IReadOnlyList<UtilityNetwork> networks) => new(CapturedAt, networks, Events, Connections);

    public InfrastructureSnapshot WithConnections(IReadOnlyList<UtilityConnection> connections) => new(CapturedAt, Networks, Events, connections);

    public InfrastructureSnapshot WithNode(string nodeId, Func<UtilityNode, UtilityNode> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);
        var networks = Networks.Select(network =>
        {
            var node = network.GetNode(nodeId);
            return node is null ? network : network.WithNode(nodeId, updater);
        }).ToList();
        return new InfrastructureSnapshot(CapturedAt, networks, Events, Connections);
    }

    public InfrastructureSnapshot WithEdge(string edgeId, Func<UtilityEdge, UtilityEdge> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);
        var networks = Networks.Select(network =>
        {
            var edge = network.GetEdge(edgeId);
            return edge is null ? network : network.WithEdge(edgeId, updater);
        }).ToList();
        return new InfrastructureSnapshot(CapturedAt, networks, Events, Connections);
    }
}
