using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public interface IAgentDecisionPolicy
{
    string Name { get; }

    AgentDecision Decide(PersonAgent person, HouseholdAgent household, SimulationTime currentTime, IRandomSource randomSource);
}
