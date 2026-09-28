using System.Numerics;
using ConsoleCity.Graphics.Rendering;
using Raylib_cs;

namespace ConsoleCity.Graphics.Tests;

public sealed class RenderScaleHelperTests
{
    [Fact]
    public void MapWindowToLogical_AccountsForCenteredScaledRenderSurface()
    {
        var destination = new Rectangle(320f, 180f, 1280f, 720f);

        var logical = RenderScaleHelper.MapWindowToLogical(new Vector2(960f, 540f), destination, 1280, 720);

        Assert.Equal(new Vector2(640f, 360f), logical);
    }

    [Fact]
    public void ScaleWindowDeltaToLogical_UsesInverseScale()
    {
        var destination = new Rectangle(320f, 180f, 640f, 360f);

        var logicalDelta = RenderScaleHelper.ScaleWindowDeltaToLogical(new Vector2(20f, 10f), destination, 1280, 720);

        Assert.Equal(new Vector2(40f, 20f), logicalDelta);
    }
}
