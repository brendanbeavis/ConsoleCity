namespace ConsoleCity.Game;

public readonly record struct TechnologyId
{
    public string Value { get; }

    public TechnologyId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Technology id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}