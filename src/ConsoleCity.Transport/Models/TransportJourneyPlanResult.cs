namespace ConsoleCity.Transport;

public sealed record TransportJourneyPlanResult(
    bool Success,
    TransportJourney? Journey,
    TransportRoute? Route,
    string Message,
    IReadOnlyList<TransportEvent> Events);
