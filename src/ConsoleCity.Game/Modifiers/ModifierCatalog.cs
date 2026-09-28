namespace ConsoleCity.Game;

public static class ModifierCatalog
{
    private static readonly IReadOnlyList<GameModifierDefinition> AllModifiers = new[]
    {
        new GameModifierDefinition(
            new GameModifierId("research-grants"),
            "Research Grants",
            GameModifierCategory.Research,
            "Research generation increases as the city invests in its knowledge base.",
            10m,
            null,
            ModifierStackingRule.Stack,
            ModifierAcquisitionMethod.Purchase,
            Array.Empty<TechnologyId>(),
            Array.Empty<string>(),
            [new GameModifierEffect(ModifierTargets.ResearchGain, ModifierEffectKind.Multiplicative, 1.15m)]),
        new GameModifierDefinition(
            new GameModifierId("civic-momentum"),
            "Civic Momentum",
            GameModifierCategory.EventsEnvironment,
            "Temporary civic enthusiasm boosts strategic research output.",
            6m,
            24,
            ModifierStackingRule.RefreshDuration,
            ModifierAcquisitionMethod.Purchase,
            Array.Empty<TechnologyId>(),
            Array.Empty<string>(),
            [new GameModifierEffect(ModifierTargets.ResearchGain, ModifierEffectKind.Multiplicative, 1.10m)]),
        new GameModifierDefinition(
            new GameModifierId("compact-planning"),
            "Compact Planning",
            GameModifierCategory.Construction,
            "Construction costs are reduced for denser development strategies.",
            8m,
            null,
            ModifierStackingRule.Unique,
            ModifierAcquisitionMethod.Purchase,
            [new TechnologyId("basic-construction")],
            Array.Empty<string>(),
            [new GameModifierEffect(ModifierTargets.ConstructionCost, ModifierEffectKind.Multiplicative, 0.90m)]),
        new GameModifierDefinition(
            new GameModifierId("resilient-infrastructure"),
            "Resilient Infrastructure",
            GameModifierCategory.Infrastructure,
            "Infrastructure failures are less severe and repair faster.",
            12m,
            48,
            ModifierStackingRule.RefreshDuration,
            ModifierAcquisitionMethod.Purchase,
            [new TechnologyId("basic-utilities")],
            Array.Empty<string>(),
            [new GameModifierEffect(ModifierTargets.InfrastructureFailureSeverity, ModifierEffectKind.Multiplicative, 0.70m)]),
        new GameModifierDefinition(
            new GameModifierId("skilled-workforce"),
            "Skilled Workforce",
            GameModifierCategory.Agents,
            "Workers gain skills more quickly from the city’s institutions.",
            10m,
            null,
            ModifierStackingRule.Stack,
            ModifierAcquisitionMethod.Purchase,
            Array.Empty<TechnologyId>(),
            Array.Empty<string>(),
            [new GameModifierEffect(ModifierTargets.AgentSkillGain, ModifierEffectKind.Multiplicative, 1.10m)]),
        new GameModifierDefinition(
            new GameModifierId("industrial-giant"),
            "Industrial Giant",
            GameModifierCategory.Specialisation,
            "Industrial output rises at the cost of additional environmental pressure.",
            14m,
            72,
            ModifierStackingRule.Unique,
            ModifierAcquisitionMethod.Purchase,
            [new TechnologyId("mechanised-farming")],
            Array.Empty<string>(),
            [
                new GameModifierEffect(ModifierTargets.ProductionOutput, ModifierEffectKind.Multiplicative, 1.20m),
                new GameModifierEffect(ModifierTargets.EventSeverity, ModifierEffectKind.Multiplicative, 1.10m)
            ])
    };

    public static IReadOnlyList<GameModifierDefinition> All => AllModifiers;

    public static GameModifierDefinition Get(GameModifierId id)
        => AllModifiers.FirstOrDefault(modifier => modifier.Id == id)
            ?? throw new KeyNotFoundException($"Modifier '{id}' was not found.");

    public static IReadOnlyList<GameModifierDefinition> GetAvailableModifiers(GameProgressionState progression)
        => AllModifiers.Where(modifier => modifier.IsEligible(progression)).ToList();
}
