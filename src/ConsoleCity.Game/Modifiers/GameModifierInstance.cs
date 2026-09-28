using ConsoleCity.Core;

namespace ConsoleCity.Game;

public sealed record class GameModifierInstance(
    GameModifierId Id,
    SimulationTime AcquiredAt,
    SimulationTime? ExpiresAt,
    int Stacks);