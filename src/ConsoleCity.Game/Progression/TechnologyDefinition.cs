namespace ConsoleCity.Game;

public sealed record class TechnologyDefinition
{
    public TechnologyId Id { get; }

    public string Name { get; }

    public TechnologyCategory Category { get; }

    public string Description { get; }

    public decimal ResearchCost { get; }

    public IReadOnlyList<TechnologyId> Prerequisites { get; }

    public IReadOnlyList<string> Unlocks { get; }

    public TechnologyDefinition(
        TechnologyId id,
        string name,
        TechnologyCategory category,
        string description,
        decimal researchCost,
        IReadOnlyList<TechnologyId>? prerequisites,
        IReadOnlyList<string>? unlocks)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Technology name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Technology description cannot be empty.", nameof(description));
        }

        if (researchCost < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(researchCost));
        }

        Id = id;
        Name = name.Trim();
        Category = category;
        Description = description.Trim();
        ResearchCost = researchCost;
        Prerequisites = prerequisites ?? Array.Empty<TechnologyId>();
        Unlocks = unlocks ?? Array.Empty<string>();
    }
}
