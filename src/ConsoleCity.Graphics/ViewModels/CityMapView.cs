using ConsoleCity.Core;

namespace ConsoleCity.Graphics.ViewModels;

public sealed record class CityMapView
{
    public GridPosition Minimum { get; }

    public GridPosition Maximum { get; }

    public IReadOnlyList<MapCellView> Cells { get; }

    public CityMapView(GridPosition minimum, GridPosition maximum, IReadOnlyList<MapCellView> cells)
    {
        ArgumentNullException.ThrowIfNull(cells);

        Minimum = minimum;
        Maximum = maximum;
        Cells = cells;
    }
}
