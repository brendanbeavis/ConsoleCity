namespace ConsoleCity.Game;

public readonly record struct GameModifierId
{
    public string Value { get; }

    public GameModifierId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Modifier id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}