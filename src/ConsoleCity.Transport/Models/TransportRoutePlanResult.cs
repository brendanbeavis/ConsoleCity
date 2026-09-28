namespace ConsoleCity.Transport;

public sealed record TransportRoutePlanResult(
    bool Success,
    TransportRoute? Route,
    string Message);
