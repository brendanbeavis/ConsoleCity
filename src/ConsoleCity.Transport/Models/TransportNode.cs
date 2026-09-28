using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportNode
{
    public EntityId Id { get; }

    public GridPosition Position { get; }

    public TransportNodeType NodeType { get; }

    public TransportNode(EntityId id, GridPosition position, TransportNodeType nodeType)
    {
        Id = id;
        Position = position;
        NodeType = nodeType;
    }
}
