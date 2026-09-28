using ConsoleCity.Core;

namespace ConsoleCity.Game;

public sealed record CityStatistics(
    string WorldName,
    string CityName,
    SimulationTime CurrentTime,
    int Population,
    int Households,
    int ResidentialBuildings,
    int Businesses,
    int EmployedPeople,
    int ActiveTrips,
    int CompletedTrips,
    int HouseholdPurchases,
    Money TotalHouseholdSavings,
    Money TotalBusinessCash,
    Money TotalWagesEarned,
    Money TotalHouseholdExpenses,
    int FoodInventoryUnits);
