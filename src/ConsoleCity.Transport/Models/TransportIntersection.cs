using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportIntersection
{
    public EntityId Id { get; }

    public GridPosition Position { get; }

    public IReadOnlyList<EntityId> ConnectedLinkIds { get; }

    public TransportIntersection(EntityId id, GridPosition position, IReadOnlyList<EntityId> connectedLinkIds)
    {
        ArgumentNullException.ThrowIfNull(connectedLinkIds);
        Id = id;
        Position = position;
        ConnectedLinkIds = connectedLinkIds;
    }
}
