using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Game.Construction;

/// <summary>
/// Represents an active or completed building construction project.
/// Tracks lifecycle state, progress, cost, and the building being constructed.
/// </summary>
public sealed record class BuildingConstruction
{
    /// <summary>
    /// Unique identifier for this construction project.
    /// </summary>
    public ConstructionId Id { get; }

    /// <summary>
    /// Current lifecycle state of the construction.
    /// </summary>
    public BuildingConstructionState State { get; }

    /// <summary>
    /// Type of building being constructed.
    /// </summary>
    public BuildingType BuildingType { get; }

    /// <summary>
    /// Location where the building will be placed.
    /// </summary>
    public GridPosition Location { get; }

    /// <summary>
    /// Total cost of the construction project in currency units.
    /// </summary>
    public Money Cost { get; }

    /// <summary>
    /// Total duration of construction in simulation ticks.
    /// </summary>
    public int DurationTicks { get; }

    /// <summary>
    /// Number of ticks elapsed since construction started.
    /// </summary>
    public int TicksElapsed { get; }

    /// <summary>
    /// Time when this construction was initiated.
    /// </summary>
    public SimulationTime RequestedAt { get; }

    /// <summary>
    /// Time when construction was started (state changed to UnderConstruction).
    /// </summary>
    public SimulationTime? StartedAt { get; }

    /// <summary>
    /// Time when construction was completed or failed.
    /// </summary>
    public SimulationTime? CompletedAt { get; }

    /// <summary>
    /// Optional reason for cancellation or failure.
    /// </summary>
    public string? CancellationReason { get; }

    /// <summary>
    /// ID of the building created upon completion (null until completed).
    /// </summary>
    public BuildingId? ResultingBuildingId { get; }

    public BuildingConstruction(
        ConstructionId id,
        BuildingType buildingType,
        GridPosition location,
        Money cost,
        int durationTicks,
        SimulationTime requestedAt)
    {
        if (durationTicks <= 0)
        {
            throw new ArgumentException("Construction duration must be positive.", nameof(durationTicks));
        }

        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Construction ID cannot be empty.", nameof(id));
        }

        Id = id;
        BuildingType = buildingType;
        Location = location;
        Cost = cost;
        DurationTicks = durationTicks;
        TicksElapsed = 0;
        RequestedAt = requestedAt;
        State = BuildingConstructionState.Requested;
        StartedAt = null;
        CompletedAt = null;
        CancellationReason = null;
        ResultingBuildingId = null;
    }

    /// <summary>
    /// Creates a new BuildingConstruction with updated state.
    /// </summary>
    private BuildingConstruction(
        ConstructionId id,
        BuildingConstructionState state,
        BuildingType buildingType,
        GridPosition location,
        Money cost,
        int durationTicks,
        int ticksElapsed,
        SimulationTime requestedAt,
        SimulationTime? startedAt,
        SimulationTime? completedAt,
        string? cancellationReason,
        BuildingId? resultingBuildingId)
    {
        Id = id;
        State = state;
        BuildingType = buildingType;
        Location = location;
        Cost = cost;
        DurationTicks = durationTicks;
        TicksElapsed = ticksElapsed;
        RequestedAt = requestedAt;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        CancellationReason = cancellationReason;
        ResultingBuildingId = resultingBuildingId;
    }

    /// <summary>
    /// Transition to Validated state (after validation passes).
    /// </summary>
    public BuildingConstruction Validate()
    {
        if (State != BuildingConstructionState.Requested)
        {
            throw new InvalidOperationException($"Cannot validate construction in state {State}.");
        }

        return new BuildingConstruction(
            Id, BuildingConstructionState.Validated, BuildingType, Location, Cost, DurationTicks,
            TicksElapsed, RequestedAt, StartedAt, CompletedAt, CancellationReason, ResultingBuildingId);
    }

    /// <summary>
    /// Transition to Funded state (after payment confirmed).
    /// </summary>
    public BuildingConstruction Fund()
    {
        if (State != BuildingConstructionState.Validated)
        {
            throw new InvalidOperationException($"Cannot fund construction in state {State}.");
        }

        return new BuildingConstruction(
            Id, BuildingConstructionState.Funded, BuildingType, Location, Cost, DurationTicks,
            TicksElapsed, RequestedAt, StartedAt, CompletedAt, CancellationReason, ResultingBuildingId);
    }

    /// <summary>
    /// Start construction (transition to UnderConstruction).
    /// </summary>
    public BuildingConstruction StartConstruction(SimulationTime now)
    {
        if (State != BuildingConstructionState.Funded)
        {
            throw new InvalidOperationException($"Cannot start construction in state {State}.");
        }

        return new BuildingConstruction(
            Id, BuildingConstructionState.UnderConstruction, BuildingType, Location, Cost, DurationTicks,
            TicksElapsed, RequestedAt, now, CompletedAt, CancellationReason, ResultingBuildingId);
    }

    /// <summary>
    /// Advance construction by one tick.
    /// </summary>
    public BuildingConstruction AdvanceTick()
    {
        if (State != BuildingConstructionState.UnderConstruction)
        {
            throw new InvalidOperationException($"Cannot advance construction in state {State}.");
        }

        int newTicksElapsed = TicksElapsed + 1;

        // Check if construction is complete
        if (newTicksElapsed >= DurationTicks)
        {
            return new BuildingConstruction(
                Id, BuildingConstructionState.Completed, BuildingType, Location, Cost, DurationTicks,
                newTicksElapsed, RequestedAt, StartedAt, new SimulationTime(StartedAt?.Tick ?? 0 + newTicksElapsed), CancellationReason, ResultingBuildingId);
        }

        return new BuildingConstruction(
            Id, BuildingConstructionState.UnderConstruction, BuildingType, Location, Cost, DurationTicks,
            newTicksElapsed, RequestedAt, StartedAt, CompletedAt, CancellationReason, ResultingBuildingId);
    }

    /// <summary>
    /// Cancel construction (can only be done before completion).
    /// </summary>
    public BuildingConstruction Cancel(string reason)
    {
        if (State == BuildingConstructionState.Completed || State == BuildingConstructionState.Failed || State == BuildingConstructionState.Cancelled)
        {
            throw new InvalidOperationException($"Cannot cancel construction in state {State}.");
        }

        var now = new SimulationTime(RequestedAt.Tick + TicksElapsed);
        return new BuildingConstruction(
            Id, BuildingConstructionState.Cancelled, BuildingType, Location, Cost, DurationTicks,
            TicksElapsed, RequestedAt, StartedAt, now, reason, ResultingBuildingId);
    }

    /// <summary>
    /// Mark construction as failed.
    /// </summary>
    public BuildingConstruction Fail(string reason)
    {
        if (State == BuildingConstructionState.Completed || State == BuildingConstructionState.Failed || State == BuildingConstructionState.Cancelled)
        {
            throw new InvalidOperationException($"Cannot fail construction in state {State}.");
        }

        var now = new SimulationTime(RequestedAt.Tick + TicksElapsed);
        return new BuildingConstruction(
            Id, BuildingConstructionState.Failed, BuildingType, Location, Cost, DurationTicks,
            TicksElapsed, RequestedAt, StartedAt, now, reason, ResultingBuildingId);
    }

    /// <summary>
    /// Set the resulting building ID after completion.
    /// </summary>
    public BuildingConstruction WithResultingBuilding(BuildingId buildingId)
    {
        return new BuildingConstruction(
            Id, State, BuildingType, Location, Cost, DurationTicks,
            TicksElapsed, RequestedAt, StartedAt, CompletedAt, CancellationReason, buildingId);
    }

    /// <summary>
    /// Get progress as a percentage (0-100).
    /// </summary>
    public double GetProgress() => Math.Min(100.0, (TicksElapsed / (double)DurationTicks) * 100.0);
}

