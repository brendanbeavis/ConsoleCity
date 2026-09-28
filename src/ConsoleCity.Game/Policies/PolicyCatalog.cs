namespace ConsoleCity.Game;

public static class PolicyCatalog
{
    private static readonly IReadOnlyList<GamePolicyDefinition> AllPolicies = new[]
    {
        new GamePolicyDefinition(
            new GamePolicyId("development-incentives"),
            "Development Incentives",
            GamePolicyCategory.Development,
            "Encourage growth by reducing strategic construction friction.",
            [new GamePolicyEffect(PolicyTargets.ConstructionCost, ModifierEffectKind.Multiplicative, 0.90m)],
            Array.Empty<TechnologyId>()),
        new GamePolicyDefinition(
            new GamePolicyId("education-investment"),
            "Education Investment",
            GamePolicyCategory.Education,
            "Improve research throughput through sustained civic investment.",
            [new GamePolicyEffect(PolicyTargets.ResearchGain, ModifierEffectKind.Multiplicative, 1.15m)],
            Array.Empty<TechnologyId>()),
        new GamePolicyDefinition(
            new GamePolicyId("environmental-protection"),
            "Environmental Protection",
            GamePolicyCategory.Environment,
            "Reduce the severity of environmental incidents.",
            [new GamePolicyEffect(PolicyTargets.EventSeverity, ModifierEffectKind.Multiplicative, 0.85m)],
            Array.Empty<TechnologyId>()),
        new GamePolicyDefinition(
            new GamePolicyId("migration-facilitation"),
            "Migration Facilitation",
            GamePolicyCategory.Migration,
            "Make the settlement more attractive to incoming households.",
            [new GamePolicyEffect(PolicyTargets.MigrationAttractiveness, ModifierEffectKind.Multiplicative, 1.10m)],
            Array.Empty<TechnologyId>())
    };

    public static IReadOnlyList<GamePolicyDefinition> All => AllPolicies;

    public static GamePolicyDefinition Get(GamePolicyId id)
        => AllPolicies.FirstOrDefault(policy => policy.Id == id)
            ?? throw new KeyNotFoundException($"Policy '{id}' was not found.");

    public static IReadOnlyList<GamePolicyDefinition> GetAvailablePolicies(GameProgressionState progression)
        => AllPolicies.Where(policy => policy.IsEligible(progression)).ToList();
}