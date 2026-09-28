namespace ConsoleCity.Services;

/// <summary>
/// Enumeration of service-related events that occur during simulation.
/// </summary>
public enum ServiceEventType
{
    // Facility lifecycle
    FacilityOpened,
    FacilityClosed,
    FacilityDamaged,
    FacilityRepaired,

    // Demand and requests
    RequestSubmitted,
    RequestFulfilled,
    RequestPartiallyFulfilled,
    RequestDenied,
    RequestQueued,

    // Capacity events
    CapacityExceeded,
    CapacityLow,
    CapacityRestored,

    // Quality events
    QualityDegraded,
    QualityImproved,
    QualityAlert,

    // Staffing events
    StaffingLow,
    StaffingCritical,
    StaffingImproved,

    // Emergency-specific events
    EmergencyResponseStarted,
    EmergencyResponseArrived,
    EmergencyResponseCompleted,
    EmergencyResponseFailed,

    // Healthcare-specific
    PatientAdmitted,
    PatientTreated,
    PatientDischarged,
    PatientQueueing,

    // Education-specific
    StudentEnrolled,
    StudentGraduated,
    StudentUnderperforming,

    // Other
    ServiceInterruption,
    MaintenanceRequired
}
