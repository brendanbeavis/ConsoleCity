using ConsoleCity.Core;

namespace ConsoleCity.World;

public sealed record class BuildingModel
{
    public BuildingId Id { get; }

    public PlotId PlotId { get; }

    public string Name { get; }

    public BuildingType BuildingType { get; }

    public SimulationTime CreatedAt { get; }

    public GridPosition Location { get; }

    public EntityId? OwnerId { get; }

    public IReadOnlyList<GridPosition> Footprint { get; }

    public BuildingCapacities Capacities { get; }

    public Money ConstructionCost { get; }

    public SimulationTick ConstructionDuration { get; }

    public Money MaintenanceCost { get; }

    public ObjectLifecycleState State { get; }

    public double Condition { get; }

    public BuildingModel(
        BuildingId id,
        PlotId plotId,
        string name,
        BuildingType buildingType,
        SimulationTime createdAt,
        GridPosition location,
        EntityId? ownerId,
        IReadOnlyList<GridPosition> footprint,
        BuildingCapacities capacities,
        Money constructionCost,
        SimulationTick constructionDuration,
        Money maintenanceCost,
        ObjectLifecycleState state,
        double condition)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Building name cannot be empty.", nameof(name));
        }

        if (!double.IsFinite(condition) || condition is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(condition), "Building condition must be a finite fraction between 0 and 1.");
        }

        ArgumentNullException.ThrowIfNull(footprint);

        if (footprint.Count == 0)
        {
            throw new ArgumentException("A building footprint cannot be empty.", nameof(footprint));
        }

        Id = id;
        PlotId = plotId;
        Name = name.Trim();
        BuildingType = buildingType;
        CreatedAt = createdAt;
        Location = location;
        OwnerId = ownerId;
        Footprint = footprint;
        Capacities = capacities;
        ConstructionCost = constructionCost;
        ConstructionDuration = constructionDuration;
        MaintenanceCost = maintenanceCost;
        State = state;
        Condition = condition;
    }
}
