using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record class GoodsInTransit
{
    public EntityId JourneyId { get; }

    public ResourceId ResourceId { get; }

    public Quantity Quantity { get; }

    public GridPosition? Destination { get; }

    public GoodsInTransit(EntityId journeyId, ResourceId resourceId, Quantity quantity, GridPosition? destination = null)
    {
        JourneyId = journeyId;
        ResourceId = resourceId;
        Quantity = quantity;
        Destination = destination;
    }
}
