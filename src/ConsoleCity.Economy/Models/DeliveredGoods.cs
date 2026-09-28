using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record class DeliveredGoods
{
    public EntityId JourneyId { get; }

    public ResourceId ResourceId { get; }

    public Quantity Quantity { get; }

    public GridPosition? Destination { get; }

    public DeliveredGoods(EntityId journeyId, ResourceId resourceId, Quantity quantity, GridPosition? destination = null)
    {
        JourneyId = journeyId;
        ResourceId = resourceId;
        Quantity = quantity;
        Destination = destination;
    }
}
