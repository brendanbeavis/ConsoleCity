using ConsoleCity.Core;

namespace ConsoleCity.Services;

/// <summary>
/// Generates service demand based on simulation state and population conditions.
/// </summary>
public interface IServiceDemandGenerator
{
    /// <summary>
    /// Generate service demands for a given location and current simulation time.
    /// </summary>
    IReadOnlyList<ServiceDemand> GenerateDemands(GridPosition location, SimulationTime currentTime, IServiceDemandContext context);
}

/// <summary>
/// Context information available to demand generators.
/// </summary>
public interface IServiceDemandContext
{
    /// <summary>
    /// Population at a given location.
    /// </summary>
    int GetPopulationAt(GridPosition location);

    /// <summary>
    /// Number of school-age children at a location.
    /// </summary>
    int GetStudentsAt(GridPosition location);

    /// <summary>
    /// Crime rate or crime incidents at a location (0-1 scale).
    /// </summary>
    double GetCrimeRateAt(GridPosition location);

    /// <summary>
    /// Fire risk or active fires at a location.
    /// </summary>
    double GetFireRiskAt(GridPosition location);

    /// <summary>
    /// Health status or disease presence at a location (0-1 scale where 1 is very ill population).
    /// </summary>
    double GetHealthStatusAt(GridPosition location);

    /// <summary>
    /// Household income level at a location (for retail demand).
    /// </summary>
    double GetAverageIncomeAt(GridPosition location);

    /// <summary>
    /// Whether there are fire incidents active at a location.
    /// </summary>
    bool HasActiveFireAt(GridPosition location);

    /// <summary>
    /// Whether there are crime incidents at a location.
    /// </summary>
    bool HasCrimeAt(GridPosition location);
}
