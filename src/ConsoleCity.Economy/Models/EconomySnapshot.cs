using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record class EconomySnapshot
{
    public SimulationTime CapturedAt { get; }

    public IReadOnlyList<BusinessModel> Businesses { get; }

    public IReadOnlyList<PriceQuote> Prices { get; }

    public IReadOnlyList<ProductionRecipe> Recipes { get; }

    public GovernmentFinance Government { get; }

    public EconomicIndicators Indicators { get; }

    public IReadOnlyList<EconomicEvent> Events { get; }

    public EconomySnapshot(
        SimulationTime capturedAt,
        IReadOnlyList<PriceQuote> prices,
        IReadOnlyList<ProductionRecipe> recipes,
        EconomicIndicators indicators,
        IReadOnlyList<BusinessModel>? businesses = null,
        GovernmentFinance? government = null,
        IReadOnlyList<EconomicEvent>? events = null)
    {
        ArgumentNullException.ThrowIfNull(prices);
        ArgumentNullException.ThrowIfNull(recipes);
        ArgumentNullException.ThrowIfNull(indicators);

        CapturedAt = capturedAt;
        Prices = prices;
        Recipes = recipes;
        Indicators = indicators;
        Businesses = businesses ?? Array.Empty<BusinessModel>();
        Government = government ?? GovernmentFinance.Zero;
        Events = events ?? Array.Empty<EconomicEvent>();
    }
}
