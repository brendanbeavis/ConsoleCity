using ConsoleCity.Core;

namespace ConsoleCity.World;

/// <summary>
/// Derives a spatial map view from the world model.
/// Queries world entities on-demand and projects them onto a 2D grid.
/// </summary>
public sealed class MapView
{
    private readonly WorldModel world;
    private readonly Dictionary<GridPosition, MapCell> cellCache = new();

    public WorldModel World => world;

    /// <summary>
    /// Get the bounds of the known world (from terrain cells).
    /// </summary>
    public (GridPosition Min, GridPosition Max) Bounds { get; }

    public MapView(WorldModel world)
    {
        ArgumentNullException.ThrowIfNull(world);
        this.world = world;

        // Calculate bounds from terrain cells
        if (world.Cells.Count == 0)
        {
            Bounds = (new GridPosition(0, 0), new GridPosition(0, 0));
        }
        else
        {
            var minX = world.Cells.Min(c => c.Position.X);
            var minY = world.Cells.Min(c => c.Position.Y);
            var maxX = world.Cells.Max(c => c.Position.X);
            var maxY = world.Cells.Max(c => c.Position.Y);
            Bounds = (new GridPosition(minX, minY), new GridPosition(maxX, maxY));
        }
    }

    /// <summary>
    /// Get the map cell at a specific position.
    /// </summary>
    public MapCell GetCell(GridPosition position)
    {
        // Check cache first
        if (cellCache.TryGetValue(position, out var cached))
        {
            return cached;
        }

        // Project from world entities
        var cell = ProjectCell(position);
        cellCache[position] = cell;
        return cell;
    }

    /// <summary>
    /// Invalidate cache (call after world changes).
    /// </summary>
    public void InvalidateCache()
    {
        cellCache.Clear();
    }

    /// <summary>
    /// Get all cells in a rectangular region.
    /// </summary>
    public IReadOnlyList<MapCell> GetCells(GridPosition topLeft, GridPosition bottomRight)
    {
        var cells = new List<MapCell>();
        for (int x = topLeft.X; x <= bottomRight.X; x++)
        {
            for (int y = topLeft.Y; y <= bottomRight.Y; y++)
            {
                cells.Add(GetCell(new GridPosition(x, y)));
            }
        }
        return cells;
    }

    /// <summary>
    /// Get all cells for the entire world.
    /// </summary>
    public IReadOnlyList<MapCell> GetAllCells()
    {
        return GetCells(Bounds.Min, Bounds.Max);
    }

    /// <summary>
    /// Project a world cell onto a map cell.
    /// </summary>
    private MapCell ProjectCell(GridPosition position)
    {
        // Find terrain cell
        var terrainCell = world.Cells.FirstOrDefault(c => c.Position == position);
        if (terrainCell == null)
        {
            // Position outside world
            return new MapCell(
                position,
                TerrainType.Ocean, // Default to ocean for unknown areas
                isWater: true);
        }

        // Find plot and building at this location
        var allPlots = world.Regions
            .SelectMany(r => r.Cities)
            .SelectMany(c => c.Districts)
            .SelectMany(d => d.Plots)
            .ToList();

        var plot = allPlots.FirstOrDefault(p => p.Cells.Contains(position));

        BuildingId? buildingId = null;
        BuildingType? buildingType = null;

        // Find building at this location
        if (plot != null)
        {
            var building = plot.Buildings.FirstOrDefault(b =>
                b.State == ObjectLifecycleState.Operational &&
                b.Footprint.Contains(position));

            if (building != null)
            {
                buildingId = building.Id;
                buildingType = building.BuildingType;
            }
        }

        return new MapCell(
            position,
            terrainCell.TerrainType,
            buildingId,
            buildingType,
            plot?.Id,
            plot?.LandUse,
            terrainCell.HasRoad,
            terrainCell.HasRail,
            terrainCell.IsWater,
            terrainCell.Elevation);
    }

    /// <summary>
    /// Find all buildings on the map.
    /// </summary>
    public IReadOnlyList<(BuildingId Id, BuildingType Type, GridPosition Position)> GetAllBuildings()
    {
        var buildings = new List<(BuildingId, BuildingType, GridPosition)>();
        var allPlots = world.Regions
            .SelectMany(r => r.Cities)
            .SelectMany(c => c.Districts)
            .SelectMany(d => d.Plots)
            .ToList();

        foreach (var plot in allPlots)
        {
            foreach (var building in plot.Buildings)
            {
                if (building.State == ObjectLifecycleState.Operational)
                {
                    buildings.Add((building.Id, building.BuildingType, building.Location));
                }
            }
        }

        return buildings;
    }

    /// <summary>
    /// Find all plots on the map.
    /// </summary>
    public IReadOnlyList<(PlotId Id, LandUseType LandUse, GridPosition Location)> GetAllPlots()
    {
        var plots = world.Regions
            .SelectMany(r => r.Cities)
            .SelectMany(c => c.Districts)
            .SelectMany(d => d.Plots)
            .Select(p => (p.Id, p.LandUse, p.Location))
            .ToList();

        return plots;
    }

    /// <summary>
    /// Get all cells containing roads.
    /// </summary>
    public IReadOnlyList<GridPosition> GetRoads()
    {
        return world.Cells
            .Where(c => c.HasRoad)
            .Select(c => c.Position)
            .ToList();
    }

    /// <summary>
    /// Get all cells containing water.
    /// </summary>
    public IReadOnlyList<GridPosition> GetWaterCells()
    {
        return world.Cells
            .Where(c => c.IsWater)
            .Select(c => c.Position)
            .ToList();
    }
}
