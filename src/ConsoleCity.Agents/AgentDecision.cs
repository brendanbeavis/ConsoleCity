namespace ConsoleCity.Agents;

public readonly record struct AgentDecision
{
    public AgentActionType ActionType { get; }

    public double Utility { get; }

    public string Reason { get; }

    public AgentDecision(AgentActionType actionType, double utility, string reason)
    {
        if (!double.IsFinite(utility))
        {
            throw new ArgumentOutOfRangeException(nameof(utility), "Utility must be finite.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A decision needs a reason.", nameof(reason));
        }

        ActionType = actionType;
        Utility = utility;
        Reason = reason.Trim();
    }

    public AgentAction ToAction() => new(ActionType, Reason);
}
