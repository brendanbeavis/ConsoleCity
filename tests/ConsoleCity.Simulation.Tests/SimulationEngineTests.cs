using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.Infrastructure;
using ConsoleCity.Services;
using ConsoleCity.Simulation;
using ConsoleCity.Transport;
using ConsoleCity.World;

namespace ConsoleCity.Simulation.Tests;

public class SimulationEngineTests
{
    [Fact]
    public void Step_AdvancesClock_AndExecutesSystemsInOrder()
    {
        var clock = new MutableSimulationClock();
        var executionOrder = new List<string>();
        var systems = new ISimulationSystem[]
        {
            new TestSystem("Second", tickInterval: 1, order: 2, executionOrder),
            new TestSystem("First", tickInterval: 1, order: 1, executionOrder)
        };

        var context = new SimulationContext(
            clock,
            new TestWorldRepository(),
            new TestAgentModel(),
            new TestEconomyModel(),
            new TestInfrastructureModel(),
            new TestTransportModel(),
            new TestServiceModel(),
            new TestRandomSource());

        var engine = new SimulationEngine(clock, systems, context);

        engine.Step();

        Assert.Equal(1, engine.CurrentTime.Tick);
        Assert.Equal(["First", "Second"], executionOrder);
    }

    private sealed class TestSystem(string name, int tickInterval, int order, List<string> executionOrder) : ISimulationSystem
    {
        public string Name => name;
        public int TickInterval => tickInterval;
        public int Order => order;

        public void Execute(ISimulationContext context) => executionOrder.Add(name);
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
            [new TerrainCellModel(
                new GridPosition(0, 0),
                TerrainType.Plains,
                0,
                false,
                false,
                false,
                false,
                1d,
                [],
                null,
                null,
                new SimulationTime(0))],
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

        public void RegisterProvider(ServiceProvider provider)
        {
        }

        public ServiceResponse SubmitRequest(ServiceRequest request) => new(Guid.Empty, ServiceType.Other, false, 0, new SimulationTime(0), "", new ServiceCoverage(ServiceType.Other, new GridPosition(0, 0), 0d, false, 0d));

        public ServicesSnapshot Advance(SimulationTime currentTime) => Snapshot;
    }
}
