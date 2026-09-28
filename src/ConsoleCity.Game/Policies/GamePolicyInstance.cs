using ConsoleCity.Core;

namespace ConsoleCity.Game;

public sealed record class GamePolicyInstance(GamePolicyId Id, decimal Intensity, SimulationTime SetAt);