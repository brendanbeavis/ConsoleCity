using ConsoleCity.Core;

namespace ConsoleCity.Simulation;

public sealed class SimulationEngine : ISimulationEngine
{
    private readonly MutableSimulationClock clock;
    private readonly IReadOnlyList<ISimulationSystem> systems;
    private readonly SimulationContext context;
    private readonly SimulationEventProcessor eventProcessor = new();
    private readonly SimulationEngineOptions options;

    public SimulationMode Mode { get; private set; }

    public SimulationState State { get; private set; }

    public SimulationTime CurrentTime => clock.Now;

    public SimulationEngine(
        MutableSimulationClock clock,
        IReadOnlyList<ISimulationSystem> systems,
        SimulationContext context,
        SimulationMode mode = SimulationMode.Normal,
        SimulationEngineOptions? options = null)
    {
        this.clock = clock;
        this.systems = systems.OrderBy(system => system.Order).ThenBy(system => system.Name, StringComparer.Ordinal).ToList();
        this.options = options ?? new SimulationEngineOptions();
        this.context = this.options.Seed == 0
            ? context
            : context with { Random = new DeterministicRandomSource(this.options.Seed) };
        Mode = mode;
        State = SimulationState.Create(mode).WithTime(clock.Now);
    }

    public void Step(int ticks = 1)
    {
        if (Mode == SimulationMode.Paused)
        {
            return;
        }

        var totalTicks = Math.Max(1, ticks * options.Acceleration);
        for (var i = 0; i < totalTicks; i++)
        {
            clock.Advance();
            ExecuteSystems();
            State = State.WithTime(clock.Now).WithTicksProcessed(State.TicksProcessed + 1).WithMode(Mode);
        }
    }

    public void Pause() => SetMode(SimulationMode.Paused);

    public void Resume() => SetMode(SimulationMode.Normal);

    public void SetAcceleration(int acceleration)
    {
        if (acceleration <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(acceleration));
        }

        State = new SimulationState(Mode, State.CurrentTime, State.TicksProcessed, State.Events);
    }

    public void SetMode(SimulationMode mode)
    {
        Mode = mode;
        State = State.WithMode(mode);
    }

    public void EnqueueEvent(SimulationEvent simulationEvent)
    {
        eventProcessor.Enqueue(simulationEvent);
    }

    public IReadOnlyList<SimulationEventRecord> GetEventHistory() => eventProcessor.History;

    private void ExecuteSystems()
    {
        foreach (var system in systems)
        {
            if (system.TickInterval <= 1 || clock.Now.Tick % system.TickInterval == 0)
            {
                system.Execute(context);
            }
        }

        var processed = eventProcessor.Drain(clock.Now);
        if (processed.Count > 0)
        {
            State = State.WithEvents(State.Events.Concat(processed).ToList());
        }
    }
}
