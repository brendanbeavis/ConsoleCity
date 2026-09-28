namespace ConsoleCity.Core;

public interface ISimulationContext
{
    ISimulationClock Clock { get; }

    IRandomSource Random { get; }
}
