using System;
using Xunit;
using ConsoleCity.Core;
using ConsoleCity.Infrastructure;
using ConsoleCity.Simulation;
using ConsoleCity.World;
using ConsoleCity.Agents;
using ConsoleCity.Economy;
using ConsoleCity.Transport;
using ConsoleCity.Services;

namespace ConsoleCity.Simulation.Tests;

public class InfrastructureIntegrationTests
{
    [Fact]
    public void InfrastructureSystem_MapsConsumerDemand_AndInfrastructureEvents_ArePublished()
    {
        var clock = new MutableSimulationClock();
        var infraSystem = new InfrastructureSimulationSystem();
        var systems = new ISimulationSystem[] { infraSystem };

        // Build a network with insufficient source capacity to trigger a shortage
        var source = new UtilityNode("source", UtilityType.Water, new GridPosition(0, 0), UtilityNodeRole.Source, 2m, 0m, 0m, true);
        var consumer = new UtilityNode("consumer", UtilityType.Water, new GridPosition(1, 0), UtilityNodeRole.Consumer, 0m, 0m, 0m, true);
        var edge = new UtilityEdge("edge", source.Id, consumer.Id, 10m, 0m, true);
        var network = new UtilityNetwork(UtilityType.Water, [source, consumer], [edge], 0m, 0m);
        var infraModel = new SimpleInfrastructureModel(new InfrastructureSnapshot(new SimulationTime(0), [network], []));

        // Register a consumer mapped to the network node and set demand greater than source capacity
        infraModel.RegisterConnection("building-1", consumer.Id);
        infraSystem.SetConsumerDemand("building-1", 5m);

        var context = new SimulationContext(
            clock,
            new TestWorldRepository(),
            new TestAgentModel(),
            new TestEconomyModel(),
            infraModel,
            new TestTransportModel(),
            new TestServiceModel(),
            new TestRandomSource());

        var engine = new SimulationEngine(clock, systems, context);

        engine.Step();

        // After step, infra model should have advanced and supplied what it can
        var updated = infraModel.Snapshot.Networks.Single();
        Assert.Equal(2m, updated.TotalSupply);
        Assert.Equal(3m, updated.Shortage);

        // Check simulation event history contains translated infrastructure event
        var history = engine.GetEventHistory();
        Assert.Contains(history, r => r.Event.Type.StartsWith("infrastructure.Water") );
    }

    // Minimal test doubles used only for constructing SimulationContext
    private sealed class TestWorldRepository : IWorldRepository
    {
        public WorldModel Current { get; } = new(
            WorldId.New(),
            42,
            new SimulationTime(0),
            [],
            [new TerrainCellModel(new GridPosition(0,0), TerrainType.Plains, 0, false, false, false, false, 1d, [], null, null, new SimulationTime(0))],
            EnvironmentalStateModel.Neutral);
    }

    private sealed class TestAgentModel : IAgentModel
    {
        public AgentPopulationSnapshot Snapshot { get; } = new(new SimulationTime(0), [], []);
    }

    private sealed class TestEconomyModel : IEconomyModel
    {
        public EconomySnapshot Snapshot { get; } = new(
            new SimulationTime(0),
            [],
            [],
            new EconomicIndicators(0m, Money.Zero, Money.Zero, Money.Zero, Money.Zero, 0));
    }

    private sealed class TestTransportModel : ITransportModel
    {
        public TransportSnapshot Snapshot { get; } = TransportSnapshot.Empty;
    }

    private sealed class TestServiceModel : IServiceModel
    {
        public ServicesSnapshot Snapshot { get; } = ServicesSnapshot.Empty;

        public void RegisterProvider(ServiceProvider provider) { }
        public ServiceResponse SubmitRequest(ServiceRequest request) => new(Guid.Empty, ServiceType.Other, false, 0, new SimulationTime(0), "", new ServiceCoverage(ServiceType.Other, new GridPosition(0,0), 0d, false, 0d));
        public ServicesSnapshot Advance(SimulationTime currentTime) => Snapshot;
    }

    private sealed class TestRandomSource : IRandomSource
    {
        public int Next(int minInclusive, int maxExclusive) => minInclusive;
        public double NextDouble() => 0.0;
    }
}
