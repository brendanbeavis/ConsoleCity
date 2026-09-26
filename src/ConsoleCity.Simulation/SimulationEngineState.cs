namespace ConsoleCity.Simulation;

public sealed record class SimulationEngineState
{
    public SimulationState SimulationState { get; }

    public SimulationEngineOptions Options { get; }

    public SimulationEngineState(SimulationState simulationState, SimulationEngineOptions options)
    {
        ArgumentNullException.ThrowIfNull(simulationState);
        ArgumentNullException.ThrowIfNull(options);
        SimulationState = simulationState;
        Options = options;
    }
}
