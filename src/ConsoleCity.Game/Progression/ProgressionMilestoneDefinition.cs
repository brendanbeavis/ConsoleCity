namespace ConsoleCity.Game;

public sealed record class ProgressionMilestoneDefinition(
    MilestoneId Id,
    string Name,
    string Description,
    int CreditReward,
    decimal ResearchReward,
    Func<SimulationSliceSnapshot, bool> Condition);