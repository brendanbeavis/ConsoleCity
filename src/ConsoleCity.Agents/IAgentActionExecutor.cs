using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public interface IAgentActionExecutor
{
    AgentTransitionResult Apply(PersonAgent person, HouseholdAgent household, AgentAction action, SimulationTime currentTime);
}
