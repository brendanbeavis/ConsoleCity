using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public static class AgentStateEffects
{
    public static AgentTransitionResult Apply(PersonAgent person, HouseholdAgent household, AgentAction action)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentNullException.ThrowIfNull(household);

        return action.ActionType switch
        {
            AgentActionType.BuyFood => ApplyBuyFood(person, household, action.Reason),
            AgentActionType.ReturnHome => ApplyReturnHome(person, household, action.Reason),
            AgentActionType.SeekEmployment => ApplySeekEmployment(person, household, action.Reason),
            AgentActionType.GoToWork => ApplyGoToWork(person, household, action.Reason),
            AgentActionType.Socialise => ApplySocialise(person, household, action.Reason),
            AgentActionType.Rest => ApplyRest(person, household, action.Reason),
            AgentActionType.Study => ApplyStudy(person, household, action.Reason),
            AgentActionType.MoveHouse => ApplyMoveHouse(person, household, action.Reason),
            _ => new AgentTransitionResult(person, household, "No effect")
        };
    }

    private static AgentTransitionResult ApplyBuyFood(PersonAgent person, HouseholdAgent household, string reason)
    {
        var needs = ReplaceNeed(person.Needs, NeedType.Food, 1d);
        var updatedPerson = CreatePerson(
            person,
            needs: needs,
            balance: person.Balance - new Money(5m),
            expenses: person.Expenses + new Money(5m),
            currentActivity: AgentActivity.BuyingFood,
            satisfaction: Clamp01(person.Satisfaction + 0.05d));

        var updatedHousehold = CreateHousehold(
            household,
            foodDemand: ReduceDemand(household.FoodDemand, 1m),
            expenses: household.Expenses + new Money(5m),
            satisfaction: Clamp01(household.Satisfaction + 0.02d));

        return new AgentTransitionResult(updatedPerson, updatedHousehold, reason);
    }

    private static AgentTransitionResult ApplyReturnHome(PersonAgent person, HouseholdAgent household, string reason)
    {
        var homeLocation = household.HomeLocation ?? person.CurrentLocation;
        var updatedPerson = CreatePerson(
            person,
            currentLocation: homeLocation,
            currentActivity: AgentActivity.AtHome,
            satisfaction: Clamp01(person.Satisfaction + 0.03d));

        return new AgentTransitionResult(updatedPerson, household, reason);
    }

    private static AgentTransitionResult ApplySeekEmployment(PersonAgent person, HouseholdAgent household, string reason)
    {
        var employment = new EmploymentRecord(EmploymentState.Seeking, person.WorkplaceId, person.WorkplaceLocation, person.Employment.Wage);
        var updatedPerson = CreatePerson(
            person,
            employment: employment,
            currentActivity: AgentActivity.SeekingWork,
            satisfaction: Clamp01(person.Satisfaction + 0.01d));

        return new AgentTransitionResult(updatedPerson, household, reason);
    }

    private static AgentTransitionResult ApplyGoToWork(PersonAgent person, HouseholdAgent household, string reason)
    {
        var wage = person.Employment.Wage;
        var updatedPerson = CreatePerson(
            person,
            balance: person.Balance + wage,
            income: person.Income + wage,
            currentLocation: person.WorkplaceLocation ?? person.CurrentLocation,
            currentActivity: AgentActivity.Working,
            satisfaction: Clamp01(person.Satisfaction + 0.02d));

        var updatedHousehold = CreateHousehold(
            household,
            income: household.Income + wage,
            satisfaction: Clamp01(household.Satisfaction + 0.01d));

        return new AgentTransitionResult(updatedPerson, updatedHousehold, reason);
    }

    private static AgentTransitionResult ApplySocialise(PersonAgent person, HouseholdAgent household, string reason)
    {
        var needs = ReplaceNeed(person.Needs, NeedType.Social, Clamp01(GetNeed(person.Needs, NeedType.Social).Fulfillment + 0.2d));
        var updatedPerson = CreatePerson(
            person,
            needs: needs,
            currentActivity: AgentActivity.Socialising,
            satisfaction: Clamp01(person.Satisfaction + 0.04d));

        return new AgentTransitionResult(updatedPerson, household, reason);
    }

    private static AgentTransitionResult ApplyRest(PersonAgent person, HouseholdAgent household, string reason)
    {
        var updatedPerson = CreatePerson(
            person,
            currentActivity: AgentActivity.Resting,
            satisfaction: Clamp01(person.Satisfaction + 0.02d));

        return new AgentTransitionResult(updatedPerson, household, reason);
    }

    private static AgentTransitionResult ApplyStudy(PersonAgent person, HouseholdAgent household, string reason)
    {
        var needs = ReplaceNeed(person.Needs, NeedType.Education, Clamp01(GetNeed(person.Needs, NeedType.Education).Fulfillment + 0.15d));
        var updatedPerson = CreatePerson(
            person,
            needs: needs,
            currentActivity: AgentActivity.Idle,
            satisfaction: Clamp01(person.Satisfaction + 0.02d));

        return new AgentTransitionResult(updatedPerson, household, reason);
    }

    private static AgentTransitionResult ApplyMoveHouse(PersonAgent person, HouseholdAgent household, string reason)
    {
        var updatedPerson = CreatePerson(person, currentActivity: AgentActivity.Travelling);
        return new AgentTransitionResult(updatedPerson, household, reason);
    }

    private static PersonAgent CreatePerson(
        PersonAgent source,
        Money? income = null,
        Money? expenses = null,
        Money? balance = null,
        PlotId? residencePlotId = null,
        GridPosition? currentLocation = null,
        AgentActivity? currentActivity = null,
        double? satisfaction = null,
        IReadOnlyList<NeedStatus>? needs = null,
        EmploymentRecord? employment = null)
    {
        return new PersonAgent(
            source.Id,
            source.HouseholdId,
            source.DisplayName,
            source.Age,
            source.LifeStage,
            employment ?? source.Employment,
            income ?? source.Income,
            expenses ?? source.Expenses,
            balance ?? source.Balance,
            residencePlotId ?? source.ResidencePlotId,
            currentLocation ?? source.CurrentLocation,
            currentActivity ?? source.CurrentActivity,
            source.TransportPreference,
            satisfaction ?? source.Satisfaction,
            source.Attributes,
            needs ?? source.Needs,
            source.Skills,
            source.Relationships,
            source.Goals,
            source.Routine);
    }

    private static HouseholdAgent CreateHousehold(
        HouseholdAgent source,
        Money? income = null,
        Money? expenses = null,
        Money? savings = null,
        Money? debt = null,
        Quantity? foodDemand = null,
        Quantity? utilityDemand = null,
        int? transportAssets = null,
        double? satisfaction = null)
    {
        return new HouseholdAgent(
            source.Id,
            source.HomePlotId,
            source.HomeLocation,
            source.Members,
            income ?? source.Income,
            expenses ?? source.Expenses,
            savings ?? source.Savings,
            debt ?? source.Debt,
            foodDemand ?? source.FoodDemand,
            utilityDemand ?? source.UtilityDemand,
            transportAssets ?? source.TransportAssets,
            satisfaction ?? source.Satisfaction);
    }

    private static IReadOnlyList<NeedStatus> ReplaceNeed(IReadOnlyList<NeedStatus> needs, NeedType needType, double fulfillment)
    {
        var updated = needs.Select(need => need.Need == needType ? new NeedStatus(needType, fulfillment) : need).ToList();
        if (updated.All(need => need.Need != needType))
        {
            updated.Add(new NeedStatus(needType, fulfillment));
        }

        return updated;
    }

    private static NeedStatus GetNeed(IReadOnlyList<NeedStatus> needs, NeedType needType)
        => needs.FirstOrDefault(need => need.Need == needType) ?? new NeedStatus(needType, 1d);

    private static Quantity ReduceDemand(Quantity demand, decimal amount)
        => demand.Value <= amount ? Quantity.Zero : new Quantity(demand.Value - amount);

    private static double Clamp01(double value) => Math.Clamp(value, 0d, 1d);
}
