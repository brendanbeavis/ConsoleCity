using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record class GoodsTransportRequest
{
    public EntityId Id { get; }

    public ResourceId ResourceId { get; }

    public Quantity Quantity { get; }

    public GridPosition? From { get; }

    public GridPosition? To { get; }

    public GoodsTransportRequest(EntityId id, ResourceId resourceId, Quantity quantity, GridPosition? from = null, GridPosition? to = null)
    {
        Id = id;
        ResourceId = resourceId;
        Quantity = quantity;
        From = from;
        To = to;
    }
}
