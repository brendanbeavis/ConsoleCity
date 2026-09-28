using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.Transport;
using ConsoleCity.World;

namespace ConsoleCity.Game;

internal static class PlayableWorldSimulator
{
    private const int WorkStartHour = 8;
    private const int WorkEndHour = 17;
    private const int ConsumptionHour = 19;
    private const int SleepStartHour = 22;
    private const decimal UtilityCostPerHouseholdMember = 1m;

    public static SimulationSliceState Advance(SimulationSliceState state, int ticks)
    {
        var updatedState = state;
        for (var tick = 0; tick < Math.Max(1, ticks); tick++)
        {
            updatedState = AdvanceOneTick(updatedState);
        }

        return updatedState;
    }

    private static SimulationSliceState AdvanceOneTick(SimulationSliceState state)
    {
        var currentTime = state.CurrentTime.Advance();
        var updatedState = state with { CurrentTime = currentTime };

        updatedState = CompleteArrivingTrips(updatedState, currentTime);
        updatedState = StartWorkCommutes(updatedState, currentTime);
        updatedState = ApplyWorkActivity(updatedState, currentTime);
        updatedState = CloseWorkday(updatedState, currentTime);
        updatedState = ApplyConsumption(updatedState, currentTime);
        updatedState = ApplyPassiveActivities(updatedState, currentTime);
        updatedState = RefreshEconomy(updatedState, currentTime);
        updatedState = updatedState with { Progression = ProgressionEngine.Advance(updatedState.Progression, updatedState.ToSnapshot()) };

        return updatedState;
    }

    private static SimulationSliceState CompleteArrivingTrips(SimulationSliceState state, SimulationTime currentTime)
    {
        if (state.ActiveTrips.Count == 0)
        {
            return state;
        }

        var arrivals = state.ActiveTrips.Where(trip => trip.ArrivalTime.Tick <= currentTime.Tick).ToList();
        if (arrivals.Count == 0)
        {
            return state;
        }

        var people = state.People.ToDictionary(person => person.Id);
        foreach (var arrival in arrivals)
        {
            if (!people.TryGetValue(arrival.PersonId, out var person))
            {
                continue;
            }

            var activity = arrival.Purpose == "Work" && currentTime.Hour is >= WorkStartHour and < WorkEndHour
                ? AgentActivity.Working
                : AgentActivity.AtHome;

            people[arrival.PersonId] = ClonePerson(
                person,
                currentLocation: arrival.Destination,
                currentActivity: activity,
                needs: UpdateNeed(person.Needs, NeedType.Mobility, 1d));
        }

        return state with
        {
            People = people.Values.OrderBy(person => person.DisplayName, StringComparer.Ordinal).ToList(),
            ActiveTrips = state.ActiveTrips.Where(trip => trip.ArrivalTime.Tick > currentTime.Tick).ToList(),
            CompletedTrips = state.CompletedTrips + arrivals.Count
        };
    }

    private static SimulationSliceState StartWorkCommutes(SimulationSliceState state, SimulationTime currentTime)
    {
        var activeTripPeople = state.ActiveTrips.Select(trip => trip.PersonId).ToHashSet();
        var people = state.People.ToDictionary(person => person.Id);
        var trips = state.ActiveTrips.ToList();

        foreach (var person in state.People.OrderBy(person => person.DisplayName, StringComparer.Ordinal))
        {
            if (person.EmploymentState != EmploymentState.Employed || person.WorkplaceLocation is null)
            {
                continue;
            }

            if (!state.HomeBuildingByPerson.TryGetValue(person.Id, out var homeBuildingId))
            {
                continue;
            }

            var homeBuilding = state.Buildings.First(building => building.Id == homeBuildingId);
            var travelHours = CalculateTravelHours(homeBuilding.Location, person.WorkplaceLocation.Value);
            var departureHour = Math.Max(0, WorkStartHour - travelHours);
            if (currentTime.Hour != departureHour || activeTripPeople.Contains(person.Id))
            {
                continue;
            }

            if (!person.CurrentLocation.Equals(homeBuilding.Location))
            {
                continue;
            }

            trips.Add(new CommuteTrip(
                person.Id,
                homeBuilding.Location,
                person.WorkplaceLocation.Value,
                currentTime,
                currentTime.Advance(travelHours),
                TransportMode.Pedestrian,
                "Work"));

            people[person.Id] = ClonePerson(
                person,
                currentActivity: AgentActivity.Travelling,
                needs: UpdateNeed(person.Needs, NeedType.Mobility, 0.8d));
        }

        return state with
        {
            People = people.Values.OrderBy(person => person.DisplayName, StringComparer.Ordinal).ToList(),
            ActiveTrips = trips
        };
    }

    private static SimulationSliceState ApplyWorkActivity(SimulationSliceState state, SimulationTime currentTime)
    {
        if (currentTime.Hour is < WorkStartHour or >= WorkEndHour)
        {
            return state;
        }

        var people = state.People.ToDictionary(person => person.Id);
        foreach (var person in state.People)
        {
            if (person.EmploymentState != EmploymentState.Employed || person.WorkplaceLocation is null)
            {
                continue;
            }

            if (!person.CurrentLocation.Equals(person.WorkplaceLocation.Value) || person.CurrentActivity == AgentActivity.Travelling)
            {
                continue;
            }

            people[person.Id] = ClonePerson(
                person,
                currentActivity: AgentActivity.Working,
                needs: UpdateNeed(person.Needs, NeedType.Income, Math.Min(1d, person.GetNeed(NeedType.Income).Fulfillment + 0.02d)));
        }

        return state with { People = people.Values.OrderBy(person => person.DisplayName, StringComparer.Ordinal).ToList() };
    }

    private static SimulationSliceState CloseWorkday(SimulationSliceState state, SimulationTime currentTime)
    {
        if (currentTime.Hour != WorkEndHour)
        {
            return state;
        }

        var peopleById = state.People.ToDictionary(person => person.Id);
        var householdsById = state.Households.ToDictionary(household => household.Id);
        var businesses = state.Economy.Businesses.ToDictionary(business => business.Id);
        var activeTripPeople = state.ActiveTrips.Select(trip => trip.PersonId).ToHashSet();
        var trips = state.ActiveTrips.ToList();

        foreach (var business in state.Economy.Businesses)
        {
            var paidEmployees = new List<PersonId>();
            var workingEmployees = business.Employees
                .Where(employeeId => peopleById.TryGetValue(employeeId, out var person)
                    && person.WorkplaceLocation is not null
                    && person.CurrentLocation.Equals(person.WorkplaceLocation.Value)
                    && person.CurrentActivity != AgentActivity.Travelling)
                .ToList();

            foreach (var employeeId in workingEmployees)
            {
                var person = peopleById[employeeId];
                var household = householdsById[person.HouseholdId];
                var wage = person.Employment.Wage;

                peopleById[employeeId] = ClonePerson(
                    person,
                    income: person.Income + wage,
                    balance: person.Balance + wage,
                    currentActivity: AgentActivity.Working,
                    needs: UpdateNeed(person.Needs, NeedType.Income, 1d));

                householdsById[household.Id] = CloneHousehold(
                    household,
                    income: household.Income + wage,
                    savings: household.Savings + wage,
                    satisfaction: Math.Min(1d, household.Satisfaction + 0.01d));

                paidEmployees.Add(employeeId);
            }

            var updatedBusiness = CloneBusiness(
                business,
                cash: business.Cash - new Money(paidEmployees.Count * business.WagePerEmployee.Amount),
                expenses: business.Expenses + new Money(paidEmployees.Count * business.WagePerEmployee.Amount));
            updatedBusiness = ApplyProduction(updatedBusiness, workingEmployees.Count);
            businesses[business.Id] = updatedBusiness;

            foreach (var employeeId in workingEmployees)
            {
                if (!peopleById.TryGetValue(employeeId, out var person) || person.WorkplaceLocation is null)
                {
                    continue;
                }

                if (activeTripPeople.Contains(employeeId))
                {
                    continue;
                }

                if (!state.HomeBuildingByPerson.TryGetValue(employeeId, out var homeBuildingId))
                {
                    continue;
                }

                var homeBuilding = state.Buildings.First(building => building.Id == homeBuildingId);
                var travelHours = CalculateTravelHours(person.WorkplaceLocation.Value, homeBuilding.Location);
                trips.Add(new CommuteTrip(
                    employeeId,
                    person.WorkplaceLocation.Value,
                    homeBuilding.Location,
                    currentTime,
                    currentTime.Advance(travelHours),
                    TransportMode.Pedestrian,
                    "Home"));

                peopleById[employeeId] = ClonePerson(
                    peopleById[employeeId],
                    currentActivity: AgentActivity.Travelling,
                    needs: UpdateNeed(peopleById[employeeId].Needs, NeedType.Mobility, 0.75d));
            }
        }

        return state with
        {
            People = peopleById.Values.OrderBy(person => person.DisplayName, StringComparer.Ordinal).ToList(),
            Households = householdsById.Values.OrderBy(household => household.Id.Value).ToList(),
            Economy = CloneEconomy(state.Economy, currentTime, businesses.Values.OrderBy(business => business.Name, StringComparer.Ordinal).ToList()),
            ActiveTrips = trips
        };
    }

    private static BusinessModel ApplyProduction(BusinessModel business, int workerCount)
    {
        if (workerCount == 0 || business.RecipeKeys.Count == 0)
        {
            return business;
        }

        var grainLine = business.Inventory.FirstOrDefault(line => line.ResourceId.Value == "grain");
        if (grainLine is null || grainLine.Quantity.Value < 4m)
        {
            return business;
        }

        var inventory = SetInventoryQuantity(business.Inventory, "grain", grainLine.Quantity.Value - 4m);
        inventory = SetInventoryQuantity(inventory, "packaged-food", GetInventoryQuantity(inventory, "packaged-food") + 12m);

        return CloneBusiness(
            business,
            inventory: inventory,
            expenses: business.Expenses + new Money(6m),
            cash: business.Cash - new Money(6m));
    }

    private static SimulationSliceState ApplyConsumption(SimulationSliceState state, SimulationTime currentTime)
    {
        if (currentTime.Hour != ConsumptionHour)
        {
            return state;
        }

        var peopleById = state.People.ToDictionary(person => person.Id);
        var householdsById = state.Households.ToDictionary(household => household.Id);
        var businesses = state.Economy.Businesses.ToDictionary(business => business.Id);
        var foodPrice = state.Economy.Prices.FirstOrDefault(price => price.ResourceId.Value == "packaged-food")?.CurrentPrice.Amount ?? 4m;
        var foodBusiness = state.Economy.Businesses.First();
        var packagedFood = GetInventoryQuantity(foodBusiness.Inventory, "packaged-food");
        var updatedBusiness = foodBusiness;
        var purchaseCount = 0;
        var totalFoodRevenue = 0m;

        foreach (var household in state.Households)
        {
            var foodDemandUnits = household.Members.Count;
            var purchasedFoodUnits = Math.Min(packagedFood, foodDemandUnits);
            var foodCost = purchasedFoodUnits * foodPrice;
            var utilityCost = household.Members.Count * UtilityCostPerHouseholdMember;
            var totalCost = foodCost + utilityCost;
            var perPersonCost = household.Members.Count == 0 ? 0m : totalCost / household.Members.Count;
            packagedFood -= purchasedFoodUnits;
            totalFoodRevenue += foodCost;

            householdsById[household.Id] = CloneHousehold(
                household,
                expenses: household.Expenses + new Money(totalCost),
                savings: new Money(Math.Max(0m, household.Savings.Amount - totalCost)),
                foodDemand: new Quantity(household.Members.Count * 2m),
                satisfaction: Math.Min(1d, household.Satisfaction + 0.01d));

            foreach (var memberId in household.Members)
            {
                if (!peopleById.TryGetValue(memberId, out var person))
                {
                    continue;
                }

                peopleById[memberId] = ClonePerson(
                    person,
                    balance: new Money(Math.Max(0m, person.Balance.Amount - perPersonCost)),
                    expenses: person.Expenses + new Money(perPersonCost),
                    currentActivity: person.CurrentActivity == AgentActivity.Travelling ? person.CurrentActivity : AgentActivity.BuyingFood,
                    needs: UpdateNeed(person.Needs, NeedType.Food, purchasedFoodUnits > 0m ? 1d : 0.35d));
            }

            purchaseCount++;
        }

        updatedBusiness = CloneBusiness(
            updatedBusiness,
            inventory: SetInventoryQuantity(updatedBusiness.Inventory, "packaged-food", packagedFood),
            revenue: updatedBusiness.Revenue + new Money(totalFoodRevenue),
            cash: updatedBusiness.Cash + new Money(totalFoodRevenue));
        businesses[foodBusiness.Id] = updatedBusiness;

        return state with
        {
            People = peopleById.Values.OrderBy(person => person.DisplayName, StringComparer.Ordinal).ToList(),
            Households = householdsById.Values.OrderBy(household => household.Id.Value).ToList(),
            Economy = CloneEconomy(state.Economy, currentTime, businesses.Values.OrderBy(business => business.Name, StringComparer.Ordinal).ToList()),
            HouseholdPurchases = state.HouseholdPurchases + purchaseCount
        };
    }

    private static SimulationSliceState ApplyPassiveActivities(SimulationSliceState state, SimulationTime currentTime)
    {
        var people = state.People.ToDictionary(person => person.Id);
        var activeTripPeople = state.ActiveTrips.Select(trip => trip.PersonId).ToHashSet();

        foreach (var person in state.People)
        {
            if (activeTripPeople.Contains(person.Id))
            {
                continue;
            }

            if (currentTime.Hour >= SleepStartHour || currentTime.Hour < 6)
            {
                people[person.Id] = ClonePerson(
                    person,
                    currentActivity: AgentActivity.Resting,
                    needs: UpdateNeed(person.Needs, NeedType.Recreation, Math.Min(1d, person.GetNeed(NeedType.Recreation).Fulfillment + 0.05d)));
                continue;
            }

            if (currentTime.Hour > ConsumptionHour && person.CurrentLocation.Equals(GetHomeLocation(state, person.Id)))
            {
                people[person.Id] = ClonePerson(person, currentActivity: AgentActivity.AtHome);
            }
        }

        return state with { People = people.Values.OrderBy(person => person.DisplayName, StringComparer.Ordinal).ToList() };
    }

    private static SimulationSliceState RefreshEconomy(SimulationSliceState state, SimulationTime currentTime)
    {
        var supply = new Quantity(GetInventoryQuantity(state.Economy.Businesses.First().Inventory, "packaged-food"));
        var demand = new Quantity(state.Households.Sum(household => household.Members.Count));
        var prices = state.Economy.Prices
            .Select(price => price.ResourceId.Value == "packaged-food"
                ? price.WithMarketState(supply, demand)
                : price)
            .ToList();

        var indicators = new EconomicIndicators(
            state.People.Count == 0 ? 0m : state.People.Count(person => person.EmploymentState == EmploymentState.Employed) / (decimal)state.People.Count,
            state.Households.Aggregate(Money.Zero, static (current, household) => current + household.Income),
            state.Households.Aggregate(Money.Zero, static (current, household) => current + household.Expenses),
            state.Economy.Businesses.Aggregate(Money.Zero, static (current, business) => current + business.Revenue),
            state.Economy.Businesses.Aggregate(Money.Zero, static (current, business) => current + business.Expenses),
            0,
            consumerDemand: demand,
            production: new Quantity(GetInventoryQuantity(state.Economy.Businesses.First().Inventory, "packaged-food")),
            inventory: state.Economy.Businesses
                .SelectMany(business => business.Inventory)
                .Aggregate(Quantity.Zero, static (current, line) => current + line.Quantity));

        return state with
        {
            Economy = new EconomySnapshot(
                currentTime,
                prices,
                state.Economy.Recipes,
                indicators,
                state.Economy.Businesses,
                state.Economy.Government,
                state.Economy.Events)
        };
    }

    private static GridPosition GetHomeLocation(SimulationSliceState state, PersonId personId)
    {
        var buildingId = state.HomeBuildingByPerson[personId];
        return state.Buildings.First(building => building.Id == buildingId).Location;
    }

    private static int CalculateTravelHours(GridPosition origin, GridPosition destination)
        => Math.Max(1, (int)Math.Ceiling((double)origin.DistanceTo(destination).Value / 1.6d));

    private static decimal GetInventoryQuantity(IReadOnlyList<InventoryLine> inventory, string resourceKey)
        => inventory.FirstOrDefault(line => line.ResourceId.Value == resourceKey)?.Quantity.Value ?? 0m;

    private static IReadOnlyList<InventoryLine> SetInventoryQuantity(IReadOnlyList<InventoryLine> inventory, string resourceKey, decimal quantity)
    {
        var updated = inventory.Where(line => line.ResourceId.Value != resourceKey).ToList();
        updated.Add(new InventoryLine(resourceKey, Math.Max(0m, quantity)));
        return updated.OrderBy(line => line.ResourceId.Value, StringComparer.Ordinal).ToList();
    }

    private static IReadOnlyList<NeedStatus> UpdateNeed(IReadOnlyList<NeedStatus> needs, NeedType needType, double fulfillment)
    {
        var updated = needs.Where(need => need.Need != needType).ToList();
        updated.Add(new NeedStatus(needType, Math.Clamp(fulfillment, 0d, 1d)));
        return updated.OrderBy(need => need.Need).ToList();
    }

    private static EconomySnapshot CloneEconomy(EconomySnapshot source, SimulationTime capturedAt, IReadOnlyList<BusinessModel> businesses)
        => new(
            capturedAt,
            source.Prices,
            source.Recipes,
            source.Indicators,
            businesses,
            source.Government,
            source.Events);

    private static PersonAgent ClonePerson(
        PersonAgent source,
        Money? income = null,
        Money? expenses = null,
        Money? balance = null,
        GridPosition? currentLocation = null,
        AgentActivity? currentActivity = null,
        double? satisfaction = null,
        IReadOnlyList<NeedStatus>? needs = null)
        => new(
            source.Id,
            source.HouseholdId,
            source.DisplayName,
            source.Age,
            source.LifeStage,
            source.Employment,
            income ?? source.Income,
            expenses ?? source.Expenses,
            balance ?? source.Balance,
            source.ResidencePlotId,
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

    private static HouseholdAgent CloneHousehold(
        HouseholdAgent source,
        Money? income = null,
        Money? expenses = null,
        Money? savings = null,
        Quantity? foodDemand = null,
        double? satisfaction = null)
        => new(
            source.Id,
            source.HomePlotId,
            source.HomeLocation,
            source.Members,
            income ?? source.Income,
            expenses ?? source.Expenses,
            savings ?? source.Savings,
            source.Debt,
            foodDemand ?? source.FoodDemand,
            source.UtilityDemand,
            source.TransportAssets,
            satisfaction ?? source.Satisfaction);

    private static BusinessModel CloneBusiness(
        BusinessModel source,
        Money? cash = null,
        Money? revenue = null,
        Money? expenses = null,
        IReadOnlyList<InventoryLine>? inventory = null)
        => new(
            source.Id,
            source.Name,
            source.ActorType,
            source.State,
            source.FoundedAt,
            source.BuildingId,
            source.Location,
            cash ?? source.Cash,
            revenue ?? source.Revenue,
            expenses ?? source.Expenses,
            source.WagePerEmployee,
            source.Employees,
            inventory ?? source.Inventory,
            source.RecipeKeys);
}
