using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record EconomicIndicators
{
    public decimal EmploymentRate { get; }

    public decimal UnemploymentRate => 1m - EmploymentRate;

    public Money AggregateHouseholdIncome { get; }

    public Money AggregateHouseholdExpenses { get; }

    public Money AggregateBusinessRevenue { get; }

    public Money AggregateBusinessCosts { get; }

    public int BusinessClosures { get; }

    public Money GovernmentRevenue { get; }

    public Money GovernmentExpenditure { get; }

    public Quantity ConsumerDemand { get; }

    public Quantity Production { get; }

    public Quantity Inventory { get; }

    public Money TradeBalance { get; }

    public EconomicIndicators(
        decimal employmentRate,
        Money aggregateHouseholdIncome,
        Money aggregateHouseholdExpenses,
        Money aggregateBusinessRevenue,
        Money aggregateBusinessCosts,
        int businessClosures,
        Money? governmentRevenue = null,
        Money? governmentExpenditure = null,
        Quantity? consumerDemand = null,
        Quantity? production = null,
        Quantity? inventory = null,
        Money? tradeBalance = null)
    {
        if (employmentRate < 0m || employmentRate > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(employmentRate));
        }

        EmploymentRate = employmentRate;
        AggregateHouseholdIncome = aggregateHouseholdIncome;
        AggregateHouseholdExpenses = aggregateHouseholdExpenses;
        AggregateBusinessRevenue = aggregateBusinessRevenue;
        AggregateBusinessCosts = aggregateBusinessCosts;
        BusinessClosures = businessClosures;
        GovernmentRevenue = governmentRevenue ?? Money.Zero;
        GovernmentExpenditure = governmentExpenditure ?? Money.Zero;
        ConsumerDemand = consumerDemand ?? Quantity.Zero;
        Production = production ?? Quantity.Zero;
        Inventory = inventory ?? Quantity.Zero;
        TradeBalance = tradeBalance ?? Money.Zero;
    }
}
