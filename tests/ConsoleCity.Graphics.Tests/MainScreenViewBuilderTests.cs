using ConsoleCity.Game;
using ConsoleCity.Graphics.ViewModels;
using ConsoleCity.World;

namespace ConsoleCity.Graphics.Tests;

public sealed class MainScreenViewBuilderTests
{
    [Fact]
    public void Build_WithoutWorld_ReturnsEmptyPlaceholderView()
    {
        var session = new GameSession();
        var builder = new MainScreenViewBuilder();

        var view = builder.Build(session, "1x");

        Assert.Equal("No World", view.Simulation.CityName);
        Assert.Contains("Create a world to begin.", view.Selection.Lines);
        Assert.Empty(view.Map.Cells);
    }

    [Fact]
    public void Build_WithWorld_ProjectsStatisticsAndSelection()
    {
        var session = new GameSession();
        session.CreateNewWorld(42);
        var inspector = session.GetMapInspector()!;
        var selected = inspector.GetObjectAt(new ConsoleCity.Core.GridPosition(0, 0));
        var builder = new MainScreenViewBuilder();

        var view = builder.Build(session, "2x", selectedObject: selected, isBuildMode: true, selectedBuildingType: BuildingType.Shop);

        Assert.Equal("Seedfall", view.Simulation.CityName);
        Assert.Contains(view.SummaryBar.Items, item => item.StartsWith("POP ", StringComparison.Ordinal));
        Assert.Equal(BuildingType.Shop, view.BuildPalette.SelectedBuildingType);
        Assert.True(view.BuildPalette.IsActive);
        Assert.Contains(view.Selection.Lines, line => line.StartsWith("BUILDING ", StringComparison.Ordinal));
        Assert.NotEmpty(view.Map.Cells);
    }
}
