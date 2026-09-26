namespace ConsoleCity.Agents;

public readonly record struct AgentAction
{
    public AgentActionType ActionType { get; }

    public string Reason { get; }

    public AgentAction(AgentActionType actionType, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("An action needs a reason.", nameof(reason));
        }

        ActionType = actionType;
        Reason = reason.Trim();
    }
}
