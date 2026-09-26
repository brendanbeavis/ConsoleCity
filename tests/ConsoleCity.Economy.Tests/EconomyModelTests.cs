using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.World;

namespace ConsoleCity.Economy.Tests;

public class EconomyModelTests
{
    [Fact]
    public void PriceQuote_WithMarketState_AdjustsPriceUpWhenDemandExceedsSupply()
    {
        var quote = new PriceQuote(new ResourceId("packaged-food"), new Money(10m), new Money(10m), new Quantity(0m), new Quantity(0m));

        var updated = quote.WithMarketState(new Quantity(1m), new Quantity(4m));

        Assert.True(updated.CurrentPrice.Amount > quote.CurrentPrice.Amount);
        Assert.Equal(new Quantity(1m), updated.Supply);
        Assert.Equal(new Quantity(4m), updated.Demand);
    }

    [Fact]
    public void Advance_PaysWagesToPersonAndHousehold()
    {
        var personId = PersonId.New();
        var householdId = HouseholdId.New();
        var businessId = OrganizationId.New();

        var person = CreatePerson(
            personId,
            householdId,
            businessId,
            employmentState: EmploymentState.Employed,
            balance: new Money(50m),
            income: new Money(200m),
            wage: new Money(100m));
        var household = CreateHousehold(householdId, personId, savings: new Money(300m));
        var business = CreateBusiness(businessId, [personId], [], [], new Money(1000m), new Money(0m));
        var model = new SimpleEconomyModel(new EconomySnapshot(
            new SimulationTime(0),
            [new PriceQuote("steel", 5m, 5m)],
            [new ProductionRecipe("unused", [new InventoryLine("input", 1)], [new InventoryLine("output", 1)])],
            new EconomicIndicators(1m, Money.Zero, Money.Zero, Money.Zero, Money.Zero, 0),
            [business],
            GovernmentFinance.Zero));

        var result = model.Advance(new SimulationTime(1), [person], [household]);

        var updatedPerson = Assert.Single(result.People);
        var updatedHousehold = Assert.Single(result.Households);
        var updatedBusiness = Assert.Single(result.Snapshot.Businesses);

        Assert.Equal(new Money(150m), updatedPerson.Balance);
        Assert.Equal(new Money(300m), updatedPerson.Income);
        Assert.Equal(new Money(400m), updatedHousehold.Savings);
        Assert.Equal(new Money(900m), updatedBusiness.Cash);
        Assert.Contains(result.Transfers, transfer => transfer.TransferType == EconomicTransferType.Wage && transfer.ToPersonId == personId && transfer.ToHouseholdId == householdId);
        Assert.Contains(result.Events, eventItem => eventItem.EventType == EconomicEventType.WagePaid);
    }

    [Fact]
    public void Advance_ProducesGoods_AndRecordsGovernmentRevenue()
    {
        var businessId = OrganizationId.New();
        var business = CreateBusiness(
            businessId,
            [],
            [new InventoryLine("grain", 10m)],
            ["food-processing"],
            new Money(0m),
            new Money(0m));
        var model = new SimpleEconomyModel(new EconomySnapshot(
            new SimulationTime(0),
            [new PriceQuote("packaged-food", 10m, 10m)],
            [new ProductionRecipe("food-processing", [new InventoryLine("grain", 10m)], [new InventoryLine("packaged-food", 8m)])],
            new EconomicIndicators(0m, Money.Zero, Money.Zero, Money.Zero, Money.Zero, 0),
            [business],
            GovernmentFinance.Zero));

        var result = model.Advance(new SimulationTime(1), [], []);

        var updatedBusiness = Assert.Single(result.Snapshot.Businesses);
        var producedInventory = updatedBusiness.Inventory.ToDictionary(line => line.ResourceId, line => line.Quantity);

        Assert.Equal(new Quantity(0m), producedInventory[new ResourceId("grain")]);
        Assert.Equal(new Quantity(8m), producedInventory[new ResourceId("packaged-food")]);
        Assert.True(updatedBusiness.Revenue.Amount > 0m);
        Assert.True(result.Snapshot.Government.Revenue.Amount > 0m);
        Assert.Contains(result.Events, eventItem => eventItem.EventType == EconomicEventType.ProductionCompleted);
    }

    [Fact]
    public void Advance_ConsumesHouseholdDemand_AndRaisesFoodPriceWhenSupplyIsTight()
    {
        var personId = PersonId.New();
        var householdId = HouseholdId.New();
        var businessId = OrganizationId.New();

        var person = CreatePerson(personId, householdId, businessId, employmentState: EmploymentState.Unemployed);
        var household = CreateHousehold(householdId, personId, savings: new Money(100m), foodDemand: new Quantity(5m));
        var business = CreateBusiness(
            businessId,
            [],
            [new InventoryLine("packaged-food", 2m)],
            [],
            new Money(0m),
            new Money(0m));
        var model = new SimpleEconomyModel(new EconomySnapshot(
            new SimulationTime(0),
            [new PriceQuote("packaged-food", 10m, 10m)],
            [],
            new EconomicIndicators(0m, Money.Zero, Money.Zero, Money.Zero, Money.Zero, 0),
            [business],
            GovernmentFinance.Zero));

        var result = model.Advance(new SimulationTime(1), [person], [household]);

        var updatedHousehold = Assert.Single(result.Households);
        var updatedBusiness = Assert.Single(result.Snapshot.Businesses);
        var updatedQuote = Assert.Single(result.Snapshot.Prices);

        Assert.Equal(new Quantity(4m), updatedHousehold.FoodDemand);
        Assert.Equal(new Money(90m), updatedHousehold.Savings);
        Assert.Equal(new Quantity(1m), updatedBusiness.Inventory.Single(line => line.ResourceId.Value == "packaged-food").Quantity);
        Assert.True(updatedQuote.CurrentPrice.Amount > updatedQuote.BasePrice.Amount);
        Assert.Contains(result.Transfers, transfer => transfer.TransferType == EconomicTransferType.HouseholdPurchase && transfer.ToHouseholdId == householdId);
    }

    private static ConsoleCity.Economy.BusinessModel CreateBusiness(
        OrganizationId businessId,
        IReadOnlyList<PersonId> employees,
        IReadOnlyList<InventoryLine> inventory,
        IReadOnlyList<string> recipeKeys,
        Money cash,
        Money revenue)
    {
        return new BusinessModel(
            businessId,
            "Test Business",
            EconomicActorType.Business,
            BusinessLifecycleState.Operating,
            new SimulationTime(0),
            null,
            null,
            cash,
            revenue,
            Money.Zero,
            new Money(100m),
            employees,
            inventory,
            recipeKeys);
    }

    private static HouseholdAgent CreateHousehold(HouseholdId householdId, PersonId memberId, Money? savings = null, Quantity? foodDemand = null)
    {
        return new HouseholdAgent(
            householdId,
            null,
            null,
            [memberId],
            Money.Zero,
            Money.Zero,
            savings ?? Money.Zero,
            Money.Zero,
            foodDemand ?? new Quantity(0m),
            new Quantity(0m),
            0,
            0.5d);
    }

    private static PersonAgent CreatePerson(
        PersonId personId,
        HouseholdId householdId,
        OrganizationId businessId,
        EmploymentState employmentState,
        Money? balance = null,
        Money? income = null,
        Money? wage = null)
    {
        return new PersonAgent(
            personId,
            householdId,
            "Avery",
            32,
            LifeStage.Adult,
            new EmploymentRecord(employmentState, businessId, new GridPosition(0, 0), wage ?? Money.Zero),
            income ?? Money.Zero,
            Money.Zero,
            balance ?? Money.Zero,
            null,
            new GridPosition(0, 0),
            AgentActivity.AtHome,
            TransportPreference.Walk,
            0.5d,
            AgentAttributeProfile.Default,
            [new NeedStatus(NeedType.Food, 1d), new NeedStatus(NeedType.Social, 1d), new NeedStatus(NeedType.Recreation, 1d), new NeedStatus(NeedType.Education, 1d), new NeedStatus(NeedType.Income, 1d)],
            [new SkillRating("Retail", 0.5d)],
            [new RelationshipLink(personId, RelationshipType.HouseholdMember, 1d)],
            [new AgentGoal(GoalType.Shelter, "Keep a stable home", 0.8d)],
            new RoutinePlan([new RoutineBlock(8, 17, AgentActionType.GoToWork)]));
    }
}
