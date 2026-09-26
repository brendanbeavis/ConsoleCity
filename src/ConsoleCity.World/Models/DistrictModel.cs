using ConsoleCity.Core;

namespace ConsoleCity.World;

public sealed record class DistrictModel
{
    public DistrictId Id { get; }

    public CityId CityId { get; }

    public string Name { get; }

    public DistrictType DistrictType { get; }

    public SimulationTime CreatedAt { get; }

    public GridPosition Location { get; }

    public EntityId? OwnerId { get; }

    public IReadOnlyList<GridPosition> Cells { get; }

    public IReadOnlyList<PlotModel> Plots { get; }

    public DistrictModel(
        DistrictId id,
        CityId cityId,
        string name,
        DistrictType districtType,
        SimulationTime createdAt,
        GridPosition location,
        EntityId? ownerId,
        IReadOnlyList<GridPosition> cells,
        IReadOnlyList<PlotModel> plots)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("District name cannot be empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(cells);
        ArgumentNullException.ThrowIfNull(plots);

        if (cells.Count == 0)
        {
            throw new ArgumentException("A district must contain at least one cell.", nameof(cells));
        }

        if (plots.Any(plot => plot.DistrictId != id))
        {
            throw new ArgumentException("Each plot must belong to the district it is contained in.", nameof(plots));
        }

        Id = id;
        CityId = cityId;
        Name = name.Trim();
        DistrictType = districtType;
        CreatedAt = createdAt;
        Location = location;
        OwnerId = ownerId;
        Cells = cells;
        Plots = plots;
    }
}
