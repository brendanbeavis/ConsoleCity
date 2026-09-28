using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportSnapshot
{
    public SimulationTime CapturedAt { get; }

    public TransportNetwork Network { get; }

    public TransportTrafficState Traffic { get; }

    public IReadOnlyList<TransportVehicle> Vehicles { get; }

    public IReadOnlyList<TransportJourney> Journeys { get; }

    public TransportSnapshot(
        SimulationTime capturedAt,
        TransportNetwork network,
        TransportTrafficState traffic,
        IReadOnlyList<TransportVehicle> vehicles,
        IReadOnlyList<TransportJourney> journeys)
    {
        ArgumentNullException.ThrowIfNull(network);
        ArgumentNullException.ThrowIfNull(traffic);
        ArgumentNullException.ThrowIfNull(vehicles);
        ArgumentNullException.ThrowIfNull(journeys);
        CapturedAt = capturedAt;
        Network = network;
        Traffic = traffic;
        Vehicles = vehicles;
        Journeys = journeys;
    }

    public static TransportSnapshot Empty { get; } = new(new SimulationTime(0), TransportNetwork.Empty, TransportTrafficState.Empty, Array.Empty<TransportVehicle>(), Array.Empty<TransportJourney>());
}
