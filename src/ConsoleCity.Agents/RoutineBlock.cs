namespace ConsoleCity.Agents;

public readonly record struct RoutineBlock
{
    public int StartHour { get; }

    public int EndHour { get; }

    public AgentActionType PreferredAction { get; }

    public RoutineBlock(int startHour, int endHour, AgentActionType preferredAction)
    {
        if (startHour is < 0 or > 23)
        {
            throw new ArgumentOutOfRangeException(nameof(startHour));
        }

        if (endHour is < 1 or > 24)
        {
            throw new ArgumentOutOfRangeException(nameof(endHour));
        }

        if (endHour <= startHour)
        {
            throw new ArgumentException("Routine blocks must have a positive duration.", nameof(endHour));
        }

        StartHour = startHour;
        EndHour = endHour;
        PreferredAction = preferredAction;
    }

    public bool ContainsHour(int hour) => hour >= StartHour && hour < EndHour;
}
