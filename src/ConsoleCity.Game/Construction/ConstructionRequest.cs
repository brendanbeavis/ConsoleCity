using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Game.Construction;

/// <summary>
/// Represents a player's request to construct a building.
/// Captures the intent, location, and validation state.
/// </summary>
public sealed record class ConstructionRequest
{
    /// <summary>
    /// Unique identifier for this request.
    /// </summary>
    public ConstructionId Id { get; }

    /// <summary>
    /// Type of building being requested.
    /// </summary>
    public BuildingType BuildingType { get; }

    /// <summary>
    /// Desired location for the building.
    /// </summary>
    public GridPosition Location { get; }

    /// <summary>
    /// Time when the request was submitted.
    /// </summary>
    public SimulationTime RequestedAt { get; }

    /// <summary>
    /// Validation errors (empty if valid).
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; }

    /// <summary>
    /// True if this request has been validated and has no errors.
    /// </summary>
    public bool IsValid => ValidationErrors.Count == 0;

    public ConstructionRequest(
        BuildingType buildingType,
        GridPosition location,
        SimulationTime requestedAt)
    {
        Id = ConstructionId.New();
        BuildingType = buildingType;
        Location = location;
        RequestedAt = requestedAt;
        ValidationErrors = new List<string>();
    }

    public ConstructionRequest(
        ConstructionId id,
        BuildingType buildingType,
        GridPosition location,
        SimulationTime requestedAt,
        IReadOnlyList<string> validationErrors)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Construction ID cannot be empty.", nameof(id));
        }

        Id = id;
        BuildingType = buildingType;
        Location = location;
        RequestedAt = requestedAt;
        ValidationErrors = validationErrors ?? new List<string>();
    }

    /// <summary>
    /// Add a validation error to this request.
    /// </summary>
    public ConstructionRequest WithValidationError(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("Validation error cannot be empty.", nameof(error));
        }

        var errors = new List<string>(ValidationErrors) { error };
        return new ConstructionRequest(Id, BuildingType, Location, RequestedAt, errors);
    }

    /// <summary>
    /// Add multiple validation errors.
    /// </summary>
    public ConstructionRequest WithValidationErrors(params string[] errors)
    {
        var newErrors = new List<string>(ValidationErrors);
        foreach (var error in errors)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                newErrors.Add(error);
            }
        }
        return new ConstructionRequest(Id, BuildingType, Location, RequestedAt, newErrors);
    }

    /// <summary>
    /// Clear all validation errors (mark as valid).
    /// </summary>
    public ConstructionRequest ClearValidationErrors()
    {
        return new ConstructionRequest(Id, BuildingType, Location, RequestedAt, new List<string>());
    }
}

