using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.World;

namespace ConsoleCity.Game;

internal sealed record SimulationSliceState(
    int Seed,
    SimulationTime CurrentTime,
    WorldModel World,
    IReadOnlyList<PersonAgent> People,
    IReadOnlyList<HouseholdAgent> Households,
    EconomySnapshot Economy,
    GameProgressionState Progression,
    IReadOnlyDictionary<PersonId, BuildingId> HomeBuildingByPerson,
    IReadOnlyDictionary<HouseholdId, BuildingId> HomeBuildingByHousehold,
    IReadOnlyDictionary<PersonId, BuildingId> WorkplaceBuildingByPerson,
    IReadOnlyList<CommuteTrip> ActiveTrips,
    int CompletedTrips,
    int HouseholdPurchases)
{
    public RegionModel Region => World.Regions[0];

    public CityModel City => Region.Cities[0];

    public DistrictModel District => City.Districts[0];

    public IReadOnlyList<PlotModel> Plots => District.Plots;

    public IReadOnlyList<BuildingModel> Buildings => Plots.SelectMany(plot => plot.Buildings).ToList();

    public SimulationSliceSnapshot ToSnapshot()
        => new(
            Seed,
            CurrentTime,
            World,
            new AgentPopulationSnapshot(CurrentTime, People, Households),
            Economy,
            Progression,
            ActiveTrips,
            CompletedTrips,
            HouseholdPurchases);
}
