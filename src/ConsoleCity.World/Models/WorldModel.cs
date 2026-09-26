using ConsoleCity.Core;

namespace ConsoleCity.World;

/// <summary>
/// The world keeps explicit containment and a simple spatial grid so the model stays serializable and easy to test.
/// </summary>
public sealed record class WorldModel
{
    public WorldId Id { get; }

    public int Seed { get; }

    public SimulationTime CreatedAt { get; }

    public IReadOnlyList<RegionModel> Regions { get; }

    public IReadOnlyList<TerrainCellModel> Cells { get; }

    public EnvironmentalStateModel Environment { get; }

    public WorldModel(
        WorldId id,
        int seed,
        SimulationTime createdAt,
        IReadOnlyList<RegionModel> regions,
        IReadOnlyList<TerrainCellModel> cells,
        EnvironmentalStateModel environment)
    {
        ArgumentNullException.ThrowIfNull(regions);
        ArgumentNullException.ThrowIfNull(cells);

        Id = id;
        Seed = seed;
        CreatedAt = createdAt;
        Regions = regions;
        Cells = cells;
        Environment = environment;
    }
}
