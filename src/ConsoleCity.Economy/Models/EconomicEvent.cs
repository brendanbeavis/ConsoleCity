using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Economy;

public sealed record class EconomicEvent : ISimulationEvent
{
    public EntityId Id { get; }

    public EconomicEventType EventType { get; }

    public string Type => EventType.ToString();

    public SimulationTick StartedAt { get; }

    public SimulationTick? Duration { get; }

    public GridPosition? Location { get; }

    public string? Description { get; }

    public double? Severity { get; }

    public OrganizationId? BusinessId { get; }

    public PersonId? PersonId { get; }

    public HouseholdId? HouseholdId { get; }

    public ResourceId? ResourceId { get; }

    public Money? Amount { get; }

    public EconomicEvent(
        EconomicEventType eventType,
        SimulationTick startedAt,
        string? description = null,
        double? severity = null,
        OrganizationId? businessId = null,
        PersonId? personId = null,
        HouseholdId? householdId = null,
        ResourceId? resourceId = null,
        Money? amount = null,
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
        StartedAt = startedAt;
        Description = description;
        Severity = severity;
        BusinessId = businessId;
        PersonId = personId;
        HouseholdId = householdId;
        ResourceId = resourceId;
        Amount = amount;
        Location = location;
        Duration = duration;
    }
}
