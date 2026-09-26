using ConsoleCity.Core;

namespace ConsoleCity.World;

public sealed record class TerrainCellModel
{
    public GridPosition Position { get; }

    public TerrainType TerrainType { get; }

    public int Elevation { get; }

    public bool IsWater { get; }

    public bool HasRoad { get; }

    public bool HasRail { get; }

    public bool HasPath { get; }

    public double DevelopmentSuitability { get; }

    public IReadOnlyList<ResourceDepositModel> ResourceDeposits { get; }

    public PlotId? PlotId { get; }

    public BuildingId? BuildingId { get; }

    public SimulationTime CreatedAt { get; }

    public TerrainCellModel(
        GridPosition position,
        TerrainType terrainType,
        int elevation,
        bool isWater,
        bool hasRoad,
        bool hasRail,
        bool hasPath,
        double developmentSuitability,
        IReadOnlyList<ResourceDepositModel> resourceDeposits,
        PlotId? plotId,
        BuildingId? buildingId,
        SimulationTime createdAt)
    {
        if (!double.IsFinite(developmentSuitability) || developmentSuitability is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(developmentSuitability), "Development suitability must be a finite fraction between 0 and 1.");
        }

        ArgumentNullException.ThrowIfNull(resourceDeposits);

        Position = position;
        TerrainType = terrainType;
        Elevation = elevation;
        IsWater = isWater;
        HasRoad = hasRoad;
        HasRail = hasRail;
        HasPath = hasPath;
        DevelopmentSuitability = developmentSuitability;
        ResourceDeposits = resourceDeposits;
        PlotId = plotId;
        BuildingId = buildingId;
        CreatedAt = createdAt;
    }
}
