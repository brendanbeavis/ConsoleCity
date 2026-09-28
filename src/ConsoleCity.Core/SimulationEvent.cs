namespace ConsoleCity.Core;

public readonly record struct SimulationEvent : ISimulationEvent
{
    public EntityId Id { get; }

    public string Type { get; }

    public SimulationTick StartedAt { get; }

    public SimulationTick? Duration { get; }

    public GridPosition? Location { get; }

    public string? Description { get; }

    public double? Severity { get; }

    public SimulationEvent(
        EntityId id,
        string type,
        SimulationTick startedAt,
        SimulationTick? duration = null,
        GridPosition? location = null,
        string? description = null,
        double? severity = null)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Event type cannot be empty.", nameof(type));
        }

        if (severity is double value && !double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(severity), "Severity must be finite.");
        }

        Id = id;
        Type = type.Trim();
        StartedAt = startedAt;
        Duration = duration;
        Location = location;
        Description = description;
        Severity = severity;
    }
}
