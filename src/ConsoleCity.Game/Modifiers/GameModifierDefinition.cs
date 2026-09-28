namespace ConsoleCity.Game;

public sealed record class GameModifierDefinition
{
    public GameModifierId Id { get; }

    public string Name { get; }

    public GameModifierCategory Category { get; }

    public string Description { get; }

    public decimal CreditCost { get; }

    public int? DurationTicks { get; }

    public ModifierStackingRule StackingRule { get; }

    public ModifierAcquisitionMethod AcquisitionMethod { get; }

    public IReadOnlyList<TechnologyId> RequiredTechnologies { get; }

    public IReadOnlyList<string> RequiredCapabilities { get; }

    public IReadOnlyList<GameModifierEffect> Effects { get; }

    public GameModifierDefinition(
        GameModifierId id,
        string name,
        GameModifierCategory category,
        string description,
        decimal creditCost,
        int? durationTicks,
        ModifierStackingRule stackingRule,
        ModifierAcquisitionMethod acquisitionMethod,
        IReadOnlyList<TechnologyId>? requiredTechnologies,
        IReadOnlyList<string>? requiredCapabilities,
        IReadOnlyList<GameModifierEffect>? effects)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Modifier name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Modifier description cannot be empty.", nameof(description));
        }

        if (creditCost < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(creditCost));
        }

        if (durationTicks is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(durationTicks));
        }

        Id = id;
        Name = name.Trim();
        Category = category;
        Description = description.Trim();
        CreditCost = creditCost;
        DurationTicks = durationTicks;
        StackingRule = stackingRule;
        AcquisitionMethod = acquisitionMethod;
        RequiredTechnologies = requiredTechnologies ?? Array.Empty<TechnologyId>();
        RequiredCapabilities = requiredCapabilities ?? Array.Empty<string>();
        Effects = effects ?? Array.Empty<GameModifierEffect>();
    }

    public bool IsEligible(GameProgressionState progression)
    {
        ArgumentNullException.ThrowIfNull(progression);
        return RequiredTechnologies.All(progression.HasTechnology)
            && RequiredCapabilities.All(progression.HasCapability);
    }
}