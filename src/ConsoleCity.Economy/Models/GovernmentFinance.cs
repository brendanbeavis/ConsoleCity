using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public sealed record GovernmentFinance
{
    public Money Revenue { get; }

    public Money Expenditure { get; }

    public Money Balance { get; }

    public GovernmentFinance(Money revenue, Money expenditure, Money balance)
    {
        Revenue = revenue;
        Expenditure = expenditure;
        Balance = balance;
    }

    public static GovernmentFinance Zero => new(Money.Zero, Money.Zero, Money.Zero);
}
