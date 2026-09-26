using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record class InventoryLine
{
    public ResourceId ResourceId { get; }

    public Quantity Quantity { get; }

    public InventoryLine(ResourceId resourceId, Quantity quantity)
    {
        ResourceId = resourceId;
        Quantity = quantity;
    }

    public InventoryLine(string resourceKey, decimal quantity)
        : this(new ResourceId(resourceKey), new Quantity(quantity))
    {
    }
}
