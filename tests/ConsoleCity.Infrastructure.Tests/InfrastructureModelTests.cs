using ConsoleCity.Core;
using ConsoleCity.Infrastructure;

namespace ConsoleCity.Infrastructure.Tests;

public class InfrastructureModelTests
{
    [Fact]
    public void Advance_DistributesCapacityToConnectedConsumers()
    {
        var source = new UtilityNode("source", UtilityType.Electricity, new GridPosition(0, 0), UtilityNodeRole.Source, 100m, 0m, 0m, true);
        var junction = new UtilityNode("junction", UtilityType.Electricity, new GridPosition(1, 0), UtilityNodeRole.Junction, 0m, 0m, 0m, true);
        var consumer = new UtilityNode("consumer", UtilityType.Electricity, new GridPosition(2, 0), UtilityNodeRole.Consumer, 0m, 50m, 0m, true);
        var sourceToJunction = new UtilityEdge("edge-1", source.Id, junction.Id, 80m, 0m, true);
        var junctionToConsumer = new UtilityEdge("edge-2", junction.Id, consumer.Id, 60m, 0m, true);
        var network = new UtilityNetwork(UtilityType.Electricity, [source, junction, consumer], [sourceToJunction, junctionToConsumer], 50m, 0m);
        var model = new SimpleInfrastructureModel(new InfrastructureSnapshot(new SimulationTime(0), [network], []));

        var result = model.Advance(new SimulationTime(1));

        var updated = result.Snapshot.Networks.Single();
        Assert.Equal(50m, updated.TotalDemand);
        Assert.Equal(50m, updated.TotalSupply);
        Assert.Equal(50m, updated.GetNode(consumer.Id)!.Supplied);
        Assert.Equal(50m, updated.GetEdge(junctionToConsumer.Id)!.Flow);
        Assert.Empty(result.Events);
    }

    [Fact]
    public void Advance_ReportsCapacityShortageWhenDemandExceedsSupply()
    {
        var source = new UtilityNode("source", UtilityType.Water, new GridPosition(0, 0), UtilityNodeRole.Source, 40m, 0m, 0m, true);
        var consumer = new UtilityNode("consumer", UtilityType.Water, new GridPosition(1, 0), UtilityNodeRole.Consumer, 0m, 100m, 0m, true);
        var edge = new UtilityEdge("edge", source.Id, consumer.Id, 100m, 0m, true);
        var network = new UtilityNetwork(UtilityType.Water, [source, consumer], [edge], 100m, 0m);
        var model = new SimpleInfrastructureModel(new InfrastructureSnapshot(new SimulationTime(0), [network], []));

        var result = model.Advance(new SimulationTime(1));

        var updated = result.Snapshot.Networks.Single();
        Assert.Equal(40m, updated.TotalSupply);
        Assert.Equal(60m, updated.Shortage);
        Assert.Contains(result.Events, e => e.Type == InfrastructureEventType.CapacityShortageStarted && e.UtilityType == UtilityType.Water);
    }

    [Fact]
    public void Advance_ReportsOutageAndRecovery()
    {
        var source = new UtilityNode("source", UtilityType.Telecom, new GridPosition(0, 0), UtilityNodeRole.Source, 100m, 0m, 0m, true);
        var consumer = new UtilityNode("consumer", UtilityType.Telecom, new GridPosition(1, 0), UtilityNodeRole.Consumer, 0m, 20m, 0m, true);
        var edge = new UtilityEdge("edge", source.Id, consumer.Id, 100m, 0m, true);
        var network = new UtilityNetwork(UtilityType.Telecom, [source, consumer], [edge], 20m, 0m);
        var model = new SimpleInfrastructureModel(new InfrastructureSnapshot(new SimulationTime(0), [network], []));

        var baseline = model.Advance(new SimulationTime(1));
        Assert.Empty(baseline.Events);

        model.SetEdgeOperational(edge.Id, false);
        var outage = model.Advance(new SimulationTime(2));
        Assert.Contains(outage.Events, e => e.Type == InfrastructureEventType.OutageStarted && e.TargetId == edge.Id);
        Assert.Equal(0m, outage.Snapshot.Networks.Single().TotalSupply);

        model.SetEdgeOperational(edge.Id, true);
        var recovery = model.Advance(new SimulationTime(3));
        Assert.Contains(recovery.Events, e => e.Type == InfrastructureEventType.OutageRecovered && e.TargetId == edge.Id);
        Assert.Equal(20m, recovery.Snapshot.Networks.Single().TotalSupply);
    }

    [Fact]
    public void Advance_LeavesDisconnectedConsumersUnserved()
    {
        var source = new UtilityNode("source", UtilityType.Fuel, new GridPosition(0, 0), UtilityNodeRole.Source, 100m, 0m, 0m, true);
        var connected = new UtilityNode("connected", UtilityType.Fuel, new GridPosition(1, 0), UtilityNodeRole.Consumer, 0m, 30m, 0m, true);
        var disconnected = new UtilityNode("disconnected", UtilityType.Fuel, new GridPosition(2, 0), UtilityNodeRole.Consumer, 0m, 30m, 0m, true);
        var edge = new UtilityEdge("edge", source.Id, connected.Id, 100m, 0m, true);
        var network = new UtilityNetwork(UtilityType.Fuel, [source, connected, disconnected], [edge], 60m, 0m);
        var model = new SimpleInfrastructureModel(new InfrastructureSnapshot(new SimulationTime(0), [network], []));

        var result = model.Advance(new SimulationTime(1));

        var updated = result.Snapshot.Networks.Single();
        Assert.Equal(30m, updated.GetNode(connected.Id)!.Supplied);
        Assert.Equal(0m, updated.GetNode(disconnected.Id)!.Supplied);
        Assert.Equal(30m, updated.Shortage);
    }
}
