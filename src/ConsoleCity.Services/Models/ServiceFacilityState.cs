using ConsoleCity.Core;

namespace ConsoleCity.Services;

/// <summary>
/// Extended service facility that tracks operational metrics, quality, and state.
/// </summary>
public sealed record class ServiceFacilityState
{
    /// <summary>
    /// Unique identifier for this facility.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// The building housing this service facility.
    /// </summary>
    public BuildingId BuildingId { get; init; }

    /// <summary>
    /// The broad type of service this facility provides.
    /// </summary>
    public ServiceType ServiceType { get; init; }

    /// <summary>
    /// The specific category/specialization of this facility.
    /// </summary>
    public ServiceCategory Category { get; init; }

    /// <summary>
    /// Geographic location of the facility.
    /// </summary>
    public GridPosition Location { get; init; }

    /// <summary>
    /// Maximum throughput/capacity for this facility.
    /// </summary>
    public int Capacity { get; init; }

    /// <summary>
    /// Current number of staff assigned to this facility.
    /// </summary>
    public int StaffAssigned { get; init; }

    /// <summary>
    /// Maximum staff this facility can support.
    /// </summary>
    public int MaxStaff { get; init; }

    /// <summary>
    /// Base quality rating of this facility (0-1).
    /// </summary>
    public double BaseQuality { get; init; }

    /// <summary>
    /// Geographic coverage radius in grid units.
    /// </summary>
    public double CoverageRadius { get; init; }

    /// <summary>
    /// Whether the facility is currently operational.
    /// </summary>
    public bool IsOperational { get; init; }

    /// <summary>
    /// Current lifecycle state of the facility.
    /// </summary>
    public ObjectLifecycleState LifecycleState { get; init; }

    /// <summary>
    /// Current condition of the facility (0-1), affects quality and reliability.
    /// </summary>
    public double Condition { get; init; }

    /// <summary>
    /// Funding level for this facility (affects quality and staffing maintenance).
    /// </summary>
    public double FundingLevel { get; init; }

    /// <summary>
    /// Current current demand being served or queued.
    /// </summary>
    public int CurrentDemand { get; init; }

    /// <summary>
    /// Demand that could not be fulfilled this period.
    /// </summary>
    public int UnservedDemand { get; init; }

    /// <summary>
    /// Time this facility was last opened/started operational.
    /// </summary>
    public SimulationTime OpenedAt { get; init; }

    /// <summary>
    /// Last maintenance time if applicable.
    /// </summary>
    public SimulationTime? LastMaintenance { get; init; }

    public ServiceFacilityState(
        string id,
        BuildingId buildingId,
        ServiceType serviceType,
        ServiceCategory category,
        GridPosition location,
        int capacity,
        int staffAssigned,
        int maxStaff,
        double baseQuality,
        double coverageRadius,
        bool isOperational,
        ObjectLifecycleState lifecycleState,
        double condition,
        double fundingLevel,
        int currentDemand,
        int unservedDemand,
        SimulationTime openedAt,
        SimulationTime? lastMaintenance = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Facility ID cannot be empty.", nameof(id));
        }

        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        if (staffAssigned < 0 || maxStaff < 0 || staffAssigned > maxStaff)
        {
            throw new ArgumentOutOfRangeException(nameof(staffAssigned));
        }

        if (!double.IsFinite(baseQuality) || baseQuality is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(baseQuality));
        }

        if (!double.IsFinite(coverageRadius) || coverageRadius < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(coverageRadius));
        }

        if (!double.IsFinite(condition) || condition is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(condition));
        }

        if (!double.IsFinite(fundingLevel) || fundingLevel is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(fundingLevel));
        }

        if (currentDemand < 0 || unservedDemand < 0)
        {
            throw new ArgumentOutOfRangeException("Demand values must be non-negative.");
        }

        Id = id;
        BuildingId = buildingId;
        ServiceType = serviceType;
        Category = category;
        Location = location;
        Capacity = capacity;
        StaffAssigned = staffAssigned;
        MaxStaff = maxStaff;
        BaseQuality = baseQuality;
        CoverageRadius = coverageRadius;
        IsOperational = isOperational;
        LifecycleState = lifecycleState;
        Condition = condition;
        FundingLevel = fundingLevel;
        CurrentDemand = currentDemand;
        UnservedDemand = unservedDemand;
        OpenedAt = openedAt;
        LastMaintenance = lastMaintenance;
    }

    /// <summary>
    /// Calculate the effective quality of this facility based on multiple factors.
    /// </summary>
    public double ComputeEffectiveQuality()
    {
        if (!IsOperational || LifecycleState != ObjectLifecycleState.Operational)
        {
            return 0d;
        }

        // Start with base quality
        double quality = BaseQuality;

        // Reduce by condition degradation
        quality *= Condition;

        // Reduce by staffing adequacy
        double staffingRatio = MaxStaff > 0 ? (double)StaffAssigned / MaxStaff : 0d;
        quality *= (0.5d + (staffingRatio * 0.5d)); // 50% reduction if understaffed, 0% if properly staffed

        // Reduce by funding level
        quality *= (0.7d + (FundingLevel * 0.3d)); // 70% minimum if unfunded, 100% if fully funded

        // Reduce by overload
        double utilization = Capacity > 0 ? (double)CurrentDemand / Capacity : 0d;
        double overloadPenalty = Math.Max(0, utilization - 1.0d); // Penalty only if over capacity
        quality *= Math.Max(0.3d, 1d - overloadPenalty); // Min 30% quality even when overloaded

        return Math.Clamp(quality, 0d, 1d);
    }

    /// <summary>
    /// Get the utilization rate of this facility (0-1, where >1 means overcapacity).
    /// </summary>
    public double GetUtilizationRate() => Capacity > 0 ? (double)CurrentDemand / Capacity : 0d;

    /// <summary>
    /// Get the staffing adequacy ratio (0-1, where 1 is fully staffed).
    /// </summary>
    public double GetStaffingRatio() => MaxStaff > 0 ? (double)StaffAssigned / MaxStaff : 0d;

    /// <summary>
    /// Get the fulfilled vs total demand.
    /// </summary>
    public double GetFulfillmentRate()
    {
        int totalDemand = CurrentDemand + UnservedDemand;
        return totalDemand > 0 ? (double)CurrentDemand / totalDemand : 1d;
    }
}
