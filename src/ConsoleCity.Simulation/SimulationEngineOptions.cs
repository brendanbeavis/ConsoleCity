namespace ConsoleCity.Simulation;

public sealed record class SimulationEngineOptions
{
    public int TicksPerStep { get; init; } = 1;

    public int Acceleration { get; init; } = 1;

    public long Seed { get; init; } = 0;
}
