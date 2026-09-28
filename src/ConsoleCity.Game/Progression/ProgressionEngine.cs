using ConsoleCity.Core;

namespace ConsoleCity.Game;

public static class ProgressionEngine
{
    private static readonly IReadOnlyList<ProgressionMilestoneDefinition> Milestones = new[]
    {
        new ProgressionMilestoneDefinition(
            new MilestoneId("first-business"),
            "First Operating Business",
            "A business has started operating in the settlement.",
            10,
            2m,
            snapshot => snapshot.Economy.Businesses.Count > 0),
        new ProgressionMilestoneDefinition(
            new MilestoneId("population-10"),
            "Growing Settlement",
            "The population has reached 10 people.",
            20,
            5m,
            snapshot => snapshot.Population.People.Count >= 10),
        new ProgressionMilestoneDefinition(
            new MilestoneId("population-25"),
            "Small Town",
            "The population has reached 25 people.",
            35,
            8m,
            snapshot => snapshot.Population.People.Count >= 25),
        new ProgressionMilestoneDefinition(
            new MilestoneId("population-50"),
            "Established City",
            "The population has reached 50 people.",
            50,
            12m,
            snapshot => snapshot.Population.People.Count >= 50)
    };

    public static GameProgressionState CreateInitialProgression(SimulationTime currentTime)
        => GameProgressionState.CreateInitial(currentTime);

    public static GameProgressionState RebuildFromSnapshot(SimulationSliceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return Advance(CreateInitialProgression(snapshot.CurrentTime), snapshot);
    }

    public static GameProgressionState Advance(GameProgressionState progression, SimulationSliceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(progression);
        ArgumentNullException.ThrowIfNull(snapshot);

        var updated = progression.AddResearchPoints(CalculateResearchGain(progression, snapshot), snapshot.CurrentTime, "Ongoing civic and economic activity");

        foreach (var milestone in Milestones)
        {
            if (updated.IsMilestoneCompleted(milestone.Id) || !milestone.Condition(snapshot))
            {
                continue;
            }

            updated = updated.CompleteMilestone(new ProgressionMilestoneCompletion(milestone.Id, milestone.Name, snapshot.CurrentTime, milestone.CreditReward, milestone.ResearchReward));
        }

        updated = ModifierEngine.Advance(updated, snapshot.CurrentTime);
        updated = PolicyEngine.Advance(updated, snapshot.CurrentTime);

        var generatedEvents = EventEngine.GenerateEvents(snapshot);
        if (generatedEvents.Count > 0)
        {
            updated = updated.WithEventLog(updated.EventLog.Concat(generatedEvents).ToList());
            updated = updated with { Log = updated.Log.Concat(generatedEvents.Select(ev => new ProgressionLogEntry(ev.OccurredAt, ev.Category.ToString().ToLowerInvariant(), ev.Description))).ToList() };
        }

        return updated;
    }

    public static IReadOnlyList<ProgressionMilestoneDefinition> GetMilestones() => Milestones;

    public static IReadOnlyList<TechnologyDefinition> GetAvailableTechnologies(GameProgressionState progression)
        => TechnologyCatalog.GetAvailableTechnologies(progression);

    public static bool TryResearchTechnology(GameProgressionState progression, TechnologyId technologyId, SimulationTime currentTime, out GameProgressionState updatedState)
        => progression.TryResearchTechnology(technologyId, currentTime, out updatedState);

    private static decimal CalculateResearchGain(GameProgressionState progression, SimulationSliceSnapshot snapshot)
    {
        var populationFactor = snapshot.Population.People.Count / 40m;
        var businessFactor = snapshot.Economy.Businesses.Count / 4m;
        var activityFactor = Math.Min(2m, snapshot.CompletedTrips / 100m);
        var baseGain = 0.25m + populationFactor + businessFactor + activityFactor;
        baseGain = ModifierEngine.ResolveDecimal(progression, ModifierTargets.ResearchGain, baseGain);
        return PolicyEngine.ResolveDecimal(progression, PolicyTargets.ResearchGain, baseGain);
    }
}