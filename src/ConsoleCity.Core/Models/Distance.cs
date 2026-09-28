namespace ConsoleCity.Core;

public readonly record struct Distance
{
    public decimal Value { get; }

    public Distance(decimal value)
    {
        if (value < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Distance cannot be negative.");
        }

        Value = value;
    }

    public static Distance Zero => new(0m);

    public static Distance operator +(Distance left, Distance right) => new(left.Value + right.Value);

    public static Distance operator -(Distance left, Distance right)
    {
        var value = left.Value - right.Value;
        if (value < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(right), "Distance cannot become negative.");
        }

        return new Distance(value);
    }

    public override string ToString() => Value.ToString();
}
