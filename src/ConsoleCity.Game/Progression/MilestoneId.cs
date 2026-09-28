namespace ConsoleCity.Game;

public readonly record struct MilestoneId
{
    public string Value { get; }

    public MilestoneId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Milestone id cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}