namespace ConsoleCity.Agents;

public sealed record AgentActivityProfile
{
    public AgentActivity CurrentActivity { get; }

    public string? CurrentGoal { get; }

    public AgentActivityProfile(AgentActivity currentActivity, string? currentGoal = null)
    {
        CurrentActivity = currentActivity;
        CurrentGoal = currentGoal;
    }
}
