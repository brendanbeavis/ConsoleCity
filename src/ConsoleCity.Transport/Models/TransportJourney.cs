using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportJourney
{
    public EntityId Id { get; }

    public TransportJourneyRequest Request { get; }

    public TransportRoute Route { get; }

    public TransportJourneyStatus Status { get; }

    public SimulationTime RequestedAt { get; }

    public SimulationTime? DepartedAt { get; }

    public SimulationTime? ArrivedAt { get; }

    public TimeSpan ElapsedTravelTime { get; }

    public EntityId? VehicleId { get; }

    public TransportJourney(
        EntityId id,
        TransportJourneyRequest request,
        TransportRoute route,
        TransportJourneyStatus status,
        SimulationTime requestedAt,
        SimulationTime? departedAt = null,
        SimulationTime? arrivedAt = null,
        TimeSpan? elapsedTravelTime = null,
        EntityId? vehicleId = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(route);
        Id = id;
        Request = request;
        Route = route;
        Status = status;
        RequestedAt = requestedAt;
        DepartedAt = departedAt;
        ArrivedAt = arrivedAt;
        ElapsedTravelTime = elapsedTravelTime ?? TimeSpan.Zero;
        VehicleId = vehicleId;
    }

    public TransportJourney Start(SimulationTime currentTime, EntityId? vehicleId = null)
        => new(Id, Request, Route, TransportJourneyStatus.InTransit, RequestedAt, currentTime, null, TimeSpan.Zero, vehicleId);

    public TransportJourney Advance(TimeSpan elapsed)
    {
        if (Status != TransportJourneyStatus.InTransit)
        {
            return this;
        }

        var totalElapsed = ElapsedTravelTime + elapsed;
        if (totalElapsed >= Route.EstimatedTravelTime)
        {
            return new TransportJourney(Id, Request, Route, TransportJourneyStatus.Completed, RequestedAt, DepartedAt, ArrivedAt ?? RequestedAt, Route.EstimatedTravelTime, VehicleId);
        }

        return new TransportJourney(Id, Request, Route, Status, RequestedAt, DepartedAt, ArrivedAt, totalElapsed, VehicleId);
    }

    public TransportJourney Cancel()
        => new(Id, Request, Route, TransportJourneyStatus.Cancelled, RequestedAt, DepartedAt, ArrivedAt, ElapsedTravelTime, VehicleId);
}
