using ConsoleCity.Graphics.Models;
using ConsoleCity.Graphics.ViewModels;
using ConsoleCity.World;
using Raylib_cs;

namespace ConsoleCity.Graphics.Rendering;

public sealed class MainScreenRenderer
{
    private const int FontSize = 12;
    private const int SmallFontSize = 10;
    private const int LineHeight = 14;
    private const int Padding = 6;

    public ScreenLayout BuildLayout(int width, int height) => ScreenLayout.Create(width, height);

    public void Render(MainScreenView view, MapCamera camera, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(camera);

        var layout = BuildLayout(width, height);

        Raylib.ClearBackground(RetroPalette.Background);
        DrawPanel(layout.Header, " ");
        DrawPanel(layout.Map, "CITY MAP");
        DrawPanel(layout.SidePanel, "INSPECTOR");
        DrawPanel(layout.SummaryBar, "STATUS:");
        DrawPanel(layout.CommandBar, "COMMANDS:");

        DrawHeader(view.Simulation, layout.Header);
        DrawMap(view.Map, camera, layout.Map);
        DrawSidePanel(view, layout.SidePanel);
        DrawSummaryBar(view.SummaryBar, layout.SummaryBar);
        DrawCommands(view, layout.CommandBar);
    }

    private static void DrawPanel(PixelRect rect, string title)
    {
        Raylib.DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height, RetroPalette.PanelBackground);
        Raylib.DrawRectangleLinesEx(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height), 1f, RetroPalette.PanelBorder);
        Raylib.DrawText(title, rect.X + Padding, rect.Y + 4, SmallFontSize, RetroPalette.TextMuted);
    }

    private static void DrawHeader(SimulationStatusView view, PixelRect rect)
    {
        var state = view.IsPaused ? "PAUSED" : "RUN";
        var text = $"{view.Title}  {view.CityName}  Y{view.DateTime.Year:D4} M{view.DateTime.Month:D2} D{view.DateTime.Day:D2} {view.DateTime.Hour:D2}:00  {state}  {view.SpeedLabel}";
        Raylib.DrawText(text, rect.X + Padding, rect.Y + 8, FontSize, RetroPalette.TextPrimary);
    }

    private static void DrawMap(CityMapView view, MapCamera camera, PixelRect viewport)
    {
        Raylib.BeginScissorMode(viewport.X + 1, viewport.Y + 1, viewport.Width - 2, viewport.Height - 2);

        foreach (var cell in view.Cells)
        {
            if (!camera.IsWorldPositionVisible(cell.Position, viewport))
            {
                continue;
            }

            var screen = camera.WorldToScreen(cell.Position, viewport);
            var x = (int)MathF.Round(screen.X);
            var y = (int)MathF.Round(screen.Y);
            var size = Math.Max(2, (int)MathF.Ceiling(camera.CellSize));
            var bounds = new Rectangle(x, y, size, size);

            Raylib.DrawRectangleRec(bounds, GetTerrainColor(cell));
            DrawInfrastructure(cell, x, y, size);
            DrawBuilding(cell, x, y, size);
            Raylib.DrawRectangleLinesEx(bounds, 1f, RetroPalette.PlotOutline);

            if (cell.IsHovered)
            {
                Raylib.DrawRectangleLinesEx(bounds, 2f, RetroPalette.Hover);
            }

            if (cell.IsSelected)
            {
                Raylib.DrawRectangleLinesEx(bounds, 2f, RetroPalette.Selected);
            }
        }

        Raylib.EndScissorMode();
    }

    private static void DrawInfrastructure(MapCellView cell, int x, int y, int size)
    {
        if (cell.HasRoad)
        {
            var roadThickness = Math.Max(2, size / 5);
            Raylib.DrawRectangle(x, y + (size / 2) - (roadThickness / 2), size, roadThickness, RetroPalette.Road);
            Raylib.DrawRectangle(x + (size / 2) - (roadThickness / 2), y, roadThickness, size, RetroPalette.Road);
        }

        if (cell.HasRail)
        {
            var railColor = RetroPalette.TextMuted;
            Raylib.DrawLine(x, y + (size / 3), x + size, y + (size / 3), railColor);
            Raylib.DrawLine(x, y + ((size * 2) / 3), x + size, y + ((size * 2) / 3), railColor);
        }
    }

    private static void DrawBuilding(MapCellView cell, int x, int y, int size)
    {
        if (cell.BuildingType is null)
        {
            return;
        }

        var inset = Math.Max(1, size / 6);
        var color = GetBuildingColor(cell.BuildingType.Value);
        Raylib.DrawRectangle(x + inset, y + inset, Math.Max(2, size - (inset * 2)), Math.Max(2, size - (inset * 2)), color);
    }

    private static void DrawSidePanel(MainScreenView view, PixelRect rect)
    {
        var y = rect.Y + 20;
        DrawTextBlock(view.Selection.Lines, rect.X + Padding, ref y, rect.Width - (Padding * 2), FontSize, RetroPalette.TextPrimary, maxLines: 11);

        y += 8;
        Raylib.DrawText("BUILD", rect.X + Padding, y, SmallFontSize, RetroPalette.TextMuted);
        y += LineHeight;
        DrawTextBlock(BuildBuildLines(view.BuildPalette), rect.X + Padding, ref y, rect.Width - (Padding * 2), SmallFontSize, RetroPalette.TextPrimary, maxLines: 8);

        y += 6;
        Raylib.DrawText("CONSTRUCTION", rect.X + Padding, y, SmallFontSize, RetroPalette.TextMuted);
        y += LineHeight;
        DrawTextBlock(view.Construction.ActiveProjects, rect.X + Padding, ref y, rect.Width - (Padding * 2), SmallFontSize, RetroPalette.TextPrimary, maxLines: 6);

        y += 6;
        Raylib.DrawText("NOTIFICATIONS", rect.X + Padding, y, SmallFontSize, RetroPalette.TextMuted);
        y += LineHeight;
        DrawTextBlock(view.Notifications.Count == 0 ? ["No notifications."] : view.Notifications, rect.X + Padding, ref y, rect.Width - (Padding * 2), SmallFontSize, RetroPalette.Warning, maxLines: 6);
    }

    private static void DrawSummaryBar(SummaryBarView view, PixelRect rect)
    {
        var text = string.Join("      ", view.Items);
        Raylib.DrawText(text, rect.X + Padding + 80, rect.Y + 6, SmallFontSize, RetroPalette.TextPrimary);
    }

    private static void DrawCommands(MainScreenView view, PixelRect rect)
    {
        var mode = view.BuildPalette.IsActive ? $"BUILD:{view.BuildPalette.SelectedBuildingType}" : "INSPECT";
        var text = $"SPACE: PAUSE    .: STEP     +/-: SPEED     WHEEL: ZOOM     DRAG: PAN     CLICK: SELECT     RIGHT/ENTER: CONFIRM    B: {mode}    TAB/Q: TYPE";
        Raylib.DrawText(text, rect.X + Padding + 80, rect.Y + 7, SmallFontSize, RetroPalette.TextPrimary);
    }

    private static void DrawTextBlock(IReadOnlyList<string> lines, int x, ref int y, int width, int fontSize, Color color, int maxLines)
    {
        foreach (var line in lines.Take(maxLines))
        {
            Raylib.DrawText(TrimToWidth(line, width, fontSize), x, y, fontSize, color);
            y += LineHeight;
        }
    }

    private static IReadOnlyList<string> BuildBuildLines(BuildPaletteView view)
    {
        if (!view.IsActive)
        {
            return ["Build mode inactive."];
        }

        return view.Options
            .Where(option => option.IsSelected || option.BuildingType is BuildingType.House or BuildingType.Shop or BuildingType.Factory or BuildingType.Park or BuildingType.School)
            .Take(6)
            .Select(option => option.IsSelected ? $"> {option.BuildingType}" : $"  {option.BuildingType}")
            .ToList();
    }

    private static string TrimToWidth(string value, int width, int fontSize)
    {
        const string ellipsis = "...";
        if (Raylib.MeasureText(value, fontSize) <= width)
        {
            return value;
        }

        var current = value;
        while (current.Length > ellipsis.Length && Raylib.MeasureText(current + ellipsis, fontSize) > width)
        {
            current = current[..^1];
        }

        return current + ellipsis;
    }

    private static Color GetTerrainColor(MapCellView cell)
        => cell.IsWater
            ? RetroPalette.Water
            : cell.Terrain switch
            {
                TerrainType.Forest => RetroPalette.Forest,
                TerrainType.Hills or TerrainType.Mountains => RetroPalette.Hills,
                TerrainType.Desert => RetroPalette.Desert,
                _ => RetroPalette.Plains
            };

    private static Color GetBuildingColor(BuildingType buildingType)
        => buildingType switch
        {
            BuildingType.House or BuildingType.Apartment => RetroPalette.BuildingResidential,
            BuildingType.Shop or BuildingType.Office => RetroPalette.BuildingCommercial,
            BuildingType.Factory or BuildingType.Farm or BuildingType.UtilityFacility or BuildingType.TransportFacility => RetroPalette.BuildingIndustrial,
            _ => RetroPalette.BuildingService
        };
}
