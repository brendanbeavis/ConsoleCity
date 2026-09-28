using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Game.Construction;

/// <summary>
/// Manages active construction projects, tracking state transitions and tick progression.
/// </summary>
public sealed class ConstructionManager
{
    private readonly Dictionary<ConstructionId, BuildingConstruction> activeProjects = new();

    /// <summary>
    /// Get all active construction projects.
    /// </summary>
    public IReadOnlyDictionary<ConstructionId, BuildingConstruction> ActiveProjects => activeProjects.AsReadOnly();

    /// <summary>
    /// Register a new construction project.
    /// </summary>
    public void RegisterConstruction(BuildingConstruction construction)
    {
        ArgumentNullException.ThrowIfNull(construction);

        if (activeProjects.ContainsKey(construction.Id))
        {
            throw new InvalidOperationException($"Construction {construction.Id} is already registered.");
        }

        activeProjects[construction.Id] = construction;
    }

    /// <summary>
    /// Get a specific construction project by ID.
    /// </summary>
    public BuildingConstruction? GetConstruction(ConstructionId id)
    {
        activeProjects.TryGetValue(id, out var construction);
        return construction;
    }

    /// <summary>
    /// Get all construction projects in a specific state.
    /// </summary>
    public IReadOnlyList<BuildingConstruction> GetConstructionsInState(BuildingConstructionState state)
    {
        return activeProjects.Values
            .Where(c => c.State == state)
            .ToList();
    }

    /// <summary>
    /// Advance a construction project one tick (if it's under construction).
    /// </summary>
    public void AdvanceConstructionTick(ConstructionId id)
    {
        if (!activeProjects.TryGetValue(id, out var construction))
        {
            throw new KeyNotFoundException($"Construction {id} not found.");
        }

        if (construction.State != BuildingConstructionState.UnderConstruction)
        {
            return; // No advancement needed if not under construction
        }

        var advancedConstruction = construction.AdvanceTick();
        activeProjects[id] = advancedConstruction;
    }

    /// <summary>
    /// Update a construction project's state.
    /// </summary>
    public void UpdateConstruction(BuildingConstruction construction)
    {
        ArgumentNullException.ThrowIfNull(construction);

        if (!activeProjects.ContainsKey(construction.Id))
        {
            throw new KeyNotFoundException($"Construction {construction.Id} not found.");
        }

        activeProjects[construction.Id] = construction;
    }

    /// <summary>
    /// Cancel a construction project.
    /// </summary>
    public void CancelConstruction(ConstructionId id, string reason)
    {
        if (!activeProjects.TryGetValue(id, out var construction))
        {
            throw new KeyNotFoundException($"Construction {id} not found.");
        }

        var cancelled = construction.Cancel(reason);
        activeProjects[id] = cancelled;
    }

    /// <summary>
    /// Mark a construction project as failed.
    /// </summary>
    public void FailConstruction(ConstructionId id, string reason)
    {
        if (!activeProjects.TryGetValue(id, out var construction))
        {
            throw new KeyNotFoundException($"Construction {id} not found.");
        }

        var failed = construction.Fail(reason);
        activeProjects[id] = failed;
    }

    /// <summary>
    /// Advance all active construction projects by one tick.
    /// Returns list of IDs for completed constructions.
    /// </summary>
    public IReadOnlyList<ConstructionId> AdvanceAllTicks()
    {
        var completedIds = new List<ConstructionId>();

        var underConstructionIds = activeProjects
            .Where(kvp => kvp.Value.State == BuildingConstructionState.UnderConstruction)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in underConstructionIds)
        {
            AdvanceConstructionTick(id);

            // Check if now completed
            if (activeProjects[id].State == BuildingConstructionState.Completed)
            {
                completedIds.Add(id);
            }
        }

        return completedIds;
    }

    /// <summary>
    /// Get the total count of active construction projects.
    /// </summary>
    public int GetActiveCount()
    {
        return activeProjects.Count(kvp =>
            kvp.Value.State == BuildingConstructionState.UnderConstruction ||
            kvp.Value.State == BuildingConstructionState.Requested ||
            kvp.Value.State == BuildingConstructionState.Validated ||
            kvp.Value.State == BuildingConstructionState.Funded);
    }

    /// <summary>
    /// Get all completed constructions.
    /// </summary>
    public IReadOnlyList<BuildingConstruction> GetCompletedConstructions()
    {
        return activeProjects.Values
            .Where(c => c.State == BuildingConstructionState.Completed)
            .ToList();
    }

    /// <summary>
    /// Get all failed or cancelled constructions.
    /// </summary>
    public IReadOnlyList<BuildingConstruction> GetTerminatedConstructions()
    {
        return activeProjects.Values
            .Where(c => c.State == BuildingConstructionState.Failed || c.State == BuildingConstructionState.Cancelled)
            .ToList();
    }
}
