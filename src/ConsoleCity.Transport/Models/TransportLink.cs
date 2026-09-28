using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportLink
{
    public EntityId Id { get; }

    public EntityId FromNodeId { get; }

    public EntityId ToNodeId { get; }

    public TransportMode Mode { get; }

    public Distance Length { get; }

    public double SpeedKph { get; }

    public TransportCapacity Capacity { get; }

    public double Condition { get; }

    public TransportLink(
        EntityId id,
        EntityId fromNodeId,
        EntityId toNodeId,
        TransportMode mode,
        Distance length,
        double speedKph,
        TransportCapacity capacity,
        double condition)
    {
        if (!double.IsFinite(speedKph) || speedKph <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(speedKph));
        }

        if (!double.IsFinite(condition) || condition is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(condition));
        }

        Id = id;
        FromNodeId = fromNodeId;
        ToNodeId = toNodeId;
        Mode = mode;
        Length = length;
        SpeedKph = speedKph;
        Capacity = capacity;
        Condition = condition;
    }
}
