using ConsoleCity.Core;

namespace ConsoleCity.Game;

public sealed record class GameEventRecord(
    string Id,
    GameEventCategory Category,
    SimulationTime OccurredAt,
    string Description,
    double Severity,
    IReadOnlyList<string> Causes);