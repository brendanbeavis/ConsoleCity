using ConsoleCity.Core;

namespace ConsoleCity.Game;

public sealed record class ProgressionLogEntry(
    SimulationTime At,
    string Category,
    string Message);