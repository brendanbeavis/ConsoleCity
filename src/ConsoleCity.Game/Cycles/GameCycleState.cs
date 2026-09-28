using ConsoleCity.Core;

namespace ConsoleCity.Game;

public sealed record class GameCycleState(
    int CycleNumber,
    SimulationTime StartedAt,
    SimulationTime NextTransitionAt,
    int CycleLengthTicks)
{
    public static GameCycleState CreateInitial(SimulationTime at, int cycleLengthTicks = 720)
        => new(1, at, at.Advance(cycleLengthTicks), cycleLengthTicks);

    public GameCycleState Advance()
        => new(CycleNumber + 1, NextTransitionAt, NextTransitionAt.Advance(CycleLengthTicks), CycleLengthTicks);
}