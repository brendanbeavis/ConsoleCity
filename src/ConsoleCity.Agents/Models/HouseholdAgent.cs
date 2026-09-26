using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public sealed record class HouseholdAgent
{
    public HouseholdId Id { get; }

    public PlotId? HomePlotId { get; }

    public GridPosition? HomeLocation { get; }

    public IReadOnlyList<PersonId> Members { get; }

    public Money Income { get; }

    public Money Expenses { get; }

    public Money Savings { get; }

    public Money Debt { get; }

    public Quantity FoodDemand { get; }

    public Quantity UtilityDemand { get; }

    public int TransportAssets { get; }

    public double Satisfaction { get; }

    public bool HasMember(PersonId personId) => Members.Contains(personId);

    public HouseholdAgent(
        HouseholdId id,
        PlotId? homePlotId,
        GridPosition? homeLocation,
        IReadOnlyList<PersonId> members,
        Money income,
        Money expenses,
        Money savings,
        Money debt,
        Quantity foodDemand,
        Quantity utilityDemand,
        int transportAssets,
        double satisfaction)
    {
        if (!double.IsFinite(satisfaction) || satisfaction is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(satisfaction), "Satisfaction must be a finite fraction between 0 and 1.");
        }

        ArgumentNullException.ThrowIfNull(members);

        if (members.Count == 0)
        {
            throw new ArgumentException("A household must contain at least one member.", nameof(members));
        }

        if (members.Distinct().Count() != members.Count)
        {
            throw new ArgumentException("A household cannot contain duplicate members.", nameof(members));
        }

        Id = id;
        HomePlotId = homePlotId;
        HomeLocation = homeLocation;
        Members = members;
        Income = income;
        Expenses = expenses;
        Savings = savings;
        Debt = debt;
        FoodDemand = foodDemand;
        UtilityDemand = utilityDemand;
        TransportAssets = transportAssets;
        Satisfaction = satisfaction;
    }
}
