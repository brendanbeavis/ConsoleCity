namespace ConsoleCity.Core;

public readonly record struct ResourceId
{
    public string Value { get; }

    public ResourceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Resource identifiers cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
