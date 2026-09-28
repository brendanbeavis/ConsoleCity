namespace ConsoleCity.Game;

public static class TechnologyCatalog
{
    private static readonly IReadOnlyList<TechnologyDefinition> AllTechnologies = new[]
    {
        new TechnologyDefinition(
            new TechnologyId("basic-construction"),
            "Basic Construction",
            TechnologyCategory.Foundations,
            "Establishes a formal construction process.",
            8m,
            Array.Empty<TechnologyId>(),
            ["construction:basic", "building:house", "building:road"]),
        new TechnologyDefinition(
            new TechnologyId("basic-utilities"),
            "Basic Utilities",
            TechnologyCategory.Foundations,
            "Provides a foundation for service and utility systems.",
            8m,
            Array.Empty<TechnologyId>(),
            ["utilities:basic", "service:water", "service:power"]),
        new TechnologyDefinition(
            new TechnologyId("road-engineering"),
            "Road Engineering",
            TechnologyCategory.Transport,
            "Improves road planning and maintenance.",
            10m,
            [new TechnologyId("basic-construction")],
            ["transport:roads", "construction:roads"]),
        new TechnologyDefinition(
            new TechnologyId("sanitation"),
            "Sanitation",
            TechnologyCategory.Foundations,
            "Unlocks sanitation-focused civic planning.",
            12m,
            [new TechnologyId("basic-utilities")],
            ["service:sanitation", "policy:public-health"]),
        new TechnologyDefinition(
            new TechnologyId("grid-optimisation"),
            "Grid Optimisation",
            TechnologyCategory.Energy,
            "Improves the effectiveness of connected utility networks.",
            16m,
            [new TechnologyId("basic-utilities")],
            ["modifier:efficient-grid", "infrastructure:power-grid"]),
        new TechnologyDefinition(
            new TechnologyId("mechanised-farming"),
            "Mechanised Farming",
            TechnologyCategory.Agriculture,
            "Improves agricultural throughput and labour efficiency.",
            18m,
            [new TechnologyId("basic-construction")],
            ["modifier:agricultural-science", "building:farm"]),
        new TechnologyDefinition(
            new TechnologyId("telecommunications"),
            "Telecommunications",
            TechnologyCategory.ComputingAndCommunications,
            "Expands long-distance coordination and information flow.",
            20m,
            [new TechnologyId("road-engineering")],
            ["service:communications", "policy:planning"]),
        new TechnologyDefinition(
            new TechnologyId("high-density-construction"),
            "High-Density Construction",
            TechnologyCategory.UrbanDevelopment,
            "Allows denser urban development and taller structures.",
            24m,
            [new TechnologyId("basic-construction"), new TechnologyId("road-engineering")],
            ["building:high-density", "district:urban-core"])
    };

    public static IReadOnlyList<TechnologyDefinition> All => AllTechnologies;

    public static TechnologyDefinition Get(TechnologyId id)
        => AllTechnologies.FirstOrDefault(technology => technology.Id == id)
            ?? throw new KeyNotFoundException($"Technology '{id}' was not found.");

    public static IReadOnlyList<TechnologyDefinition> GetAvailableTechnologies(GameProgressionState state)
        => AllTechnologies.Where(technology => state.CanResearch(technology)).ToList();

    public static IReadOnlyList<string> GetUnlockedCapabilities(GameProgressionState state)
        => state.UnlockedTechnologies
            .SelectMany(technologyId => Get(technologyId).Unlocks)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
