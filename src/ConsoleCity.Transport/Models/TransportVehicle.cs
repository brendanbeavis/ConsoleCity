using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportVehicle
{
    public EntityId Id { get; }

    public TransportVehicleType VehicleType { get; }

    public TransportMode Mode { get; }

    public int Capacity { get; }

    public GridPosition Location { get; }

    public bool InService { get; }

    public EntityId? AssignedJourneyId { get; }

    public TransportVehicle(EntityId id, TransportVehicleType vehicleType, TransportMode mode, int capacity, GridPosition location, bool inService, EntityId? assignedJourneyId = null)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        Id = id;
        VehicleType = vehicleType;
        Mode = mode;
        Capacity = capacity;
        Location = location;
        InService = inService;
        AssignedJourneyId = assignedJourneyId;
    }
}
