using System.Numerics;

namespace ConsoleCity.Graphics.Models;

public readonly record struct PixelRect
{
    public int X { get; }

    public int Y { get; }

    public int Width { get; }

    public int Height { get; }

    public int Right => X + Width;

    public int Bottom => Y + Height;

    public Vector2 TopLeft => new(X, Y);

    public Vector2 Center => new(X + (Width / 2f), Y + (Height / 2f));

    public PixelRect(int x, int y, int width, int height)
    {
        if (width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public bool Contains(Vector2 point)
        => point.X >= X && point.X < Right && point.Y >= Y && point.Y < Bottom;
}
