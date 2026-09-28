namespace ConsoleCity.Game.Construction;

/// <summary>
/// Represents the lifecycle state of a construction project.
/// </summary>
public enum BuildingConstructionState
{
    /// <summary>
    /// Initial state: construction requested but not yet validated.
    /// </summary>
    Requested,

    /// <summary>
    /// Construction request has been validated; waiting for funding.
    /// </summary>
    Validated,

    /// <summary>
    /// Funding has been secured; construction is ready to begin.
    /// </summary>
    Funded,

    /// <summary>
    /// Construction is actively underway.
    /// </summary>
    UnderConstruction,

    /// <summary>
    /// Construction has been completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Construction has been cancelled by player or system.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Construction failed due to error or resource shortage.
    /// </summary>
    Failed
}
