using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportRoute
{
    public EntityId Id { get; }

    public EntityId OriginNodeId { get; }

    public EntityId DestinationNodeId { get; }

    public IReadOnlyList<EntityId> NodeIds { get; }

    public IReadOnlyList<EntityId> LinkIds { get; }

    public TransportMode Mode { get; }

    public Distance Distance { get; }

    public TimeSpan EstimatedTravelTime { get; }

    public double EstimatedCost { get; }

    public TransportRoute(
        EntityId id,
        EntityId originNodeId,
        EntityId destinationNodeId,
        IReadOnlyList<EntityId> nodeIds,
        IReadOnlyList<EntityId> linkIds,
        TransportMode mode,
        Distance distance,
        TimeSpan estimatedTravelTime,
        double estimatedCost)
    {
        if (nodeIds.Count < 2)
        {
            throw new ArgumentException("A route must include at least an origin and destination node.", nameof(nodeIds));
        }

        if (linkIds.Count == 0)
        {
            throw new ArgumentException("A route must include at least one link.", nameof(linkIds));
        }

        if (!double.IsFinite(estimatedCost) || estimatedCost < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedCost));
        }

        Id = id;
        OriginNodeId = originNodeId;
        DestinationNodeId = destinationNodeId;
        NodeIds = nodeIds;
        LinkIds = linkIds;
        Mode = mode;
        Distance = distance;
        EstimatedTravelTime = estimatedTravelTime;
        EstimatedCost = estimatedCost;
    }
}
