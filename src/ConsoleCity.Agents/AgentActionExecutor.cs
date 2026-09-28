using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public sealed class AgentActionExecutor : IAgentActionExecutor
{
    public AgentTransitionResult Apply(PersonAgent person, HouseholdAgent household, AgentAction action, SimulationTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentNullException.ThrowIfNull(household);
        _ = currentTime;

        return AgentStateEffects.Apply(person, household, action);
    }
}
