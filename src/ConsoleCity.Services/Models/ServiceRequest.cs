using ConsoleCity.Core;

namespace ConsoleCity.Services;

public sealed record class ServiceRequest
{
    public Guid Id { get; }

    public ServiceDemand Demand { get; }

    public SimulationTime RequestedAt { get; }

    public ServiceRequest(Guid id, ServiceDemand demand, SimulationTime requestedAt)
    {
        ArgumentNullException.ThrowIfNull(demand);
        Id = id;
        Demand = demand;
        RequestedAt = requestedAt;
    }
}
