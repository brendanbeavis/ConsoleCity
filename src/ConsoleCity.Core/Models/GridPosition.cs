namespace ConsoleCity.Core;

public readonly record struct GridPosition
{
    public int X { get; }
    public int Y { get; }

    public GridPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public GridPosition Offset(int dx, int dy) => new(X + dx, Y + dy);

    public Distance DistanceTo(GridPosition other)
    {
        var deltaX = other.X - X;
        var deltaY = other.Y - Y;
        return new Distance((decimal)Math.Sqrt(deltaX * deltaX + deltaY * deltaY));
    }

    public Coordinate2D ToCoordinate() => new(X, Y);

    public override string ToString() => $"({X}, {Y})";
}
