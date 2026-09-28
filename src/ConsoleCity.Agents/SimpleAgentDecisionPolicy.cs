using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public sealed class SimpleAgentDecisionPolicy : IAgentDecisionPolicy
{
    public string Name => "SimpleUtilityPolicy";

    public AgentDecision Decide(PersonAgent person, HouseholdAgent household, SimulationTime currentTime, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentNullException.ThrowIfNull(household);
        ArgumentNullException.ThrowIfNull(randomSource);

        var candidates = new List<AgentDecision>
        {
            EvaluateBuyFood(person, household),
            EvaluateReturnHome(person, household),
            EvaluateSeekEmployment(person),
            EvaluateGoToWork(person, currentTime),
            EvaluateSocialise(person),
            EvaluateRest(person),
            EvaluateStudy(person)
        };

        var highestUtility = candidates.Max(candidate => candidate.Utility);
        var topCandidates = candidates.Where(candidate => Math.Abs(candidate.Utility - highestUtility) < 0.0001d).ToList();

        if (topCandidates.Count == 1)
        {
            return topCandidates[0];
        }

        return topCandidates[randomSource.Next(0, topCandidates.Count)];
    }

    private static AgentDecision EvaluateBuyFood(PersonAgent person, HouseholdAgent household)
    {
        var hunger = person.GetNeedPressure(NeedType.Food);
        var utility = hunger * 10d + (double)household.FoodDemand.Value;
        return new AgentDecision(AgentActionType.BuyFood, utility, "Food need is pressing.");
    }

    private static AgentDecision EvaluateReturnHome(PersonAgent person, HouseholdAgent household)
    {
        if (household.HomeLocation is null)
        {
            return new AgentDecision(AgentActionType.ReturnHome, 0d, "No home location available.");
        }

        var utility = person.CurrentLocation.Equals(household.HomeLocation.Value) ? 0.5d : 5d;
        if (person.CurrentActivity == AgentActivity.AtHome)
        {
            utility -= 0.5d;
        }

        return new AgentDecision(AgentActionType.ReturnHome, utility, "Returning home is sensible.");
    }

    private static AgentDecision EvaluateSeekEmployment(PersonAgent person)
    {
        if (person.Age < 16 || person.EmploymentState == EmploymentState.Employed)
        {
            return new AgentDecision(AgentActionType.SeekEmployment, 0d, "Already employed or too young.");
        }

        var utility = 4d + person.Attributes.Diligence * 2d + person.GetNeedPressure(NeedType.Income) * 4d;
        if (person.EmploymentState == EmploymentState.Seeking)
        {
            utility += 1d;
        }

        return new AgentDecision(AgentActionType.SeekEmployment, utility, "Employment would improve stability.");
    }

    private static AgentDecision EvaluateGoToWork(PersonAgent person, SimulationTime currentTime)
    {
        if (person.EmploymentState != EmploymentState.Employed)
        {
            return new AgentDecision(AgentActionType.GoToWork, 0d, "Not employed.");
        }

        var hour = currentTime.Hour;
        var withinWorkHours = hour is >= 8 and < 18;
        var utility = withinWorkHours ? 7d : 1d;

        if (person.WorkplaceLocation is not null && person.CurrentLocation.Equals(person.WorkplaceLocation.Value))
        {
            utility -= 2d;
        }

        if (person.Routine.GetPreferredAction(hour) == AgentActionType.GoToWork)
        {
            utility += 2d;
        }

        return new AgentDecision(AgentActionType.GoToWork, utility, "Work hours or routine encourage work.");
    }

    private static AgentDecision EvaluateSocialise(PersonAgent person)
    {
        var utility = person.GetNeedPressure(NeedType.Social) * 6d + person.Attributes.Sociability * 2d;
        return new AgentDecision(AgentActionType.Socialise, utility, "Social need can improve satisfaction.");
    }

    private static AgentDecision EvaluateRest(PersonAgent person)
    {
        var utility = person.GetNeedPressure(NeedType.Recreation) * 2d + (1d - person.Attributes.Energy) * 2d;
        if (person.CurrentActivity == AgentActivity.Resting)
        {
            utility += 1d;
        }

        return new AgentDecision(AgentActionType.Rest, utility, "Rest can restore the agent.");
    }

    private static AgentDecision EvaluateStudy(PersonAgent person)
    {
        if (person.LifeStage is not (LifeStage.Child or LifeStage.Teenager or LifeStage.YoungAdult))
        {
            return new AgentDecision(AgentActionType.Study, 0d, "Study is not a strong option right now.");
        }

        var utility = person.GetNeedPressure(NeedType.Education) * 5d;
        if (person.Routine.GetPreferredAction(0) == AgentActionType.Study)
        {
            utility += 0.5d;
        }

        return new AgentDecision(AgentActionType.Study, utility, "Education has value for younger agents.");
    }
}
