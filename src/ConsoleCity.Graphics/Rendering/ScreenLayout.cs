using ConsoleCity.Graphics.Models;

namespace ConsoleCity.Graphics.Rendering;

public sealed record class ScreenLayout(
    PixelRect Header,
    PixelRect Map,
    PixelRect SidePanel,
    PixelRect SummaryBar,
    PixelRect CommandBar)
{
    public static ScreenLayout Create(int width, int height)
    {
        const int headerHeight = 22;
        const int summaryHeight = 20;
        const int commandHeight = 24;
        const int sidePanelWidth = 200;
        const int gutter = 4;

        var bodyY = headerHeight + gutter;
        var bodyHeight = height - headerHeight - summaryHeight - commandHeight - (gutter * 3);
        var mapWidth = width - sidePanelWidth - gutter;

        return new ScreenLayout(
            new PixelRect(0, 0, width, headerHeight),
            new PixelRect(0, bodyY, mapWidth, bodyHeight),
            new PixelRect(mapWidth + gutter, bodyY, sidePanelWidth, bodyHeight),
            new PixelRect(0, bodyY + bodyHeight + gutter, width, summaryHeight),
            new PixelRect(0, bodyY + bodyHeight + summaryHeight + (gutter * 2), width, commandHeight));
    }
}
