using ConsoleCity.Core;

namespace ConsoleCity.World;

public sealed record class CityModel
{
    public CityId Id { get; }

    public RegionId RegionId { get; }

    public string Name { get; }

    public SimulationTime CreatedAt { get; }

    public GridPosition Location { get; }

    public EntityId? OwnerId { get; }

    public IReadOnlyList<GridPosition> Cells { get; }

    public IReadOnlyList<DistrictModel> Districts { get; }

    public CityModel(
        CityId id,
        RegionId regionId,
        string name,
        SimulationTime createdAt,
        GridPosition location,
        EntityId? ownerId,
        IReadOnlyList<GridPosition> cells,
        IReadOnlyList<DistrictModel> districts)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("City name cannot be empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(cells);
        ArgumentNullException.ThrowIfNull(districts);

        if (cells.Count == 0)
        {
            throw new ArgumentException("A city must contain at least one cell.", nameof(cells));
        }

        if (districts.Any(district => district.CityId != id))
        {
            throw new ArgumentException("Each district must belong to the city it is contained in.", nameof(districts));
        }

        Id = id;
        RegionId = regionId;
        Name = name.Trim();
        CreatedAt = createdAt;
        Location = location;
        OwnerId = ownerId;
        Cells = cells;
        Districts = districts;
    }
}
