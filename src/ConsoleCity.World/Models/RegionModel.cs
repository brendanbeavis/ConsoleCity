using ConsoleCity.Core;

namespace ConsoleCity.World;

public sealed record class RegionModel
{
    public RegionId Id { get; }

    public WorldId WorldId { get; }

    public string Name { get; }

    public SimulationTime CreatedAt { get; }

    public GridPosition Location { get; }

    public EntityId? OwnerId { get; }

    public IReadOnlyList<GridPosition> Cells { get; }

    public IReadOnlyList<CityModel> Cities { get; }

    public RegionModel(
        RegionId id,
        WorldId worldId,
        string name,
        SimulationTime createdAt,
        GridPosition location,
        EntityId? ownerId,
        IReadOnlyList<GridPosition> cells,
        IReadOnlyList<CityModel> cities)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Region name cannot be empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(cells);
        ArgumentNullException.ThrowIfNull(cities);

        if (cells.Count == 0)
        {
            throw new ArgumentException("A region must contain at least one cell.", nameof(cells));
        }

        if (cities.Any(city => city.RegionId != id))
        {
            throw new ArgumentException("Each city must belong to the region it is contained in.", nameof(cities));
        }

        Id = id;
        WorldId = worldId;
        Name = name.Trim();
        CreatedAt = createdAt;
        Location = location;
        OwnerId = ownerId;
        Cells = cells;
        Cities = cities;
    }
}
