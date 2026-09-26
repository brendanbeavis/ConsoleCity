using ConsoleCity.Core;
using ConsoleCity.Transport;

namespace ConsoleCity.Game;

public sealed record CommuteTrip(
    PersonId PersonId,
    GridPosition Origin,
    GridPosition Destination,
    SimulationTime DepartureTime,
    SimulationTime ArrivalTime,
    TransportMode Mode,
    string Purpose);
