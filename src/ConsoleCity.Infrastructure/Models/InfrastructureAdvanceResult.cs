namespace ConsoleCity.Infrastructure;

public sealed record InfrastructureAdvanceResult(
    InfrastructureSnapshot Snapshot,
    IReadOnlyList<InfrastructureEvent> Events);
