using ConsoleCity.Core;

namespace ConsoleCity.Game;

public sealed record class ProgressionMilestoneCompletion(
    MilestoneId Id,
    string Name,
    SimulationTime CompletedAt,
    int CreditReward,
    decimal ResearchReward);