using ConsoleCity.Core;

namespace ConsoleCity.Services;

/// <summary>
/// Represents an event that occurs in the services system.
/// </summary>
public sealed record class ServiceEvent : ISimulationEvent
{
    public EntityId Id { get; }

    public ServiceEventType EventType { get; }

    public string Type => EventType.ToString();

    public SimulationTick StartedAt { get; }

    public SimulationTick? Duration { get; }

    public GridPosition? Location { get; }

    public string? Description { get; }

    public double? Severity { get; }

    public ServiceType ServiceType { get; }

    public BuildingId? FacilityId { get; }

    public EntityId? AffectedEntityId { get; }

    public ServiceEvent(
        ServiceEventType eventType,
        ServiceType serviceType,
        SimulationTick startedAt,
        string? description = null,
        double? severity = null,
        BuildingId? facilityId = null,
        EntityId? affectedEntityId = null,
        GridPosition? location = null,
        SimulationTick? duration = null,
        EntityId? id = null)
    {
        if (severity is double value && !double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(severity), "Severity must be finite.");
        }

        Id = id ?? EntityId.New();
        EventType = eventType;
        ServiceType = serviceType;
        StartedAt = startedAt;
        Description = description;
        Severity = severity;
        FacilityId = facilityId;
        AffectedEntityId = affectedEntityId;
        Location = location;
        Duration = duration;
    }
}
