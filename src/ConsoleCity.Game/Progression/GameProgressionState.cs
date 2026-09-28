using ConsoleCity.Core;

namespace ConsoleCity.Game;

public sealed record class GameProgressionState(
    int DevelopmentCredits,
    decimal ResearchPoints,
    IReadOnlyList<TechnologyId> UnlockedTechnologies,
    IReadOnlyList<ProgressionMilestoneCompletion> CompletedMilestones,
    IReadOnlyList<ProgressionLogEntry> Log,
    IReadOnlyList<GameModifierInstance> ActiveModifiers,
    IReadOnlyList<GamePolicyInstance> ActivePolicies,
    GameCycleState CycleState,
    IReadOnlyList<GameEventRecord> EventLog)
{
    public static GameProgressionState CreateInitial(SimulationTime currentTime)
        => new(
            20,
            5m,
            Array.Empty<TechnologyId>(),
            [new ProgressionMilestoneCompletion(new MilestoneId("founding-settlement"), "Founding Settlement", currentTime, 20, 5m)],
            [new ProgressionLogEntry(currentTime, "founding", "The settlement has been founded and the first civic credits have been awarded.")],
            Array.Empty<GameModifierInstance>(),
            Array.Empty<GamePolicyInstance>(),
            GameCycleState.CreateInitial(currentTime),
            Array.Empty<GameEventRecord>());

    public bool HasTechnology(TechnologyId technologyId)
        => UnlockedTechnologies.Contains(technologyId);

    public bool HasCapability(string capabilityId)
        => TechnologyCatalog.GetUnlockedCapabilities(this).Any(capability => string.Equals(capability, capabilityId, StringComparison.OrdinalIgnoreCase));

    public bool CanResearch(TechnologyDefinition technology)
        => !HasTechnology(technology.Id)
            && ResearchPoints >= technology.ResearchCost
            && technology.Prerequisites.All(HasTechnology);

    public bool IsMilestoneCompleted(MilestoneId milestoneId)
        => CompletedMilestones.Any(milestone => milestone.Id == milestoneId);

    public bool HasModifier(GameModifierId modifierId)
        => ActiveModifiers.Any(modifier => modifier.Id == modifierId);

    public bool HasPolicy(GamePolicyId policyId)
        => ActivePolicies.Any(policy => policy.Id == policyId);

    public GameProgressionState AddCredits(int amount, SimulationTime at, string reason)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        return this with
        {
            DevelopmentCredits = DevelopmentCredits + amount,
            Log = AppendLog(new ProgressionLogEntry(at, "credits", $"{reason} (+{amount})"))
        };
    }

    public GameProgressionState AddResearchPoints(decimal amount, SimulationTime at, string reason)
    {
        if (amount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        return this with
        {
            ResearchPoints = ResearchPoints + amount,
            Log = AppendLog(new ProgressionLogEntry(at, "research", $"{reason} (+{amount:0.##})"))
        };
    }

    public GameProgressionState CompleteMilestone(ProgressionMilestoneCompletion completion)
    {
        if (IsMilestoneCompleted(completion.Id))
        {
            return this;
        }

        var updated = this with
        {
            CompletedMilestones = CompletedMilestones.Concat([completion]).ToList(),
            DevelopmentCredits = DevelopmentCredits + completion.CreditReward,
            ResearchPoints = ResearchPoints + completion.ResearchReward,
            Log = AppendLog(new ProgressionLogEntry(completion.CompletedAt, "milestone", $"{completion.Name} unlocked (+{completion.CreditReward} credits, +{completion.ResearchReward:0.##} research)"))
        };

        return updated;
    }

    public bool TryResearchTechnology(TechnologyId technologyId, SimulationTime at, out GameProgressionState updatedState)
    {
        var definition = TechnologyCatalog.Get(technologyId);
        if (!CanResearch(definition))
        {
            updatedState = this;
            return false;
        }

        updatedState = this with
        {
            ResearchPoints = ResearchPoints - definition.ResearchCost,
            UnlockedTechnologies = UnlockedTechnologies.Concat([technologyId]).Distinct().ToList(),
            Log = AppendLog(new ProgressionLogEntry(at, "technology", $"{definition.Name} researched (-{definition.ResearchCost:0.##} research)"))
        };

        return true;
    }

    public GameProgressionState WithActivePolicies(IReadOnlyList<GamePolicyInstance> policies)
        => this with { ActivePolicies = policies ?? Array.Empty<GamePolicyInstance>() };

    public GameProgressionState WithCycleState(GameCycleState cycleState)
        => this with { CycleState = cycleState };

    public GameProgressionState WithEventLog(IReadOnlyList<GameEventRecord> eventLog)
        => this with { EventLog = eventLog ?? Array.Empty<GameEventRecord>() };

    public GameProgressionState WithModifiers(IReadOnlyList<GameModifierInstance> modifiers)
        => this with { ActiveModifiers = modifiers ?? Array.Empty<GameModifierInstance>() };

    private IReadOnlyList<ProgressionLogEntry> AppendLog(ProgressionLogEntry entry)
    {
        var entries = Log.Concat([entry]).ToList();
        const int maxEntries = 100;
        return entries.Count <= maxEntries ? entries : entries.Skip(entries.Count - maxEntries).ToList();
    }
}
