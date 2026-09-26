using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record class PriceQuote
{
    public ResourceId ResourceId { get; }

    public Money BasePrice { get; }

    public Money CurrentPrice { get; }

    public Quantity Supply { get; }

    public Quantity Demand { get; }

    public PriceQuote(ResourceId resourceId, Money basePrice, Money currentPrice, Quantity supply, Quantity demand)
    {
        if (basePrice.Amount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(basePrice));
        }

        if (currentPrice.Amount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(currentPrice));
        }

        ResourceId = resourceId;
        BasePrice = basePrice;
        CurrentPrice = currentPrice;
        Supply = supply;
        Demand = demand;
    }

    public PriceQuote(string resourceKey, decimal basePrice, decimal currentPrice)
        : this(new ResourceId(resourceKey), new Money(basePrice), new Money(currentPrice), Quantity.Zero, Quantity.Zero)
    {
    }

    public PriceQuote WithMarketState(Quantity supply, Quantity demand)
    {
        var demandPressure = demand.Value + 1m;
        var supplyPressure = supply.Value + 1m;
        var adjusted = BasePrice.Amount * (demandPressure / supplyPressure);
        return new PriceQuote(ResourceId, BasePrice, new Money(adjusted), supply, demand);
    }
}
