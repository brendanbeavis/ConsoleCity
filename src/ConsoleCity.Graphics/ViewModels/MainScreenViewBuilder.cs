using ConsoleCity.Core;
using ConsoleCity.Game;
using ConsoleCity.World;

namespace ConsoleCity.Graphics.ViewModels;

public sealed class MainScreenViewBuilder
{
    public MainScreenView Build(
        GameSession session,
        string speedLabel,
        MapObject? selectedObject = null,
        MapObject? hoveredObject = null,
        bool isBuildMode = false,
        BuildingType? selectedBuildingType = null,
        IReadOnlyList<string>? notifications = null)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentException.ThrowIfNullOrWhiteSpace(speedLabel);

        if (!session.IsWorldCreated || session.Snapshot is null)
        {
            return CreateEmptyView(speedLabel, notifications);
        }

        var snapshot = session.Snapshot;
        var mapView = new MapView(snapshot.World);
        var statistics = session.GetCityStatistics();
        var simulation = new SimulationStatusView(
            title: "CONSOLECITY",
            cityName: statistics.CityName,
            dateTime: snapshot.CurrentTime.ToGameDateTime(),
            isPaused: !session.IsRunning,
            speedLabel: speedLabel);

        var selectedPosition = selectedObject?.Position;
        var hoveredPosition = hoveredObject?.Position;
        var cells = mapView.GetAllCells()
            .Select(cell => new MapCellView(
                cell.Position,
                cell.Terrain,
                cell.IsWater,
                cell.HasRoad,
                cell.HasRail,
                cell.BuildingType,
                cell.LandUse,
                IsMatch(cell.Position, selectedPosition),
                IsMatch(cell.Position, hoveredPosition)))
            .ToList();

        var summaryItems = new List<string>
        {
            $"POP {statistics.Population}",
            $"HOUSEHOLDS {statistics.Households}",
            $"JOBS {statistics.EmployedPeople}",
            $"BUSINESSES {statistics.Businesses}",
            $"TRIPS {statistics.ActiveTrips}",
            $"FOOD {statistics.FoodInventoryUnits}",
            $"SAVINGS ${statistics.TotalHouseholdSavings.Amount:0}",
            $"BUSINESS CASH ${statistics.TotalBusinessCash.Amount:0}"
        };

        var buildOptions = Enum.GetValues<BuildingType>()
            .Select(type => new BuildOptionView(type, selectedBuildingType == type))
            .ToList();

        return new MainScreenView(
            simulation,
            new CityMapView(mapView.Bounds.Min, mapView.Bounds.Max, cells),
            BuildSelectionView(session, snapshot, selectedObject),
            new SummaryBarView(summaryItems),
            new BuildPaletteView(isBuildMode, selectedBuildingType, buildOptions),
            new ConstructionPanelView(BuildConstructionLines(session)),
            notifications ?? []);
    }

    private static MainScreenView CreateEmptyView(string speedLabel, IReadOnlyList<string>? notifications)
    {
        var simulation = new SimulationStatusView(
            title: "CONSOLECITY",
            cityName: "No World",
            dateTime: new SimulationTime(0).ToGameDateTime(),
            isPaused: true,
            speedLabel: speedLabel);

        return new MainScreenView(
            simulation,
            new CityMapView(new GridPosition(0, 0), new GridPosition(0, 0), []),
            new SelectionView("SELECTED", null, null, null, ["Create a world to begin."]),
            new SummaryBarView(["POP 0", "HOUSEHOLDS 0", "JOBS 0"]),
            new BuildPaletteView(false, null, []),
            new ConstructionPanelView([]),
            notifications ?? []);
    }

    private static SelectionView BuildSelectionView(GameSession session, SimulationSliceSnapshot snapshot, MapObject? selectedObject)
    {
        if (selectedObject is null)
        {
            return new SelectionView("SELECTED", null, null, null, ["Nothing selected."]);
        }

        return selectedObject.Type switch
        {
            ObjectType.Building => new SelectionView(
                "SELECTED",
                selectedObject.Type,
                selectedObject.Id,
                selectedObject.Position,
                SplitLines(session.InspectBuilding(selectedObject.Id))),
            ObjectType.Plot => new SelectionView(
                "SELECTED",
                selectedObject.Type,
                selectedObject.Id,
                selectedObject.Position,
                BuildPlotLines(snapshot.World, selectedObject.Id)),
            ObjectType.Road => new SelectionView(
                "SELECTED",
                selectedObject.Type,
                selectedObject.Id,
                selectedObject.Position,
                [
                    $"ROAD {selectedObject.Position}",
                    "Type: Surface road",
                    "Status: Connected"
                ]),
            _ => new SelectionView(
                "SELECTED",
                selectedObject.Type,
                selectedObject.Id,
                selectedObject.Position,
                [$"{selectedObject.Type} {selectedObject.Position}"])
        };
    }

    private static IReadOnlyList<string> BuildPlotLines(WorldModel world, string plotId)
    {
        var plot = world.Regions
            .SelectMany(region => region.Cities)
            .SelectMany(city => city.Districts)
            .SelectMany(district => district.Plots)
            .FirstOrDefault(item => item.Id.Value.ToString() == plotId);

        if (plot is null)
        {
            return ["Plot not found."];
        }

        return
        [
            $"PLOT {plot.Name}",
            $"Id: {plot.Id}",
            $"Land use: {plot.LandUse}",
            $"Location: {plot.Location}",
            $"Cells: {plot.Cells.Count}",
            $"Buildings: {plot.Buildings.Count}",
            $"State: {plot.State}"
        ];
    }

    private static IReadOnlyList<string> BuildConstructionLines(GameSession session)
    {
        var constructions = session.GetActiveConstructions().Values
            .OrderBy(construction => construction.RequestedAt.Tick)
            .Take(4)
            .Select(construction =>
                $"{construction.BuildingType} {construction.Location} {construction.GetProgress():0}% {construction.State}")
            .ToList();

        return constructions.Count == 0
            ? ["No active construction."]
            : constructions;
    }

    private static bool IsMatch(GridPosition position, GridPosition? other)
        => other.HasValue && other.Value == position;

    private static IReadOnlyList<string> SplitLines(string value)
        => value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
}
