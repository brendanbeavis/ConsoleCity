using ConsoleCity.Core;

namespace ConsoleCity.World;

/// <summary>
/// Resolves map cells back to simulation entities.
/// Allows selecting and inspecting objects on the map.
/// </summary>
public sealed class MapInspector
{
    private readonly MapView mapView;

    public MapInspector(MapView mapView)
    {
        ArgumentNullException.ThrowIfNull(mapView);
        this.mapView = mapView;
    }

    /// <summary>
    /// Get the object at a specific position (building or plot).
    /// </summary>
    public MapObject? GetObjectAt(GridPosition position)
    {
        var cell = mapView.GetCell(position);

        // Building has priority
        if (cell.BuildingId.HasValue)
        {
            return new MapObject(
                ObjectType.Building,
                cell.BuildingId.Value.Value.ToString(),
                cell.BuildingType?.ToString() ?? "Unknown",
                position);
        }

        // Then plot
        if (cell.PlotId.HasValue)
        {
            return new MapObject(
                ObjectType.Plot,
                cell.PlotId.Value.Value.ToString(),
                cell.LandUse?.ToString() ?? "Unknown",
                position);
        }

        // Road
        if (cell.HasRoad)
        {
            return new MapObject(
                ObjectType.Road,
                $"Road#{position.X}_{position.Y}",
                "Road",
                position);
        }

        return null;
    }

    /// <summary>
    /// Find all objects of a specific type on the map.
    /// </summary>
    public IReadOnlyList<MapObject> FindObjectsByType(ObjectType type)
    {
        var objects = new List<MapObject>();

        return type switch
        {
            ObjectType.Building => GetAllBuildings(),
            ObjectType.Plot => GetAllPlots(),
            ObjectType.Road => GetAllRoads(),
            _ => objects
        };
    }

    /// <summary>
    /// Find all buildings on the map.
    /// </summary>
    public IReadOnlyList<MapObject> GetAllBuildings()
    {
        var objects = new List<MapObject>();
        var buildings = mapView.GetAllBuildings();

        foreach (var (id, type, pos) in buildings)
        {
            objects.Add(new MapObject(
                ObjectType.Building,
                id.Value.ToString(),
                type.ToString(),
                pos));
        }

        return objects;
    }

    /// <summary>
    /// Find all plots on the map.
    /// </summary>
    public IReadOnlyList<MapObject> GetAllPlots()
    {
        var objects = new List<MapObject>();
        var plots = mapView.GetAllPlots();

        foreach (var (id, use, pos) in plots)
        {
            objects.Add(new MapObject(
                ObjectType.Plot,
                id.Value.ToString(),
                use.ToString(),
                pos));
        }

        return objects;
    }

    /// <summary>
    /// Find all roads on the map.
    /// </summary>
    public IReadOnlyList<MapObject> GetAllRoads()
    {
        var objects = new List<MapObject>();
        var roads = mapView.GetRoads();

        int i = 0;
        foreach (var pos in roads)
        {
            objects.Add(new MapObject(
                ObjectType.Road,
                $"Road#{i++}",
                "Road",
                pos));
        }

        return objects;
    }

    /// <summary>
    /// Get all objects within a rectangular region.
    /// </summary>
    public IReadOnlyList<MapObject> GetObjectsInRegion(GridPosition topLeft, GridPosition bottomRight)
    {
        var objects = new List<MapObject>();
        var buildings = mapView.GetAllBuildings();
        var plots = mapView.GetAllPlots();

        foreach (var (id, type, pos) in buildings)
        {
            if (IsInRegion(pos, topLeft, bottomRight))
            {
                objects.Add(new MapObject(
                    ObjectType.Building,
                    id.Value.ToString(),
                    type.ToString(),
                    pos));
            }
        }

        foreach (var (id, use, pos) in plots)
        {
            if (IsInRegion(pos, topLeft, bottomRight))
            {
                objects.Add(new MapObject(
                    ObjectType.Plot,
                    id.Value.ToString(),
                    use.ToString(),
                    pos));
            }
        }

        return objects;
    }

    /// <summary>
    /// Find an object by ID.
    /// </summary>
    public MapObject? FindObjectById(string objectId)
    {
        var buildings = mapView.GetAllBuildings();
        var (buildingId, buildingType, buildingPos) = buildings.FirstOrDefault(b => b.Id.Value.ToString() == objectId);
        if (buildingId != null)
        {
            return new MapObject(ObjectType.Building, objectId, buildingType.ToString(), buildingPos);
        }

        var plots = mapView.GetAllPlots();
        var (plotId, plotUse, plotPos) = plots.FirstOrDefault(p => p.Id.Value.ToString() == objectId);
        if (plotId != null)
        {
            return new MapObject(ObjectType.Plot, objectId, plotUse.ToString(), plotPos);
        }

        return null;
    }

    private bool IsInRegion(GridPosition pos, GridPosition topLeft, GridPosition bottomRight)
    {
        return pos.X >= topLeft.X && pos.X <= bottomRight.X &&
               pos.Y >= topLeft.Y && pos.Y <= bottomRight.Y;
    }
}

/// <summary>
/// Represents a selectable/inspectable object on the map.
/// </summary>
public sealed record class MapObject
{
    /// <summary>
    /// The type of object.
    /// </summary>
    public ObjectType Type { get; }

    /// <summary>
    /// The ID of the object (e.g., BuildingId or PlotId).
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Human-readable type/category (e.g., "House", "Residential", "Road").
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// The position of the object on the map.
    /// </summary>
    public GridPosition Position { get; }

    public MapObject(ObjectType type, string id, string category, GridPosition position)
    {
        Type = type;
        Id = id;
        Category = category;
        Position = position;
    }

    public override string ToString() => $"{Type} '{Category}' ({Id}) @ {Position}";
}

/// <summary>
/// The type of object on the map.
/// </summary>
public enum ObjectType
{
    Building,
    Plot,
    Road,
    Water,
    Terrain
}
