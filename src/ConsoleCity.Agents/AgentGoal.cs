using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public sealed record AgentGoal
{
    public GoalType GoalType { get; }

    public string Description { get; }

    public double Priority { get; }

    public SimulationTime? DueBy { get; }

    public AgentGoal(GoalType goalType, string description, double priority, SimulationTime? dueBy = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Goal description cannot be empty.", nameof(description));
        }

        if (!double.IsFinite(priority) || priority is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(priority), "Goal priority must be a finite fraction between 0 and 1.");
        }

        GoalType = goalType;
        Description = description.Trim();
        Priority = priority;
        DueBy = dueBy;
    }
}
