using ConsoleCity.Core;

namespace ConsoleCity.Services;

public sealed class SimpleServiceModel : IServiceModel
{
    private readonly List<ServiceProvider> providers = new();
    private readonly List<ServiceRequest> requests = new();
    private readonly List<ServiceResponse> responses = new();
    private readonly Dictionary<ServiceType, int> remainingCapacity = new();

    public ServicesSnapshot Snapshot { get; private set; }

    public SimpleServiceModel(ServicesSnapshot? snapshot = null)
    {
        Snapshot = snapshot ?? ServicesSnapshot.Empty;
        providers.AddRange(Snapshot.Providers);
        requests.AddRange(Snapshot.Requests);
        responses.AddRange(Snapshot.Responses);
    }

    public void RegisterProvider(ServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        providers.Add(provider);
        remainingCapacity[provider.ServiceType] = remainingCapacity.GetValueOrDefault(provider.ServiceType) + provider.Capacity;
        SyncSnapshot(Snapshot.CapturedAt);
    }

    public ServiceResponse SubmitRequest(ServiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        requests.Add(request);
        var response = ResolveRequest(request, Snapshot.CapturedAt);
        responses.Add(response);
        SyncSnapshot(Snapshot.CapturedAt);
        return response;
    }

    public ServicesSnapshot Advance(SimulationTime currentTime)
    {
        var newResponses = new List<ServiceResponse>();
        foreach (var request in requests.Where(request => !responses.Any(response => response.RequestId == request.Id)))
        {
            newResponses.Add(ResolveRequest(request, currentTime));
        }

        responses.AddRange(newResponses);
        SyncSnapshot(currentTime);
        return Snapshot;
    }

    public ServiceOutcome Evaluate(ServiceDemand demand, SimulationTime currentTime)
    {
        var request = new ServiceRequest(Guid.NewGuid(), demand, currentTime);
        var response = ResolveRequest(request, currentTime);
        var capacity = ComputeCapacity(demand.ServiceType, demand.Location);
        var coverage = BuildCoverage(demand.ServiceType, demand.Location, currentTime, capacity.Available > 0);
        return new ServiceOutcome(capacity, coverage, response);
    }

    private ServiceResponse ResolveRequest(ServiceRequest request, SimulationTime currentTime)
    {
        var candidateProviders = providers.Where(provider => provider.ServiceType == request.Demand.ServiceType && provider.IsOperational).ToList();
        var coveringProvider = candidateProviders
            .Where(provider => request.Demand.Location.DistanceTo(provider.Location).Value <= (decimal)provider.CoverageRadius)
            .OrderByDescending(provider => provider.Quality)
            .ThenBy(provider => request.Demand.Location.DistanceTo(provider.Location).Value)
            .FirstOrDefault();

        var capacity = ComputeCapacity(request.Demand.ServiceType, request.Demand.Location);
        var coverage = BuildCoverage(request.Demand.ServiceType, request.Demand.Location, currentTime, coveringProvider is not null);

        if (coveringProvider is null)
        {
            return new ServiceResponse(request.Id, request.Demand.ServiceType, false, 0, currentTime, "No available service provider covers the request location.", coverage);
        }

        var remaining = remainingCapacity.GetValueOrDefault(request.Demand.ServiceType);
        var provided = Math.Min(request.Demand.Amount, remaining);
        remainingCapacity[request.Demand.ServiceType] = Math.Max(0, remaining - provided);
        var success = provided >= request.Demand.Amount;
        var message = success ? "Service request satisfied." : "Service request partially satisfied due to capacity constraints.";
        return new ServiceResponse(request.Id, request.Demand.ServiceType, success, provided, currentTime, message, coverage);
    }

    private ServiceCapacity ComputeCapacity(ServiceType serviceType, GridPosition location)
    {
        var available = providers
            .Where(provider => provider.ServiceType == serviceType && provider.IsOperational)
            .Where(provider => location.DistanceTo(provider.Location).Value <= (decimal)provider.CoverageRadius)
            .Sum(provider => provider.Capacity);

        var demand = requests
            .Where(request => request.Demand.ServiceType == serviceType)
            .Where(request => location.DistanceTo(request.Demand.Location).Value <= (decimal)providers.Where(provider => provider.ServiceType == serviceType && provider.IsOperational).Select(provider => provider.CoverageRadius).DefaultIfEmpty(0d).Max())
            .Sum(request => request.Demand.Amount);

        return new ServiceCapacity(available, demand);
    }

    private ServiceCoverage BuildCoverage(ServiceType serviceType, GridPosition location, SimulationTime currentTime, bool covered)
    {
        var candidate = providers
            .Where(provider => provider.ServiceType == serviceType && provider.IsOperational)
            .OrderByDescending(provider => provider.Quality)
            .FirstOrDefault();

        var radius = candidate?.CoverageRadius ?? 0d;
        var responseTime = candidate is null ? 0d : Math.Max(0d, (double)location.DistanceTo(candidate.Location).Value / Math.Max(1d, candidate.CoverageRadius));
        return new ServiceCoverage(serviceType, location, radius, covered, responseTime);
    }

    private void SyncSnapshot(SimulationTime currentTime)
    {
        Snapshot = new ServicesSnapshot(currentTime, providers.ToList(), requests.ToList(), responses.ToList());
    }
}
