using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public sealed record AgentPopulationSnapshot(
    SimulationTime CapturedAt,
    IReadOnlyList<PersonAgent> People,
    IReadOnlyList<HouseholdAgent> Households);
