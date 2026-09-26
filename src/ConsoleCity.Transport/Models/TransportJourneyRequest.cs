using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportJourneyRequest
{
    public EntityId Id { get; }

    public TransportTravelDemand Demand { get; }

    public SimulationTime RequestedAt { get; }

    public TransportJourneyRequest(EntityId id, TransportTravelDemand demand, SimulationTime requestedAt)
    {
        ArgumentNullException.ThrowIfNull(demand);
        Id = id;
        Demand = demand;
        RequestedAt = requestedAt;
    }
}
