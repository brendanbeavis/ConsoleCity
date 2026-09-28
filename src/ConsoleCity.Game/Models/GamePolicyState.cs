namespace ConsoleCity.Game;

public sealed record class GamePolicyState(IReadOnlyList<GamePolicyInstance> ActivePolicies)
{
    public static GamePolicyState Empty { get; } = new(Array.Empty<GamePolicyInstance>());
}
