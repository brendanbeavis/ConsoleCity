using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportTravelDemand
{
    public EntityId Id { get; }

    public EntityId OriginNodeId { get; }

    public EntityId DestinationNodeId { get; }

    public TransportMode PreferredMode { get; }

    public double Priority { get; }

    public int PassengerCount { get; }

    public decimal FreightVolume { get; }

    public EntityId? RequesterId { get; }

    public TransportTravelDemand(
        EntityId id,
        EntityId originNodeId,
        EntityId destinationNodeId,
        TransportMode preferredMode,
        double priority,
        int passengerCount,
        decimal freightVolume,
        EntityId? requesterId = null)
    {
        if (!double.IsFinite(priority) || priority is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(priority));
        }

        if (passengerCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(passengerCount));
        }

        if (freightVolume < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(freightVolume));
        }

        Id = id;
        OriginNodeId = originNodeId;
        DestinationNodeId = destinationNodeId;
        PreferredMode = preferredMode;
        Priority = priority;
        PassengerCount = passengerCount;
        FreightVolume = freightVolume;
        RequesterId = requesterId;
    }
}
