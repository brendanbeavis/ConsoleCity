using System.Text;
using ConsoleCity.Core;

namespace ConsoleCity.World;

/// <summary>
/// Renders a MapView as ASCII/text suitable for console display.
/// </summary>
public sealed class MapRenderer
{
    private readonly MapView mapView;

    public MapRenderer(MapView mapView)
    {
        ArgumentNullException.ThrowIfNull(mapView);
        this.mapView = mapView;
    }

    /// <summary>
    /// Render the entire map as ASCII art.
    /// </summary>
    public string RenderFullMap()
    {
        var cells = mapView.GetAllCells();
        if (cells.Count == 0)
        {
            return "[Empty Map]";
        }

        var (min, max) = mapView.Bounds;
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"╔ WORLD MAP {min} to {max} ╗");
        sb.AppendLine();

        // Render grid
        for (int y = min.Y; y <= max.Y; y++)
        {
            for (int x = min.X; x <= max.X; x++)
            {
                var cell = mapView.GetCell(new GridPosition(x, y));
                sb.Append(cell.DisplayChar);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Render a specific rectangular region of the map.
    /// </summary>
    public string RenderRegion(GridPosition topLeft, GridPosition bottomRight)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"╔ MAP REGION {topLeft} to {bottomRight} ╗");
        sb.AppendLine();

        for (int y = topLeft.Y; y <= bottomRight.Y; y++)
        {
            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                var cell = mapView.GetCell(new GridPosition(x, y));
                sb.Append(cell.DisplayChar);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Render map with coordinates for reference.
    /// </summary>
    public string RenderWithCoordinates()
    {
        var (min, max) = mapView.Bounds;
        var sb = new StringBuilder();

        sb.AppendLine($"╔ WORLD MAP (with coordinates) ╗");
        sb.AppendLine();

        // Column headers
        sb.Append("    ");
        for (int x = min.X; x <= max.X; x++)
        {
            sb.Append((x % 10).ToString());
        }
        sb.AppendLine();

        // Rows with row numbers
        for (int y = min.Y; y <= max.Y; y++)
        {
            sb.Append($"{y:D3} ");
            for (int x = min.X; x <= max.X; x++)
            {
                var cell = mapView.GetCell(new GridPosition(x, y));
                sb.Append(cell.DisplayChar);
            }
            sb.Append(" ");
            sb.AppendLine(y.ToString());
        }

        return sb.ToString();
    }

    /// <summary>
    /// Render map with legend.
    /// </summary>
    public string RenderWithLegend()
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔ WORLD MAP ╗");
        sb.AppendLine();
        sb.Append(RenderFullMap());
        sb.AppendLine();
        sb.AppendLine("╔ LEGEND ╗");
        sb.AppendLine("H: House         A: Apartment    S: Shop");
        sb.AppendLine("O: Office        F: Factory      E: School");
        sb.AppendLine("P: Police        R: Fire Station .: Park");
        sb.AppendLine("U: Utility       T: Transport    -: Road");
        sb.AppendLine("|: Rail          +: Road+Rail    ~: Water");
        sb.AppendLine("^: Forest/Mount  h: Hills        `: Desert");
        sb.AppendLine(".: Plains        ?: Unknown");

        return sb.ToString();
    }

    /// <summary>
    /// Render a summary of all buildings on the map.
    /// </summary>
    public string RenderBuildingSummary()
    {
        var buildings = mapView.GetAllBuildings();
        var sb = new StringBuilder();

        sb.AppendLine("╔ BUILDINGS ╗");
        sb.AppendLine();

        if (buildings.Count == 0)
        {
            sb.AppendLine("[No buildings]");
        }
        else
        {
            var groupedByType = buildings
                .GroupBy(b => b.Type)
                .OrderBy(g => g.Key.ToString());

            foreach (var group in groupedByType)
            {
                sb.AppendLine($"{group.Key}:");
                foreach (var (id, _, pos) in group.OrderBy(b => b.Position.ToString()))
                {
                    sb.AppendLine($"  {id} @ {pos}");
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Total: {buildings.Count} buildings");

        return sb.ToString();
    }

    /// <summary>
    /// Render a summary of all plots on the map.
    /// </summary>
    public string RenderPlotSummary()
    {
        var plots = mapView.GetAllPlots();
        var sb = new StringBuilder();

        sb.AppendLine("╔ PLOTS ╗");
        sb.AppendLine();

        if (plots.Count == 0)
        {
            sb.AppendLine("[No plots]");
        }
        else
        {
            var groupedByUse = plots
                .GroupBy(p => p.LandUse)
                .OrderBy(g => g.Key.ToString());

            foreach (var group in groupedByUse)
            {
                sb.AppendLine($"{group.Key}:");
                foreach (var (id, _, pos) in group.OrderBy(p => p.Location.ToString()))
                {
                    sb.AppendLine($"  {id} @ {pos}");
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Total: {plots.Count} plots");

        return sb.ToString();
    }

    /// <summary>
    /// Get a detailed report for a specific cell.
    /// </summary>
    public string RenderCellDetail(GridPosition position)
    {
        var cell = mapView.GetCell(position);
        var sb = new StringBuilder();

        sb.AppendLine($"╔ CELL DETAIL {position} ╗");
        sb.AppendLine();
        sb.AppendLine($"Position: {cell.Position}");
        sb.AppendLine($"Terrain: {cell.Terrain}");
        sb.AppendLine($"Elevation: {cell.Elevation}");
        sb.AppendLine();

        if (cell.IsWater)
        {
            sb.AppendLine("Water: Yes");
        }

        if (cell.HasRoad)
        {
            sb.AppendLine("Road: Yes");
        }

        if (cell.HasRail)
        {
            sb.AppendLine("Rail: Yes");
        }

        if (cell.PlotId.HasValue)
        {
            sb.AppendLine($"Plot: {cell.PlotId}");
            sb.AppendLine($"Land Use: {cell.LandUse}");
        }

        if (cell.BuildingId.HasValue)
        {
            sb.AppendLine($"Building: {cell.BuildingId}");
            sb.AppendLine($"Type: {cell.BuildingType}");
        }

        sb.AppendLine();
        sb.AppendLine(cell.GetDisplayText());

        return sb.ToString();
    }
}
