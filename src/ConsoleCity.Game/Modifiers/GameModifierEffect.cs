namespace ConsoleCity.Game;

public sealed record class GameModifierEffect
{
    public string Target { get; }

    public ModifierEffectKind Kind { get; }

    public decimal Value { get; }

    public GameModifierEffect(string target, ModifierEffectKind kind, decimal value)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException("Modifier effect target cannot be empty.", nameof(target));
        }

        Target = target.Trim();
        Kind = kind;
        Value = value;
    }
}