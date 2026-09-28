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

public class InfrastructureSystemTests
{
    [Fact]
    public void InfrastructureSystem_AdvancesSimpleInfrastructureModel_WhenEngineSteps()
    {
        var clock = new MutableSimulationClock();
        var systems = new ISimulationSystem[]
        {
            new InfrastructureSimulationSystem()
        };

        // Create a minimal network so the model has a snapshot to advance
        var source = new UtilityNode("source", UtilityType.Electricity, new GridPosition(0, 0), UtilityNodeRole.Source, 10m, 0m, 0m, true);
        var consumer = new UtilityNode("consumer", UtilityType.Electricity, new GridPosition(1, 0), UtilityNodeRole.Consumer, 0m, 5m, 0m, true);
        var edge = new UtilityEdge("edge", source.Id, consumer.Id, 10m, 0m, true);
        var network = new UtilityNetwork(UtilityType.Electricity, [source, consumer], [edge], 5m, 0m);
        var infraModel = new SimpleInfrastructureModel(new InfrastructureSnapshot(new SimulationTime(0), [network], []));

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

        Assert.Equal(0, infraModel.Snapshot.CapturedAt.Tick);

        engine.Step();

        Assert.Equal(1, engine.CurrentTime.Tick);
        Assert.Equal(1, infraModel.Snapshot.CapturedAt.Tick);
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
