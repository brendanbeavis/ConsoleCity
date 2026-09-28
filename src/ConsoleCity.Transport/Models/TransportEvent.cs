using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportEvent : ISimulationEvent
{
    public EntityId Id { get; }

    public string Type { get; }

    public SimulationTick StartedAt { get; }

    public SimulationTick? Duration { get; }

    public GridPosition? Location { get; }

    public string? Description { get; }

    public double? Severity { get; }

    public TransportEvent(string type, SimulationTick startedAt, string? description = null, double? severity = null, GridPosition? location = null, SimulationTick? duration = null, EntityId? id = null)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Event type cannot be empty.", nameof(type));
        }

        if (severity is double value && !double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(severity));
        }

        Id = id ?? EntityId.New();
        Type = type.Trim();
        StartedAt = startedAt;
        Description = description;
        Severity = severity;
        Location = location;
        Duration = duration;
    }
}
