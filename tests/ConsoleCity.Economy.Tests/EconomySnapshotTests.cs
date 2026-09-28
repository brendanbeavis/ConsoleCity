using ConsoleCity.Core;
using ConsoleCity.Economy;

namespace ConsoleCity.Economy.Tests;

public class EconomySnapshotTests
{
    [Fact]
    public void EconomySnapshot_StoresPricesRecipesAndIndicators()
    {
        var snapshot = new EconomySnapshot(
            new SimulationTime(24),
            [new PriceQuote("food", 10m, 12m)],
            [new ProductionRecipe("food-processing", [new InventoryLine("grain", 100)], [new InventoryLine("packaged-food", 80)])],
            new EconomicIndicators(0.92m, new Money(5000m), new Money(4200m), new Money(12000m), new Money(9000m), 1));

        Assert.Single(snapshot.Prices);
        Assert.Single(snapshot.Recipes);
        Assert.Equal(1, snapshot.Indicators.BusinessClosures);
    }
}
