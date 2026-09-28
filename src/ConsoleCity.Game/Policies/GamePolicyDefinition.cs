namespace ConsoleCity.Game;

public sealed record class GamePolicyDefinition
{
    public GamePolicyId Id { get; }
    public string Name { get; }
    public GamePolicyCategory Category { get; }
    public string Description { get; }
    public IReadOnlyList<GamePolicyEffect> Effects { get; }
    public IReadOnlyList<TechnologyId> RequiredTechnologies { get; }

    public GamePolicyDefinition(GamePolicyId id, string name, GamePolicyCategory category, string description, IReadOnlyList<GamePolicyEffect>? effects, IReadOnlyList<TechnologyId>? requiredTechnologies)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Policy name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Policy description cannot be empty.", nameof(description));
        Id = id;
        Name = name.Trim();
        Category = category;
        Description = description.Trim();
        Effects = effects ?? Array.Empty<GamePolicyEffect>();
        RequiredTechnologies = requiredTechnologies ?? Array.Empty<TechnologyId>();
    }

    public bool IsEligible(GameProgressionState progression) => RequiredTechnologies.All(progression.HasTechnology);
}