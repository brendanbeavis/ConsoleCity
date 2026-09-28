using System.Numerics;
using Raylib_cs;

namespace ConsoleCity.Graphics.Rendering;

public static class RenderScaleHelper
{
    public static Rectangle GetDestinationRectangle(int logicalWidth, int logicalHeight, int windowWidth, int windowHeight)
    {
        var scale = MathF.Min(windowWidth / (float)logicalWidth, windowHeight / (float)logicalHeight);
        var width = logicalWidth * scale;
        var height = logicalHeight * scale;
        var x = (windowWidth - width) / 2f;
        var y = (windowHeight - height) / 2f;

        return new Rectangle(x, y, width, height);
    }

    public static Vector2 MapWindowToLogical(Vector2 windowPosition, Rectangle destination, int logicalWidth, int logicalHeight)
    {
        var scaleX = destination.Width / logicalWidth;
        var scaleY = destination.Height / logicalHeight;

        if (scaleX == 0f || scaleY == 0f)
        {
            return Vector2.Zero;
        }

        return new Vector2(
            (windowPosition.X - destination.X) / scaleX,
            (windowPosition.Y - destination.Y) / scaleY);
    }

    public static Vector2 ScaleWindowDeltaToLogical(Vector2 windowDelta, Rectangle destination, int logicalWidth, int logicalHeight)
    {
        var scaleX = destination.Width / logicalWidth;
        var scaleY = destination.Height / logicalHeight;

        if (scaleX == 0f || scaleY == 0f)
        {
            return Vector2.Zero;
        }

        return new Vector2(windowDelta.X / scaleX, windowDelta.Y / scaleY);
    }
}
