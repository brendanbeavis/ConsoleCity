using ConsoleCity.Core;

namespace ConsoleCity.World;

public sealed record class PlotModel
{
    public PlotId Id { get; }

    public DistrictId DistrictId { get; }

    public string Name { get; }

    public LandUseType LandUse { get; }

    public SimulationTime CreatedAt { get; }

    public GridPosition Location { get; }

    public EntityId? OwnerId { get; }

    public IReadOnlyList<GridPosition> Cells { get; }

    public IReadOnlyList<BuildingModel> Buildings { get; }

    public ObjectLifecycleState State { get; }

    public PlotModel(
        PlotId id,
        DistrictId districtId,
        string name,
        LandUseType landUse,
        SimulationTime createdAt,
        GridPosition location,
        EntityId? ownerId,
        IReadOnlyList<GridPosition> cells,
        IReadOnlyList<BuildingModel> buildings,
        ObjectLifecycleState state)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Plot name cannot be empty.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(cells);
        ArgumentNullException.ThrowIfNull(buildings);

        if (cells.Count == 0)
        {
            throw new ArgumentException("A plot must contain at least one cell.", nameof(cells));
        }

        if (buildings.Any(building => building.PlotId != id))
        {
            throw new ArgumentException("Each building must belong to the plot it is contained in.", nameof(buildings));
        }

        Id = id;
        DistrictId = districtId;
        Name = name.Trim();
        LandUse = landUse;
        CreatedAt = createdAt;
        Location = location;
        OwnerId = ownerId;
        Cells = cells;
        Buildings = buildings;
        State = state;
    }
}
