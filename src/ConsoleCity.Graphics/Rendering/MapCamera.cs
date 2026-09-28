using System.Numerics;
using ConsoleCity.Core;
using ConsoleCity.Graphics.Models;

namespace ConsoleCity.Graphics.Rendering;

public sealed class MapCamera
{
    private const float MinimumZoom = 0.5f;
    private const float MaximumZoom = 6f;
    private const float ZoomStep = 0.1f;

    private Vector2 worldTopLeft;

    public GridPosition MinimumBounds { get; private set; }

    public GridPosition MaximumBounds { get; private set; }

    public float BaseCellSize { get; }

    public float Zoom { get; private set; }

    public float CellSize => BaseCellSize * Zoom;

    public MapCamera(float baseCellSize = 16f, float zoom = 1f)
    {
        if (baseCellSize <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(baseCellSize));
        }

        BaseCellSize = baseCellSize;
        Zoom = Math.Clamp(zoom, MinimumZoom, MaximumZoom);
        MinimumBounds = new GridPosition(0, 0);
        MaximumBounds = new GridPosition(0, 0);
        worldTopLeft = Vector2.Zero;
    }

    public void SetBounds(GridPosition minimum, GridPosition maximum, PixelRect viewport)
    {
        MinimumBounds = minimum;
        MaximumBounds = maximum;
        CenterOnBounds(viewport);
    }

    public void CenterOnBounds(PixelRect viewport)
    {
        var worldWidth = MaximumBounds.X - MinimumBounds.X + 1;
        var worldHeight = MaximumBounds.Y - MinimumBounds.Y + 1;
        var visibleWidth = viewport.Width / CellSize;
        var visibleHeight = viewport.Height / CellSize;

        worldTopLeft = new Vector2(
            MinimumBounds.X + ((worldWidth - visibleWidth) / 2f),
            MinimumBounds.Y + ((worldHeight - visibleHeight) / 2f));

        ClampToBounds(viewport);
    }

    public void PanPixels(Vector2 deltaPixels, PixelRect viewport)
    {
        worldTopLeft -= deltaPixels / CellSize;
        ClampToBounds(viewport);
    }

    public void ZoomAt(float mouseWheelDelta, Vector2 anchorScreenPosition, PixelRect viewport)
    {
        if (mouseWheelDelta == 0f)
        {
            return;
        }

        var worldBeforeZoom = ScreenToWorldPosition(anchorScreenPosition, viewport);
        var adjustedZoom = Zoom + (mouseWheelDelta * ZoomStep);
        Zoom = Math.Clamp(adjustedZoom, MinimumZoom, MaximumZoom);
        var worldAfterZoom = ScreenToWorldPosition(anchorScreenPosition, viewport);
        worldTopLeft += worldBeforeZoom - worldAfterZoom;
        ClampToBounds(viewport);
    }

    public Vector2 WorldToScreen(GridPosition worldPosition, PixelRect viewport)
    {
        var cellX = (worldPosition.X - worldTopLeft.X) * CellSize;
        var cellY = (worldPosition.Y - worldTopLeft.Y) * CellSize;
        return new Vector2(viewport.X + cellX, viewport.Y + cellY);
    }

    public GridPosition ScreenToWorld(Vector2 screenPosition, PixelRect viewport)
    {
        var world = ScreenToWorldPosition(screenPosition, viewport);
        return new GridPosition((int)MathF.Floor(world.X), (int)MathF.Floor(world.Y));
    }

    public bool IsWorldPositionVisible(GridPosition position, PixelRect viewport)
    {
        var screen = WorldToScreen(position, viewport);
        return screen.X + CellSize >= viewport.X
            && screen.Y + CellSize >= viewport.Y
            && screen.X < viewport.Right
            && screen.Y < viewport.Bottom;
    }

    private Vector2 ScreenToWorldPosition(Vector2 screenPosition, PixelRect viewport)
    {
        return new Vector2(
            worldTopLeft.X + ((screenPosition.X - viewport.X) / CellSize),
            worldTopLeft.Y + ((screenPosition.Y - viewport.Y) / CellSize));
    }

    private void ClampToBounds(PixelRect viewport)
    {
        var worldWidth = MaximumBounds.X - MinimumBounds.X + 1;
        var worldHeight = MaximumBounds.Y - MinimumBounds.Y + 1;
        var visibleWidth = viewport.Width / CellSize;
        var visibleHeight = viewport.Height / CellSize;

        worldTopLeft = new Vector2(
            ClampAxis(worldTopLeft.X, MinimumBounds.X, worldWidth, visibleWidth),
            ClampAxis(worldTopLeft.Y, MinimumBounds.Y, worldHeight, visibleHeight));
    }

    private static float ClampAxis(float value, int minBound, int worldSpan, float visibleSpan)
    {
        if (visibleSpan >= worldSpan)
        {
            return minBound - ((visibleSpan - worldSpan) / 2f);
        }

        var maxTopLeft = minBound + worldSpan - visibleSpan;
        return Math.Clamp(value, minBound, maxTopLeft);
    }
}
