using System.Linq;
using ConsoleCity.Core;
using ConsoleCity.Infrastructure;
using ConsoleCity.World;

namespace ConsoleCity.Simulation;

public sealed class InfrastructureSimulationSystem : ISimulationSystem
{
    public string Name => "Infrastructure";

    public int TickInterval => 1;

    // Order chosen to run after basic world/agents but before services/economy (adjustable later)
    public int Order => 50;

    private readonly Dictionary<string, decimal> consumerDemand = new(StringComparer.OrdinalIgnoreCase);

    public void SetConsumerDemand(string consumerId, decimal demand)
    {
        if (demand < 0m) throw new ArgumentOutOfRangeException(nameof(demand));
        ArgumentNullException.ThrowIfNull(consumerId);
        consumerDemand[consumerId] = demand;
    }

    public decimal GetConsumerDemand(string consumerId) => consumerDemand.GetValueOrDefault(consumerId);

    private IReadOnlyList<UtilityConnection> GetConnectionsFromModel(IInfrastructureModel? infra)
    {
        if (infra is null) return Array.Empty<UtilityConnection>();
        return infra.Snapshot.Connections;
    }

    public void Execute(ISimulationContext context)
    {
        if (context is not SimulationContext simContext)
        {
            return;
        }

        var infra = simContext.Infrastructure;
        if (infra is null)
        {
            return;
        }

        // Aggregate building-based demand and any runtime overrides per connected node
        var nodeDemand = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        // Get list of buildings from world
        var world = simContext.World.Current;
        var buildings = world.Regions.SelectMany(r => r.Cities).SelectMany(c => c.Districts).SelectMany(d => d.Plots).SelectMany(p => p.Buildings).ToList();

        foreach (var building in buildings)
        {
            var consumerId = building.Id.Value.ToString();
            // Find the connection for this building
            var conn = infra.GetConnection(consumerId);
            if (conn is null) continue;

            // Find node and its utility type
            var node = infra.Snapshot.Networks.SelectMany(n => n.Nodes).FirstOrDefault(n => n.Id == conn.NodeId);
            if (node is null) continue;

            var utilityType = node.UtilityType;
            var baseDemand = ComputeBuildingDemand(building, utilityType);
            var overrideDemand = consumerDemand.GetValueOrDefault(consumerId);
            var totalDemand = baseDemand + overrideDemand;

            nodeDemand[conn.NodeId] = nodeDemand.GetValueOrDefault(conn.NodeId) + totalDemand;
        }

        // Apply computed demands to nodes
        foreach (var pair in nodeDemand)
        {
            try
            {
                infra.SetNodeDemand(pair.Key, pair.Value);
            }
            catch
            {
                // ignore invalid node ids
            }
        }

        // Advance the infrastructure model using the simulation clock
        var result = infra.Advance(simContext.Clock.Now);

        // Snapshot updated inside model; simulation engine will inspect snapshot for events
    }

    private static decimal ComputeBuildingDemand(BuildingModel building, UtilityType utilityType)
    {
        // Simple deterministic demand model based on building capacities
        var residents = building.Capacities.Residents;
        var jobs = building.Capacities.Jobs;
        var customers = building.Capacities.Customers;
        var production = building.Capacities.Production;

        return utilityType switch
        {
            UtilityType.Electricity => residents * 1.0m + jobs * 0.5m + production * 0.2m,
            UtilityType.Water => residents * 0.5m + jobs * 0.1m,
            UtilityType.Sewage => residents * 0.45m + jobs * 0.09m,
            UtilityType.Waste => residents * 0.2m + customers * 0.1m,
            UtilityType.Telecom => jobs * 0.2m + customers * 0.05m,
            UtilityType.Fuel => jobs * 0.1m + production * 0.1m,
            _ => 0m,
        };
    }
}
