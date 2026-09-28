using ConsoleCity.Core;

namespace ConsoleCity.Services;

/// <summary>
/// Enhanced service model that tracks per-facility state and computes probabilistic outcomes.
/// </summary>
public sealed class EnhancedServiceModel : IServiceModel
{
    private readonly List<ServiceFacilityState> facilities = new();
    private readonly List<ServiceRequest> requests = new();
    private readonly List<ServiceResponse> responses = new();
    private readonly List<ServiceEvent> events = new();
    private readonly IRandomSource random;

    public ServicesSnapshot Snapshot { get; private set; }

    public IReadOnlyList<ServiceFacilityState> Facilities => facilities.AsReadOnly();

    public IReadOnlyList<ServiceEvent> Events => events.AsReadOnly();

    public EnhancedServiceModel(IRandomSource random, ServicesSnapshot? snapshot = null)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
        Snapshot = snapshot ?? ServicesSnapshot.Empty;
    }

    /// <summary>
    /// Register a service facility in the model.
    /// </summary>
    public void RegisterFacility(ServiceFacilityState facility)
    {
        ArgumentNullException.ThrowIfNull(facility);

        if (facilities.Any(f => f.Id == facility.Id))
        {
            throw new InvalidOperationException($"Facility with ID '{facility.Id}' is already registered.");
        }

        facilities.Add(facility);

        events.Add(new ServiceEvent(
            ServiceEventType.FacilityOpened,
            facility.ServiceType,
            new SimulationTick(0), // Will be set by system
            $"Facility '{facility.Id}' opened with capacity {facility.Capacity}",
            null,
            facility.BuildingId,
            EntityId.New(),
            facility.Location
        ));

        SyncSnapshot(new SimulationTime(0));
    }

    public void RegisterProvider(ServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        // Convert ServiceProvider to ServiceFacilityState
        var facility = new ServiceFacilityState(
            id: provider.Id,
            buildingId: provider.FacilityId,
            serviceType: provider.ServiceType,
            category: DetermineCategory(provider.ServiceType),
            location: provider.Location,
            capacity: provider.Capacity,
            staffAssigned: provider.Staff,
            maxStaff: provider.Staff,
            baseQuality: provider.Quality,
            coverageRadius: provider.CoverageRadius,
            isOperational: provider.IsOperational,
            lifecycleState: ObjectLifecycleState.Operational,
            condition: 1.0d,
            fundingLevel: 1.0d,
            currentDemand: 0,
            unservedDemand: 0,
            openedAt: new SimulationTime(0)
        );

        RegisterFacility(facility);
    }

    public ServiceResponse SubmitRequest(ServiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        requests.Add(request);

        var response = ProcessRequest(request, new SimulationTime(0));
        responses.Add(response);
        SyncSnapshot(new SimulationTime(0));
        return response;
    }

    public ServicesSnapshot Advance(SimulationTime currentTime)
    {
        var newResponses = new List<ServiceResponse>();

        // Process unresponded requests
        foreach (var request in requests.Where(r => !responses.Any(resp => resp.RequestId == r.Id)))
        {
            newResponses.Add(ProcessRequest(request, currentTime));
        }

        responses.AddRange(newResponses);

        // Update facility states (degradation, maintenance, etc.)
        for (int i = 0; i < facilities.Count; i++)
        {
            var facility = facilities[i];

            // Degrade condition over time if not maintained
            double conditionDegradation = 0.001d; // Slow degradation
            double newCondition = Math.Max(0d, facility.Condition - conditionDegradation);

            // Reset demand
            facilities[i] = facility with
            {
                CurrentDemand = 0,
                UnservedDemand = 0,
                Condition = newCondition
            };
        }

        SyncSnapshot(currentTime);
        return Snapshot;
    }

    /// <summary>
    /// Get or create statistics for a service type across all facilities.
    /// </summary>
    public ServiceStatistics GetServiceStatistics(ServiceType serviceType)
    {
        var relevantFacilities = facilities.Where(f => f.ServiceType == serviceType).ToList();

        int totalCapacity = relevantFacilities.Sum(f => f.Capacity);
        int totalDemand = relevantFacilities.Sum(f => f.CurrentDemand + f.UnservedDemand);
        int totalFulfilled = relevantFacilities.Sum(f => f.CurrentDemand);
        double avgQuality = relevantFacilities.Count > 0 
            ? relevantFacilities.Average(f => f.ComputeEffectiveQuality())
            : 0d;
        double avgUtilization = relevantFacilities.Count > 0
            ? relevantFacilities.Average(f => f.GetUtilizationRate())
            : 0d;
        int operationalFacilities = relevantFacilities.Count(f => f.IsOperational);

        return new ServiceStatistics(
            serviceType,
            operationalFacilities,
            totalCapacity,
            totalDemand,
            totalFulfilled,
            avgQuality,
            avgUtilization,
            totalDemand > 0 ? (double)totalFulfilled / totalDemand : 1d
        );
    }

    private ServiceResponse ProcessRequest(ServiceRequest request, SimulationTime currentTime)
    {
        var demand = request.Demand;

        // Find facilities that can serve this request
        var candidateFacilities = facilities
            .Where(f => f.ServiceType == demand.ServiceType && f.IsOperational)
            .Where(f => demand.Location.DistanceTo(f.Location).Value <= (decimal)f.CoverageRadius)
            .OrderByDescending(f => f.ComputeEffectiveQuality())
            .ThenBy(f => demand.Location.DistanceTo(f.Location).Value)
            .ToList();

        if (candidateFacilities.Count == 0)
        {
            // No coverage
            var noCoverage = new ServiceCoverage(demand.ServiceType, demand.Location, 0d, false, 0d);
            return new ServiceResponse(
                request.Id,
                demand.ServiceType,
                false,
                0,
                currentTime,
                "No available service provider covers the request location.",
                noCoverage
            );
        }

        // Try to fulfill request at best facility
        var bestFacility = candidateFacilities[0];
        int availableCapacity = bestFacility.Capacity - bestFacility.CurrentDemand;

        int providedAmount = 0;
        double successProbability = bestFacility.ComputeEffectiveQuality();

        if (availableCapacity > 0)
        {
            // Probabilistic fulfillment based on quality
            providedAmount = random.Next(0, availableCapacity + 1);

            // Apply success probability
            if (random.NextDouble() > successProbability)
            {
                providedAmount = Math.Max(0, (int)(providedAmount * 0.5d)); // Reduce if "failed"
            }

            providedAmount = Math.Min(providedAmount, demand.Amount);
        }

        bool success = providedAmount >= demand.Amount;

        // Update facility demand tracking
        var facilityIndex = facilities.IndexOf(bestFacility);
        var updatedFacility = bestFacility with
        {
            CurrentDemand = bestFacility.CurrentDemand + providedAmount,
            UnservedDemand = bestFacility.UnservedDemand + (demand.Amount - providedAmount)
        };
        facilities[facilityIndex] = updatedFacility;

        // Create coverage info
        var responseTimeHours = (double)demand.Location.DistanceTo(bestFacility.Location).Value / Math.Max(1d, bestFacility.CoverageRadius);
        var serviceCoverage = new ServiceCoverage(
            demand.ServiceType,
            demand.Location,
            bestFacility.CoverageRadius,
            true,
            responseTimeHours
        );

        string message = success
            ? $"Service request fulfilled by facility {bestFacility.Id}."
            : $"Service request partially fulfilled. Requested: {demand.Amount}, Provided: {providedAmount}.";

        var response = new ServiceResponse(
            request.Id,
            demand.ServiceType,
            success,
            providedAmount,
            currentTime,
            message,
            serviceCoverage
        );

        // Emit event
        var eventType = success ? ServiceEventType.RequestFulfilled : ServiceEventType.RequestPartiallyFulfilled;
        events.Add(new ServiceEvent(
            eventType,
            demand.ServiceType,
            new SimulationTick(0), // Will be set by system
            message,
            success ? null : 0.5d, // Severity if partial
            bestFacility.BuildingId,
            demand.RequesterId,
            demand.Location
        ));

        return response;
    }

    private ServiceCategory DetermineCategory(ServiceType serviceType) => serviceType switch
    {
        ServiceType.Police => ServiceCategory.PoliceStation,
        ServiceType.Fire => ServiceCategory.FireStation,
        ServiceType.Ambulance => ServiceCategory.Clinic,
        ServiceType.Hospital => ServiceCategory.Hospital,
        ServiceType.PrimaryEducation => ServiceCategory.PrimarySchool,
        ServiceType.SecondaryEducation => ServiceCategory.SecondarySchool,
        ServiceType.University => ServiceCategory.University,
        ServiceType.Government => ServiceCategory.CityHall,
        ServiceType.Civic => ServiceCategory.Library,
        ServiceType.Recreation => ServiceCategory.RecreationCenter,
        ServiceType.Retail => ServiceCategory.Market,
        ServiceType.Parks => ServiceCategory.Park,
        _ => ServiceCategory.Other
    };

    private void SyncSnapshot(SimulationTime currentTime)
    {
        // Convert facilities back to ServiceProviders for snapshot compatibility
        var providers = facilities.Select(f => new ServiceProvider(
            f.Id,
            f.ServiceType,
            f.BuildingId,
            f.Location,
            f.Capacity,
            f.StaffAssigned,
            f.ComputeEffectiveQuality(),
            f.CoverageRadius,
            f.IsOperational
        )).ToList();

        Snapshot = new ServicesSnapshot(currentTime, providers, requests.ToList(), responses.ToList());
    }
}

/// <summary>
/// Statistics about service delivery for a particular service type.
/// </summary>
public sealed record class ServiceStatistics
{
    public ServiceType ServiceType { get; }

    public int OperationalFacilities { get; }

    public int TotalCapacity { get; }

    public int TotalDemand { get; }

    public int TotalFulfilled { get; }

    public double AverageQuality { get; }

    public double AverageUtilization { get; }

    public double FulfillmentRate { get; }

    public ServiceStatistics(
        ServiceType serviceType,
        int operationalFacilities,
        int totalCapacity,
        int totalDemand,
        int totalFulfilled,
        double averageQuality,
        double averageUtilization,
        double fulfillmentRate)
    {
        ServiceType = serviceType;
        OperationalFacilities = operationalFacilities;
        TotalCapacity = totalCapacity;
        TotalDemand = totalDemand;
        TotalFulfilled = totalFulfilled;
        AverageQuality = averageQuality;
        AverageUtilization = averageUtilization;
        FulfillmentRate = fulfillmentRate;
    }
}
