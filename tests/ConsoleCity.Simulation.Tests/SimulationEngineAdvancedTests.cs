using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.Infrastructure;
using ConsoleCity.Services;
using ConsoleCity.Simulation;
using ConsoleCity.Transport;
using ConsoleCity.World;

namespace ConsoleCity.Simulation.Tests;

public class SimulationEngineAdvancedTests
{
    [Fact]
    public void Step_WithAcceleration_AdvancesMultipleTicks()
    {
        var clock = new MutableSimulationClock();
        var systems = new[] { new CountingSystem() };
        var context = CreateContext(clock);
        var engine = new SimulationEngine(clock, systems, context, SimulationMode.Normal, new SimulationEngineOptions { Acceleration = 2, Seed = 123 });

        engine.Step();

        Assert.Equal(2, engine.CurrentTime.Tick);
        Assert.Equal(2, engine.State.TicksProcessed);
        Assert.Equal(2, systems[0].Count);
    }

    [Fact]
    public void Pause_PreventsAdvancement()
    {
        var clock = new MutableSimulationClock();
        var systems = new[] { new CountingSystem() };
        var context = CreateContext(clock);
        var engine = new SimulationEngine(clock, systems, context);

        engine.Pause();
        engine.Step();

        Assert.Equal(0, engine.CurrentTime.Tick);
        Assert.Equal(0, systems[0].Count);
        Assert.True(engine.State.IsPaused);
    }

    [Fact]
    public void EventHistory_TracksProcessedEvents()
    {
        var clock = new MutableSimulationClock();
        var systems = Array.Empty<ISimulationSystem>();
        var context = CreateContext(clock);
        var engine = new SimulationEngine(clock, systems, context);
        var ev = new SimulationEvent(EntityId.New(), "TestEvent", new SimulationTick(0));

        engine.EnqueueEvent(ev);
        engine.Step();

        Assert.Single(engine.GetEventHistory());
        Assert.Equal("TestEvent", engine.GetEventHistory()[0].Event.Type);
        Assert.True(engine.GetEventHistory()[0].Processed);
    }

    [Fact]
    public void Step_MultipleDays_RemainsDeterministic()
    {
        var clock = new MutableSimulationClock();
        var systems = new[] { new CountingSystem() };
        var context = CreateContext(clock);
        var engine = new SimulationEngine(clock, systems, context, SimulationMode.Normal, new SimulationEngineOptions { Seed = 7 });

        for (var day = 0; day < 3; day++)
        {
            for (var hour = 0; hour < SimulationTime.TicksPerDay; hour++)
            {
                engine.Step();
            }
        }

        Assert.Equal(72, engine.CurrentTime.Tick);
        Assert.Equal(72, engine.State.TicksProcessed);
        Assert.Equal(72, systems[0].Count);
    }

    private static SimulationContext CreateContext(MutableSimulationClock clock)
        => new(
            clock,
            new TestWorldRepository(),
            new TestAgentModel(),
            new TestEconomyModel(),
            new TestInfrastructureModel(),
            new TestTransportModel(),
            new TestServiceModel(),
            new TestRandomSource());

    private sealed class CountingSystem : ISimulationSystem
    {
        public string Name => "Counting";
        public int TickInterval => 1;
        public int Order => 1;
        public int Count { get; private set; }
        public void Execute(ISimulationContext context) => Count++;
    }

    private sealed class TestRandomSource : IRandomSource
    {
        public int Next(int minInclusive, int maxExclusive) => minInclusive;
        public double NextDouble() => 0.0;
    }

    private sealed class TestWorldRepository : IWorldRepository
    {
        public WorldModel Current { get; } = new(
            WorldId.New(),
            42,
            new SimulationTime(0),
            [],
            [new TerrainCellModel(new GridPosition(0, 0), TerrainType.Plains, 0, false, false, false, false, 1d, [], null, null, new SimulationTime(0))],
            EnvironmentalStateModel.Neutral);
    }

    private sealed class TestAgentModel : IAgentModel
    {
        public AgentPopulationSnapshot Snapshot { get; } = new(new SimulationTime(0), [], []);
    }

    private sealed class TestEconomyModel : IEconomyModel
    {
        public EconomySnapshot Snapshot { get; } = new(new SimulationTime(0), [], [], new EconomicIndicators(0m, Money.Zero, Money.Zero, Money.Zero, Money.Zero, 0));
    }

    private sealed class TestInfrastructureModel : IInfrastructureModel
    {
        public InfrastructureSnapshot Snapshot { get; } = new(new SimulationTime(0), [], []);
    }

    private sealed class TestTransportModel : ITransportModel
    {
        public TransportSnapshot Snapshot { get; } = TransportSnapshot.Empty;
    }

    private sealed class TestServiceModel : IServiceModel
    {
        public ServicesSnapshot Snapshot { get; } = ServicesSnapshot.Empty;
        public void RegisterProvider(ServiceProvider provider) { }
        public ServiceResponse SubmitRequest(ServiceRequest request) => new(Guid.Empty, ServiceType.Other, false, 0, new SimulationTime(0), "", new ServiceCoverage(ServiceType.Other, new GridPosition(0, 0), 0d, false, 0d));
        public ServicesSnapshot Advance(SimulationTime currentTime) => Snapshot;
    }
}
