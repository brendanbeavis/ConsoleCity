namespace ConsoleCity.Game;

public readonly record struct GamePolicyId
{
    public string Value { get; }

    public GamePolicyId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Policy id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}