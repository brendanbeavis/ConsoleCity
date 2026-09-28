using ConsoleCity.Core;

namespace ConsoleCity.World;

/// <summary>
/// Represents the contents of a single grid cell in the map.
/// A cell can contain terrain, buildings, roads, water, or be empty/unassigned.
/// </summary>
public sealed record class MapCell
{
    /// <summary>
    /// The grid position of this cell.
    /// </summary>
    public GridPosition Position { get; }

    /// <summary>
    /// The terrain type at this location.
    /// </summary>
    public TerrainType Terrain { get; }

    /// <summary>
    /// The ID of the building at this location, if any.
    /// </summary>
    public BuildingId? BuildingId { get; }

    /// <summary>
    /// The type of building at this location, if any.
    /// </summary>
    public BuildingType? BuildingType { get; }

    /// <summary>
    /// The ID of the plot at this location, if any.
    /// </summary>
    public PlotId? PlotId { get; }

    /// <summary>
    /// The land use type of the plot, if any.
    /// </summary>
    public LandUseType? LandUse { get; }

    /// <summary>
    /// True if this cell has a road.
    /// </summary>
    public bool HasRoad { get; }

    /// <summary>
    /// True if this cell has a rail line.
    /// </summary>
    public bool HasRail { get; }

    /// <summary>
    /// True if this cell is water.
    /// </summary>
    public bool IsWater { get; }

    /// <summary>
    /// True if this cell is elevated (relative to sea level).
    /// </summary>
    public int Elevation { get; }

    /// <summary>
    /// Display character for console rendering.
    /// </summary>
    public char DisplayChar => DetermineDisplayChar();

    public MapCell(
        GridPosition position,
        TerrainType terrain,
        BuildingId? buildingId = null,
        BuildingType? buildingType = null,
        PlotId? plotId = null,
        LandUseType? landUse = null,
        bool hasRoad = false,
        bool hasRail = false,
        bool isWater = false,
        int elevation = 0)
    {
        Position = position;
        Terrain = terrain;
        BuildingId = buildingId;
        BuildingType = buildingType;
        PlotId = plotId;
        LandUse = landUse;
        HasRoad = hasRoad;
        HasRail = hasRail;
        IsWater = isWater;
        Elevation = elevation;
    }

    /// <summary>
    /// Determine the display character for this cell based on its contents.
    /// </summary>
    private char DetermineDisplayChar()
    {
        // Priority: building > road > rail > water > terrain
        if (BuildingId.HasValue)
        {
            return BuildingType switch
            {
                World.BuildingType.House => 'H',
                World.BuildingType.Apartment => 'A',
                World.BuildingType.Shop => 'S',
                World.BuildingType.Office => 'O',
                World.BuildingType.Factory => 'F',
                World.BuildingType.Farm => 'F',
                World.BuildingType.School => 'E',
                World.BuildingType.PoliceStation => 'P',
                World.BuildingType.FireStation => 'R',
                World.BuildingType.Park => '.',
                World.BuildingType.UtilityFacility => 'U',
                World.BuildingType.TransportFacility => 'T',
                _ => 'B'
            };
        }

        if (HasRoad && HasRail)
            return '+';
        if (HasRail)
            return '|';
        if (HasRoad)
            return '-';

        if (IsWater)
            return '~';

        return Terrain switch
        {
            TerrainType.Plains => '.',
            TerrainType.Forest => '^',
            TerrainType.Hills => 'h',
            TerrainType.Mountains => '^',
            TerrainType.Desert => '`',
            TerrainType.Coast => '~',
            TerrainType.River => '~',
            TerrainType.Lake => '~',
            TerrainType.Ocean => '~',
            _ => '?'
        };
    }

    /// <summary>
    /// Get display text for this cell (building name, plot info, etc.)
    /// </summary>
    public string GetDisplayText()
    {
        if (BuildingId.HasValue)
        {
            return $"{BuildingType} @ {Position}";
        }

        if (PlotId.HasValue)
        {
            return $"Plot ({LandUse}) @ {Position}";
        }

        if (HasRoad)
        {
            return $"Road @ {Position}";
        }

        return $"{Terrain} @ {Position}";
    }

    /// <summary>
    /// True if this cell contains an interactive object (building or plot).
    /// </summary>
    public bool IsInteractive => BuildingId.HasValue || PlotId.HasValue;
}
