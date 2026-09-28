namespace ConsoleCity.Transport;

public sealed record TransportAdvanceResult(
    TransportSnapshot Snapshot,
    IReadOnlyList<TransportJourney> CompletedJourneys,
    IReadOnlyList<TransportEvent> Events);
