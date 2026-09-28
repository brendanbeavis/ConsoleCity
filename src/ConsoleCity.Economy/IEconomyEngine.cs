using ConsoleCity.Agents;
using ConsoleCity.Core;

namespace ConsoleCity.Economy;

public interface IEconomyEngine
{
    EconomyStepResult Advance(SimulationTime currentTime, IReadOnlyList<PersonAgent> people, IReadOnlyList<HouseholdAgent> households);
}
