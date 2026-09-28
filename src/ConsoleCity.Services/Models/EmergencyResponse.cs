using ConsoleCity.Core;

namespace ConsoleCity.Services;

/// <summary>
/// Represents an emergency response unit (fire truck, ambulance, police car, etc.)
/// </summary>
public sealed record class EmergencyResponseUnit
{
    public string Id { get; }

    public ServiceType ServiceType { get; }

    public string FacilityId { get; }

    public GridPosition CurrentLocation { get; }

    public GridPosition? TargetLocation { get; }

    public EmergencyResponseState State { get; }

    public SimulationTime? DispatchedAt { get; }

    public SimulationTime? ArrivedAt { get; }

    public int RemainingCapacity { get; }

    public EmergencyResponseUnit(
        string id,
        ServiceType serviceType,
        string facilityId,
        GridPosition currentLocation,
        GridPosition? targetLocation,
        EmergencyResponseState state,
        SimulationTime? dispatchedAt,
        SimulationTime? arrivedAt,
        int remainingCapacity)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Unit ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(facilityId))
        {
            throw new ArgumentException("Facility ID cannot be empty.", nameof(facilityId));
        }

        if (remainingCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(remainingCapacity));
        }

        Id = id;
        ServiceType = serviceType;
        FacilityId = facilityId;
        CurrentLocation = currentLocation;
        TargetLocation = targetLocation;
        State = state;
        DispatchedAt = dispatchedAt;
        ArrivedAt = arrivedAt;
        RemainingCapacity = remainingCapacity;
    }
}

/// <summary>
/// States for emergency response units.
/// </summary>
public enum EmergencyResponseState
{
    /// <summary>Unit is available at facility.</summary>
    Available,

    /// <summary>Unit is en route to incident.</summary>
    EnRoute,

    /// <summary>Unit has arrived and is working on incident.</summary>
    OnScene,

    /// <summary>Unit is returning to facility.</summary>
    Returning,

    /// <summary>Unit is out of service for maintenance.</summary>
    OutOfService
}

/// <summary>
/// Represents a pending emergency response request.
/// </summary>
public sealed record class EmergencyResponse
{
    public string Id { get; }

    public ServiceType ServiceType { get; }

    public GridPosition IncidentLocation { get; }

    public EmergencyResponseUnit? AssignedUnit { get; }

    public EmergencyResponseStatus Status { get; }

    public double Urgency { get; }

    public SimulationTime RequestedAt { get; }

    public SimulationTime? CompletedAt { get; }

    public int EstimatedResponseTimeMinutes { get; }

    public string? Description { get; }

    public EmergencyResponse(
        string id,
        ServiceType serviceType,
        GridPosition incidentLocation,
        double urgency,
        SimulationTime requestedAt,
        EmergencyResponseUnit? assignedUnit = null,
        EmergencyResponseStatus status = EmergencyResponseStatus.Pending,
        SimulationTime? completedAt = null,
        int estimatedResponseTimeMinutes = 0,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Response ID cannot be empty.", nameof(id));
        }

        if (!double.IsFinite(urgency) || urgency is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(urgency));
        }

        if (estimatedResponseTimeMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedResponseTimeMinutes));
        }

        Id = id;
        ServiceType = serviceType;
        IncidentLocation = incidentLocation;
        AssignedUnit = assignedUnit;
        Status = status;
        Urgency = urgency;
        RequestedAt = requestedAt;
        CompletedAt = completedAt;
        EstimatedResponseTimeMinutes = estimatedResponseTimeMinutes;
        Description = description;
    }
}

/// <summary>
/// Possible states for emergency response.
/// </summary>
public enum EmergencyResponseStatus
{
    /// <summary>Waiting for unit dispatch.</summary>
    Pending,

    /// <summary>Unit has been dispatched.</summary>
    Dispatched,

    /// <summary>Unit is en route.</summary>
    EnRoute,

    /// <summary>Unit arrived at scene.</summary>
    OnScene,

    /// <summary>Response completed.</summary>
    Completed,

    /// <summary>No units available.</summary>
    Canceled
}

/// <summary>
/// Allocates emergency response units to incidents based on location and availability.
/// </summary>
public interface IEmergencyResponseAllocator
{
    /// <summary>
    /// Find the best available emergency response unit for an incident.
    /// </summary>
    EmergencyResponseUnit? FindAvailableUnit(ServiceType serviceType, GridPosition incidentLocation, IReadOnlyList<EmergencyResponseUnit> availableUnits);

    /// <summary>
    /// Calculate estimated response time from unit to incident.
    /// </summary>
    int CalculateEstimatedResponseTime(GridPosition fromLocation, GridPosition toLocation, ServiceType serviceType);

    /// <summary>
    /// Calculate distance between two grid positions.
    /// </summary>
    double CalculateDistance(GridPosition from, GridPosition to);
}

/// <summary>
/// Simple emergency response allocator based on grid distance.
/// </summary>
public sealed class SimpleEmergencyResponseAllocator : IEmergencyResponseAllocator
{
    private const double MinutesPerGridUnit = 5.0d; // ~5 minutes per grid unit traveled

    public EmergencyResponseUnit? FindAvailableUnit(
        ServiceType serviceType,
        GridPosition incidentLocation,
        IReadOnlyList<EmergencyResponseUnit> availableUnits)
    {
        // Find closest available unit of the matching service type
        return availableUnits
            .Where(u => u.ServiceType == serviceType && u.State == EmergencyResponseState.Available)
            .OrderBy(u => CalculateDistance(u.CurrentLocation, incidentLocation))
            .FirstOrDefault();
    }

    public int CalculateEstimatedResponseTime(GridPosition fromLocation, GridPosition toLocation, ServiceType serviceType)
    {
        double distance = CalculateDistance(fromLocation, toLocation);

        // Base time on distance
        int baseTime = (int)Math.Ceiling(distance * MinutesPerGridUnit);

        // Adjust based on service type (some are faster/slower)
        int adjustedTime = serviceType switch
        {
            ServiceType.Fire => (int)(baseTime * 0.9d), // Fire trucks prioritized, slightly faster
            ServiceType.Police => (int)(baseTime * 0.95d), // Police cars are fast
            ServiceType.Ambulance => (int)(baseTime * 1.0d), // Standard speed
            _ => baseTime
        };

        return Math.Max(1, adjustedTime);
    }

    public double CalculateDistance(GridPosition from, GridPosition to)
    {
        // Euclidean distance
        int dx = (int)(to.X - from.X);
        int dy = (int)(to.Y - from.Y);
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
