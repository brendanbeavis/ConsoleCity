namespace ConsoleCity.Core;

public interface ISimulationSystem
{
    string Name { get; }

    int TickInterval { get; }

    int Order { get; }

    void Execute(ISimulationContext context);
}
