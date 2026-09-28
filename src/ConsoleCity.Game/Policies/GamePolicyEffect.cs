namespace ConsoleCity.Game;

public sealed record class GamePolicyEffect(string Target, ModifierEffectKind Kind, decimal Value);