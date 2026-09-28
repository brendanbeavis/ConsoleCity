namespace ConsoleCity.Core;

public readonly record struct Coordinate2D
{
    public double X { get; }
    public double Y { get; }

    public Coordinate2D(double x, double y)
    {
        if (!double.IsFinite(x))
        {
            throw new ArgumentOutOfRangeException(nameof(x), "X must be finite.");
        }

        if (!double.IsFinite(y))
        {
            throw new ArgumentOutOfRangeException(nameof(y), "Y must be finite.");
        }

        X = x;
        Y = y;
    }

    public Distance DistanceTo(Coordinate2D other)
    {
        var deltaX = other.X - X;
        var deltaY = other.Y - Y;
        return new Distance((decimal)Math.Sqrt(deltaX * deltaX + deltaY * deltaY));
    }

    public override string ToString() => $"({X}, {Y})";
}
