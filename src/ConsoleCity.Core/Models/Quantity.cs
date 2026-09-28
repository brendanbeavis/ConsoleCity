namespace ConsoleCity.Core;

public readonly record struct Quantity
{
    public decimal Value { get; }

    public Quantity(decimal value)
    {
        if (value < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Quantity cannot be negative.");
        }

        Value = value;
    }

    public static Quantity Zero => new(0m);

    public static Quantity operator +(Quantity left, Quantity right) => new(left.Value + right.Value);

    public static Quantity operator -(Quantity left, Quantity right)
    {
        var value = left.Value - right.Value;
        if (value < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(right), "Quantity cannot become negative.");
        }

        return new Quantity(value);
    }

    public override string ToString() => Value.ToString();
}
