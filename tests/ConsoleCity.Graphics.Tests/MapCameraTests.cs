using System.Numerics;
using ConsoleCity.Core;
using ConsoleCity.Graphics.Models;
using ConsoleCity.Graphics.Rendering;

namespace ConsoleCity.Graphics.Tests;

public sealed class MapCameraTests
{
    [Fact]
    public void ScreenToWorld_RoundTripsVisibleCell()
    {
        var viewport = new PixelRect(0, 0, 320, 180);
        var camera = new MapCamera(baseCellSize: 16f);
        camera.SetBounds(new GridPosition(0, 0), new GridPosition(3, 2), viewport);

        var world = new GridPosition(1, 1);
        var screen = camera.WorldToScreen(world, viewport) + new Vector2(camera.CellSize / 2f, camera.CellSize / 2f);

        var result = camera.ScreenToWorld(screen, viewport);

        Assert.Equal(world, result);
    }

    [Fact]
    public void PanPixels_ChangesWorldPositionUnderViewportCenter()
    {
        var viewport = new PixelRect(0, 0, 120, 120);
        var camera = new MapCamera(baseCellSize: 16f);
        camera.SetBounds(new GridPosition(0, 0), new GridPosition(20, 20), viewport);

        var before = camera.ScreenToWorld(viewport.Center, viewport);

        camera.PanPixels(new Vector2(-16f, 0f), viewport);

        var after = camera.ScreenToWorld(viewport.Center, viewport);
        Assert.True(after.X > before.X);
    }

    [Fact]
    public void ZoomAt_PreservesAnchorCell()
    {
        var viewport = new PixelRect(0, 0, 320, 180);
        var camera = new MapCamera(baseCellSize: 16f);
        camera.SetBounds(new GridPosition(0, 0), new GridPosition(20, 20), viewport);

        var anchor = new Vector2(100f, 70f);
        var before = camera.ScreenToWorld(anchor, viewport);

        camera.ZoomAt(1f, anchor, viewport);

        var after = camera.ScreenToWorld(anchor, viewport);
        Assert.Equal(before, after);
    }
}
