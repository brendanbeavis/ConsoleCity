using ConsoleCity.Core;

namespace ConsoleCity.Simulation;

public sealed class MutableSimulationClock : ISimulationClock
{
    public SimulationTime Now { get; private set; } = new(0);

    public void Advance(long ticks = 1) => Now = Now.Advance(ticks);
}
