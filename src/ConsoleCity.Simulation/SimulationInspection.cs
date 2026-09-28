namespace ConsoleCity.Simulation;

public sealed record class SimulationInspection
{
    public SimulationEngineState EngineState { get; }

    public IReadOnlyList<SimulationEventRecord> Events { get; }

    public SimulationInspection(SimulationEngineState engineState, IReadOnlyList<SimulationEventRecord> events)
    {
        ArgumentNullException.ThrowIfNull(engineState);
        ArgumentNullException.ThrowIfNull(events);
        EngineState = engineState;
        Events = events;
    }
}
