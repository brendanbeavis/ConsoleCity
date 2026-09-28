namespace ConsoleCity.Graphics.ViewModels;

public sealed record class MainScreenView
{
    public SimulationStatusView Simulation { get; }

    public CityMapView Map { get; }

    public SelectionView Selection { get; }

    public SummaryBarView SummaryBar { get; }

    public BuildPaletteView BuildPalette { get; }

    public ConstructionPanelView Construction { get; }

    public IReadOnlyList<string> Notifications { get; }

    public MainScreenView(
        SimulationStatusView simulation,
        CityMapView map,
        SelectionView selection,
        SummaryBarView summaryBar,
        BuildPaletteView buildPalette,
        ConstructionPanelView construction,
        IReadOnlyList<string> notifications)
    {
        ArgumentNullException.ThrowIfNull(simulation);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(summaryBar);
        ArgumentNullException.ThrowIfNull(buildPalette);
        ArgumentNullException.ThrowIfNull(construction);
        ArgumentNullException.ThrowIfNull(notifications);

        Simulation = simulation;
        Map = map;
        Selection = selection;
        SummaryBar = summaryBar;
        BuildPalette = buildPalette;
        Construction = construction;
        Notifications = notifications;
    }
}
