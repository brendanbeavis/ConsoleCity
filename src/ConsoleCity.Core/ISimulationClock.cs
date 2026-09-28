namespace ConsoleCity.Core;

public interface ISimulationClock
{
    SimulationTime Now { get; }
}
