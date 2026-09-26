using ConsoleCity.Core;

namespace ConsoleCity.Simulation;

public sealed record class SimulationState
{
    public SimulationMode Mode { get; }

    public SimulationTime CurrentTime { get; }

    public long TicksProcessed { get; }

    public bool IsPaused => Mode == SimulationMode.Paused;

    public IReadOnlyList<SimulationEventRecord> Events { get; }

    public SimulationState(SimulationMode mode, SimulationTime currentTime, long ticksProcessed, IReadOnlyList<SimulationEventRecord> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        Mode = mode;
        CurrentTime = currentTime;
        TicksProcessed = ticksProcessed;
        Events = events;
    }

    public static SimulationState Create(SimulationMode mode) => new(mode, new SimulationTime(0), 0, Array.Empty<SimulationEventRecord>());

    public SimulationState WithMode(SimulationMode mode) => new(mode, CurrentTime, TicksProcessed, Events);

    public SimulationState WithTime(SimulationTime currentTime) => new(Mode, currentTime, TicksProcessed, Events);

    public SimulationState WithTicksProcessed(long ticksProcessed) => new(Mode, CurrentTime, ticksProcessed, Events);

    public SimulationState WithEvents(IReadOnlyList<SimulationEventRecord> events) => new(Mode, CurrentTime, TicksProcessed, events);
}
