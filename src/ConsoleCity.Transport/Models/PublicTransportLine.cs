using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class PublicTransportLine
{
    public EntityId Id { get; }

    public TransportMode Mode { get; }

    public IReadOnlyList<EntityId> StopNodeIds { get; }

    public IReadOnlyList<EntityId> VehicleIds { get; }

    public TimeSpan Headway { get; }

    public PublicTransportLine(EntityId id, TransportMode mode, IReadOnlyList<EntityId> stopNodeIds, IReadOnlyList<EntityId> vehicleIds, TimeSpan headway)
    {
        ArgumentNullException.ThrowIfNull(stopNodeIds);
        ArgumentNullException.ThrowIfNull(vehicleIds);
        if (headway <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(headway));
        }

        Id = id;
        Mode = mode;
        StopNodeIds = stopNodeIds;
        VehicleIds = vehicleIds;
        Headway = headway;
    }
}
