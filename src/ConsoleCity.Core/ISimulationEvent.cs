namespace ConsoleCity.Core;

public interface ISimulationEvent
{
    EntityId Id { get; }

    string Type { get; }

    SimulationTick StartedAt { get; }

    SimulationTick? Duration { get; }

    GridPosition? Location { get; }

    string? Description { get; }

    double? Severity { get; }
}
