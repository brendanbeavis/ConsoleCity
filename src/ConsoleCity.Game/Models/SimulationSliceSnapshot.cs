using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.World;

namespace ConsoleCity.Game;

public sealed record SimulationSliceSnapshot(
    int Seed,
    SimulationTime CurrentTime,
    WorldModel World,
    AgentPopulationSnapshot Population,
    EconomySnapshot Economy,
    GameProgressionState Progression,
    IReadOnlyList<CommuteTrip> ActiveTrips,
    int CompletedTrips,
    int HouseholdPurchases);
