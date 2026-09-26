using ConsoleCity.Agents;
using ConsoleCity.Core;

namespace ConsoleCity.Agents.Tests;

public class AgentModelTests
{
    [Fact]
    public void LifeStage_FromAge_MapsExpectedRanges()
    {
        Assert.Equal(LifeStage.Child, LifeStageExtensions.FromAge(8));
        Assert.Equal(LifeStage.Teenager, LifeStageExtensions.FromAge(15));
        Assert.Equal(LifeStage.YoungAdult, LifeStageExtensions.FromAge(20));
        Assert.Equal(LifeStage.Adult, LifeStageExtensions.FromAge(40));
        Assert.Equal(LifeStage.Senior, LifeStageExtensions.FromAge(75));
    }

    [Fact]
    public void NeedStatus_RejectsInvalidFulfillment()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new NeedStatus(NeedType.Food, 1.2d));
    }

    [Fact]
    public void HouseholdAgent_RejectsEmptyOrDuplicateMembers()
    {
        var member = PersonId.New();

        Assert.Throws<ArgumentException>(() => new HouseholdAgent(
            HouseholdId.New(),
            null,
            null,
            [],
            Money.Zero,
            Money.Zero,
            Money.Zero,
            Money.Zero,
            new Quantity(0m),
            new Quantity(0m),
            0,
            0.5d));

        Assert.Throws<ArgumentException>(() => new HouseholdAgent(
            HouseholdId.New(),
            null,
            null,
            [member, member],
            Money.Zero,
            Money.Zero,
            Money.Zero,
            Money.Zero,
            new Quantity(0m),
            new Quantity(0m),
            0,
            0.5d));
    }

    [Fact]
    public void PersonAgent_HoldsIdentityStateAndRelationships()
    {
        var household = CreateHousehold(out var personId);
        var person = CreatePerson(personId, household.Id);

        Assert.Equal(personId, person.Id);
        Assert.Equal(household.Id, person.HouseholdId);
        Assert.Equal(LifeStage.Adult, person.LifeStage);
        Assert.Equal(5, person.Needs.Count);
        Assert.Single(person.Skills);
        Assert.Single(person.Relationships);
        Assert.Single(person.Goals);
        Assert.Equal(AgentActivity.AtHome, person.CurrentActivity);
    }

    [Fact]
    public void SimpleAgentDecisionPolicy_PrioritisesFoodWhenHungry()
    {
        var household = CreateHousehold(out var personId, foodDemand: new Quantity(3m));
        var person = CreatePerson(personId, household.Id, foodFulfillment: 0.1d, socialFulfillment: 1d, recreationFulfillment: 1d);
        var policy = new SimpleAgentDecisionPolicy();

        var decision = policy.Decide(person, household, new SimulationTime(12), new FixedRandomSource(0));

        Assert.Equal(AgentActionType.BuyFood, decision.ActionType);
    }

    [Fact]
    public void SimpleAgentDecisionPolicy_PrioritisesWorkDuringWorkHours()
    {
        var household = CreateHousehold(out var personId);
        var person = CreatePerson(
            personId,
            household.Id,
            employmentState: EmploymentState.Employed,
            currentActivity: AgentActivity.Idle,
            workplaceLocation: new GridPosition(9, 9));
        var policy = new SimpleAgentDecisionPolicy();

        var decision = policy.Decide(person, household, new SimulationTime(9), new FixedRandomSource(0));

        Assert.Equal(AgentActionType.GoToWork, decision.ActionType);
    }

    [Fact]
    public void SimpleAgentDecisionPolicy_UsesDeterministicTieBreakers()
    {
        var household = CreateHousehold(out var personId, includeHomeLocation: false, foodDemand: new Quantity(0m));
        var person = CreatePerson(
            personId,
            household.Id,
            1d,
            1d,
            1d,
            1d,
            1d,
            EmploymentState.Unemployed,
            new GridPosition(4, 4),
            null,
            global::ConsoleCity.Agents.AgentActivity.Idle,
            10,
            LifeStage.Child,
            attributes: new AgentAttributeProfile(1d, 1d, 0d, 0d));
        var policy = new SimpleAgentDecisionPolicy();

        var decision = policy.Decide(person, household, new SimulationTime(3), new FixedRandomSource(1));

        Assert.Equal(AgentActionType.ReturnHome, decision.ActionType);
    }

    [Fact]
    public void AgentStateEffects_BuyFood_UpdatesNeedsMoneyAndHouseholdDemand()
    {
        var household = CreateHousehold(out var personId, foodDemand: new Quantity(3m));
        var person = CreatePerson(personId, household.Id, foodFulfillment: 0.2d, balance: new Money(20m), expenses: new Money(5m));

        var result = AgentStateEffects.Apply(person, household, new AgentAction(AgentActionType.BuyFood, "Eat"));

        Assert.Equal(1d, result.Person.GetNeed(NeedType.Food).Fulfillment);
        Assert.Equal(new Money(15m), result.Person.Balance);
        Assert.Equal(new Money(10m), result.Person.Expenses);
        Assert.Equal(new Quantity(2m), result.Household.FoodDemand);
        Assert.Equal(AgentActivity.BuyingFood, result.Person.CurrentActivity);
    }

    [Fact]
    public void AgentStateEffects_SeekEmployment_ChangesEmploymentState()
    {
        var household = CreateHousehold(out var personId);
        var person = CreatePerson(personId, household.Id, employmentState: EmploymentState.Unemployed);

        var result = AgentStateEffects.Apply(person, household, new AgentAction(AgentActionType.SeekEmployment, "Look for work"));

        Assert.Equal(EmploymentState.Seeking, result.Person.EmploymentState);
        Assert.Equal(AgentActivity.SeekingWork, result.Person.CurrentActivity);
    }

    [Fact]
    public void AgentStateEffects_ReturnHome_ChangesLocation()
    {
        var household = CreateHousehold(out var personId);
        var person = CreatePerson(personId, household.Id, currentLocation: new GridPosition(5, 5));

        var result = AgentStateEffects.Apply(person, household, new AgentAction(AgentActionType.ReturnHome, "Go home"));

        Assert.Equal(household.HomeLocation, result.Person.CurrentLocation);
        Assert.Equal(AgentActivity.AtHome, result.Person.CurrentActivity);
    }

    [Fact]
    public void AgentStateEffects_GoToWork_PaysWageAndMovesPerson()
    {
        var household = CreateHousehold(out var personId);
        var person = CreatePerson(
            personId,
            household.Id,
            employmentState: EmploymentState.Employed,
            balance: new Money(50m),
            income: new Money(100m),
            workplaceLocation: new GridPosition(9, 9),
            wage: new Money(20m));

        var result = AgentStateEffects.Apply(person, household, new AgentAction(AgentActionType.GoToWork, "Work"));

        Assert.Equal(new Money(70m), result.Person.Balance);
        Assert.Equal(new Money(120m), result.Person.Income);
        Assert.Equal(new Money(20m), result.Household.Income);
        Assert.Equal(new GridPosition(9, 9), result.Person.CurrentLocation);
        Assert.Equal(AgentActivity.Working, result.Person.CurrentActivity);
    }

    private static HouseholdAgent CreateHousehold(
        out PersonId personId,
        bool includeHomeLocation = true,
        Quantity? foodDemand = null)
    {
        personId = PersonId.New();
        var householdId = HouseholdId.New();
        return new HouseholdAgent(
            householdId,
            includeHomeLocation ? PlotId.New() : null,
            includeHomeLocation ? new GridPosition(1, 1) : null,
            [personId],
            Money.Zero,
            Money.Zero,
            Money.Zero,
            Money.Zero,
            foodDemand ?? new Quantity(1m),
            new Quantity(1m),
            0,
            0.5d);
    }

    private static PersonAgent CreatePerson(
        PersonId personId,
        HouseholdId householdId,
        double foodFulfillment = 0.8d,
        double socialFulfillment = 0.8d,
        double recreationFulfillment = 0.8d,
        double educationFulfillment = 0.8d,
        double incomeFulfillment = 0.8d,
        EmploymentState employmentState = EmploymentState.Unemployed,
        GridPosition? currentLocation = null,
        GridPosition? workplaceLocation = null,
        global::ConsoleCity.Agents.AgentActivity currentActivity = global::ConsoleCity.Agents.AgentActivity.AtHome,
        int age = 29,
        LifeStage lifeStage = LifeStage.Adult,
        Money? balance = null,
        Money? income = null,
        Money? expenses = null,
        Money? wage = null,
        double satisfaction = 0.5d,
        global::ConsoleCity.Agents.AgentAttributeProfile? attributes = null)
    {
        return new PersonAgent(
            personId,
            householdId,
            "Avery",
            age,
            lifeStage,
            new EmploymentRecord(employmentState, workplaceLocation is null ? null : OrganizationId.New(), workplaceLocation, wage),
            income ?? Money.Zero,
            expenses ?? Money.Zero,
            balance ?? Money.Zero,
            PlotId.New(),
            currentLocation ?? new GridPosition(1, 1),
            currentActivity,
            TransportPreference.Walk,
            satisfaction,
            attributes ?? AgentAttributeProfile.Default,
            [
                new NeedStatus(NeedType.Food, foodFulfillment),
                new NeedStatus(NeedType.Social, socialFulfillment),
                new NeedStatus(NeedType.Recreation, recreationFulfillment),
                new NeedStatus(NeedType.Education, educationFulfillment),
                new NeedStatus(NeedType.Income, incomeFulfillment)
            ],
            [new SkillRating("Retail", 0.4d)],
            [new RelationshipLink(personId, RelationshipType.HouseholdMember, 1d)],
            [new AgentGoal(GoalType.Shelter, "Keep a stable home", 0.8d)],
            new RoutinePlan([new RoutineBlock(8, 17, AgentActionType.GoToWork)]));
    }

    private sealed class FixedRandomSource(int nextValue) : IRandomSource
    {
        public int Next(int minInclusive, int maxExclusive) => Math.Clamp(nextValue, minInclusive, maxExclusive - 1);
        public double NextDouble() => 0.0d;
    }
}
