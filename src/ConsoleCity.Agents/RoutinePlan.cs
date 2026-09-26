namespace ConsoleCity.Agents;

public sealed record class RoutinePlan
{
    public IReadOnlyList<RoutineBlock> Blocks { get; }

    public RoutinePlan(IReadOnlyList<RoutineBlock> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);
        Blocks = blocks;
    }

    public AgentActionType? GetPreferredAction(int hour)
    {
        foreach (var block in Blocks)
        {
            if (block.ContainsHour(hour))
            {
                return block.PreferredAction;
            }
        }

        return null;
    }

    public static RoutinePlan Empty { get; } = new(Array.Empty<RoutineBlock>());
}
