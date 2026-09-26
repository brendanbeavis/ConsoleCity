using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.World;

namespace ConsoleCity.Game;

public sealed class GameSession : IGameSession
{
    private readonly object syncRoot = new();
    private SimulationSliceState? state;
    private bool isRunning;

    public SimulationTime Time
    {
        get
        {
            lock (syncRoot)
            {
                return state?.CurrentTime ?? new SimulationTime(0);
            }
        }
    }

    public bool IsWorldCreated
    {
        get
        {
            lock (syncRoot)
            {
                return state is not null;
            }
        }
    }

    public bool IsRunning
    {
        get
        {
            lock (syncRoot)
            {
                return isRunning;
            }
        }
    }

    public SimulationSliceSnapshot? Snapshot
    {
        get
        {
            lock (syncRoot)
            {
                return state?.ToSnapshot();
            }
        }
    }

    public void CreateNewWorld(int seed = 42)
    {
        lock (syncRoot)
        {
            state = PlayableWorldFactory.Create(seed);
            isRunning = false;
        }
    }

    public void Start()
    {
        lock (syncRoot)
        {
            EnsureWorldCreated();
            isRunning = true;
        }
    }

    public void Pause()
    {
        lock (syncRoot)
        {
            EnsureWorldCreated();
            isRunning = false;
        }
    }

    public void Advance(int ticks = 1)
    {
        lock (syncRoot)
        {
            EnsureWorldCreated();
            state = PlayableWorldSimulator.Advance(state!, ticks);
        }
    }

    public CityStatistics GetCityStatistics()
    {
        lock (syncRoot)
        {
            EnsureWorldCreated();

            var currentState = state!;
            var buildings = currentState.Buildings;
            var businesses = currentState.Economy.Businesses;
            var people = currentState.People;
            var households = currentState.Households;
            var totalSavings = households.Aggregate(Money.Zero, static (current, household) => current + household.Savings);
            var totalBusinessCash = businesses.Aggregate(Money.Zero, static (current, business) => current + business.Cash);
            var totalWages = people.Aggregate(Money.Zero, static (current, person) => current + person.Income);
            var totalExpenses = households.Aggregate(Money.Zero, static (current, household) => current + household.Expenses);
            var foodInventory = businesses.Sum(business => business.Inventory.Where(line => line.ResourceId.Value == "packaged-food").Sum(line => line.Quantity.Value));

            return new CityStatistics(
                $"World Seed {currentState.World.Seed}",
                currentState.City.Name,
                currentState.CurrentTime,
                people.Count,
                households.Count,
                buildings.Count(building => building.BuildingType is BuildingType.House or BuildingType.Apartment),
                businesses.Count,
                people.Count(person => person.EmploymentState == EmploymentState.Employed),
                currentState.ActiveTrips.Count,
                currentState.CompletedTrips,
                currentState.HouseholdPurchases,
                totalSavings,
                totalBusinessCash,
                totalWages,
                totalExpenses,
                decimal.ToInt32(decimal.Truncate(foodInventory)));
        }
    }

    public string InspectPerson(string query)
    {
        lock (syncRoot)
        {
            EnsureWorldCreated();

            var person = FindPerson(query);
            var household = state!.Households.First(item => item.Id == person.HouseholdId);
            var homeBuilding = GetHomeBuilding(person.Id);
            var workplaceBuilding = person.WorkplaceId is null
                ? null
                : state.Economy.Businesses.FirstOrDefault(business => business.Id == person.WorkplaceId)?.BuildingId is BuildingId workplaceBuildingId
                    ? state.Buildings.FirstOrDefault(building => building.Id == workplaceBuildingId)
                    : null;

            return string.Join(Environment.NewLine,
            [
                $"PERSON {person.DisplayName}",
                $"Id: {person.Id}",
                $"Age: {person.Age} ({person.LifeStage})",
                $"Location: {person.CurrentLocation}",
                $"Activity: {person.CurrentActivity}",
                $"Household: {household.Id}",
                $"Home: {homeBuilding.Name} {homeBuilding.Location}",
                $"Workplace: {(workplaceBuilding is null ? "None" : $"{workplaceBuilding.Name} {workplaceBuilding.Location}")}",
                $"Employment: {person.EmploymentState}",
                $"Income earned: ${person.Income.Amount:0.00}",
                $"Expenses: ${person.Expenses.Amount:0.00}",
                $"Balance: ${person.Balance.Amount:0.00}",
                $"Food need: {person.GetNeed(NeedType.Food).Fulfillment:0.00}",
                $"Income need: {person.GetNeed(NeedType.Income).Fulfillment:0.00}",
                $"Mobility need: {person.GetNeed(NeedType.Mobility).Fulfillment:0.00}"
            ]);
        }
    }

    public string InspectHousehold(string query)
    {
        lock (syncRoot)
        {
            EnsureWorldCreated();

            var household = FindHousehold(query);
            var building = GetHouseholdHome(household.Id);
            var members = household.Members
                .Select(memberId => state!.People.First(person => person.Id == memberId).DisplayName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToList();

            return string.Join(Environment.NewLine,
            [
                $"HOUSEHOLD {household.Id}",
                $"Home: {building.Name} {building.Location}",
                $"Members: {string.Join(", ", members)}",
                $"Income: ${household.Income.Amount:0.00}",
                $"Expenses: ${household.Expenses.Amount:0.00}",
                $"Savings: ${household.Savings.Amount:0.00}",
                $"Food demand: {household.FoodDemand.Value:0.##}",
                $"Utility demand: {household.UtilityDemand.Value:0.##}",
                $"Satisfaction: {household.Satisfaction:0.00}"
            ]);
        }
    }

    public string InspectBuilding(string query)
    {
        lock (syncRoot)
        {
            EnsureWorldCreated();

            var building = FindBuilding(query);
            var residents = state!.HomeBuildingByPerson.Count(pair => pair.Value == building.Id);
            var workers = state.WorkplaceBuildingByPerson.Count(pair => pair.Value == building.Id);
            var business = state.Economy.Businesses.FirstOrDefault(item => item.BuildingId == building.Id);

            return string.Join(Environment.NewLine,
            [
                $"BUILDING {building.Name}",
                $"Id: {building.Id}",
                $"Type: {building.BuildingType}",
                $"Location: {building.Location}",
                $"State: {building.State}",
                $"Condition: {building.Condition:0.00}",
                $"Residents: {residents} / {building.Capacities.Residents}",
                $"Jobs: {workers} / {building.Capacities.Jobs}",
                $"Business: {(business is null ? "None" : business.Name)}",
                $"Cashflow: {(business is null ? "$0.00" : $"${business.Revenue.Amount - business.Expenses.Amount:0.00}")}"
            ]);
        }
    }

    private void EnsureWorldCreated()
    {
        if (state is null)
        {
            throw new InvalidOperationException("Create a world before running the simulation.");
        }
    }

    private PersonAgent FindPerson(string query)
    {
        var matches = state!.People
            .Where(person => MatchesQuery(query, person.DisplayName, person.Id.ToString()))
            .OrderBy(person => person.DisplayName, StringComparer.Ordinal)
            .ToList();

        return matches.Count switch
        {
            0 => throw new InvalidOperationException($"No person matched '{query}'."),
            > 1 => throw new InvalidOperationException($"Multiple people matched '{query}'. Use a more specific name or id."),
            _ => matches[0]
        };
    }

    private HouseholdAgent FindHousehold(string query)
    {
        var matches = state!.Households
            .Where(household => MatchesQuery(query, household.Id.ToString(), household.Id.ToString())
                || household.Members.Any(memberId => MatchesQuery(query, state.People.First(person => person.Id == memberId).DisplayName, memberId.ToString())))
            .OrderBy(household => household.Id.Value)
            .ToList();

        return matches.Count switch
        {
            0 => throw new InvalidOperationException($"No household matched '{query}'."),
            > 1 => throw new InvalidOperationException($"Multiple households matched '{query}'. Use a household id or a more specific member name."),
            _ => matches[0]
        };
    }

    private BuildingModel FindBuilding(string query)
    {
        var matches = state!.Buildings
            .Where(building => MatchesQuery(query, building.Name, building.Id.ToString()))
            .OrderBy(building => building.Name, StringComparer.Ordinal)
            .ToList();

        return matches.Count switch
        {
            0 => throw new InvalidOperationException($"No building matched '{query}'."),
            > 1 => throw new InvalidOperationException($"Multiple buildings matched '{query}'. Use a more specific name or id."),
            _ => matches[0]
        };
    }

    private BuildingModel GetHomeBuilding(PersonId personId)
    {
        var buildingId = state!.HomeBuildingByPerson[personId];
        return state.Buildings.First(building => building.Id == buildingId);
    }

    private BuildingModel GetHouseholdHome(HouseholdId householdId)
    {
        var buildingId = state!.HomeBuildingByHousehold[householdId];
        return state.Buildings.First(building => building.Id == buildingId);
    }

    private static bool MatchesQuery(string query, string displayText, string id)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return false;
        }

        var trimmed = query.Trim();
        return displayText.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
            || id.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
    }
}
