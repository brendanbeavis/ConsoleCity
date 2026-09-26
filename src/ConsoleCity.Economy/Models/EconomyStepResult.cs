using ConsoleCity.Agents;

namespace ConsoleCity.Economy;

public sealed record EconomyStepResult(
    EconomySnapshot Snapshot,
    IReadOnlyList<PersonAgent> People,
    IReadOnlyList<HouseholdAgent> Households,
    IReadOnlyList<EconomicEvent> Events,
    IReadOnlyList<EconomicTransfer> Transfers);
