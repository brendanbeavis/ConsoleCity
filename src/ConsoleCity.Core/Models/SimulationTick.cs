namespace ConsoleCity.Core;

public readonly record struct SimulationTick
{
    public long Value { get; }

    public SimulationTick(long value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Simulation ticks cannot be negative.");
        }

        Value = value;
    }

    public static SimulationTick Zero => new(0);

    public static SimulationTick operator +(SimulationTick left, long right) => new(checked(left.Value + right));

    public static SimulationTick operator -(SimulationTick left, long right)
    {
        var value = checked(left.Value - right);
        return new(value);
    }

    public override string ToString() => Value.ToString();
}
