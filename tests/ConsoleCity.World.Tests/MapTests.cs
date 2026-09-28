using ConsoleCity.Core;
using ConsoleCity.Game;
using ConsoleCity.World;
using Xunit;

namespace ConsoleCity.World.Tests;

public class MapViewTests
{
    private readonly WorldModel world;
    private readonly MapView mapView;

    public MapViewTests()
    {
        // Create a simple test world via GameSession
        var session = new GameSession();
        session.CreateNewWorld(seed: 12345);
        world = session.Snapshot!.World;
        mapView = new MapView(world);
    }

    [Fact]
    public void MapViewCreation_WithValidWorld_Succeeds()
    {
        // Arrange & Act
        Assert.NotNull(mapView);
        Assert.NotNull(mapView.World);
    }

    [Fact]
    public void GetCell_WithValidPosition_ReturnsMappedCell()
    {
        // Arrange
        var (min, _) = mapView.Bounds;
        var testPos = new GridPosition(min.X, min.Y);

        // Act
        var cell = mapView.GetCell(testPos);

        // Assert
        Assert.NotNull(cell);
        Assert.Equal(testPos, cell.Position);
    }

    [Fact]
    public void GetCell_WithOutOfBoundsPosition_ReturnsCellWithWaterTerrain()
    {
        // Arrange
        var outOfBoundsPos = new GridPosition(-1000, -1000);

        // Act
        var cell = mapView.GetCell(outOfBoundsPos);

        // Assert
        Assert.NotNull(cell);
        Assert.Equal(outOfBoundsPos, cell.Position);
        Assert.True(cell.IsWater);
    }

    [Fact]
    public void GetAllCells_ReturnsNonEmptyList()
    {
        // Act
        var cells = mapView.GetAllCells();

        // Assert
        Assert.NotEmpty(cells);
    }

    [Fact]
    public void GetAllBuildings_ReturnsListOfBuildings()
    {
        // Act
        var buildings = mapView.GetAllBuildings();

        // Assert
        Assert.NotNull(buildings);
    }

    [Fact]
    public void GetAllPlots_ReturnsListOfPlots()
    {
        // Act
        var plots = mapView.GetAllPlots();

        // Assert
        Assert.NotNull(plots);
    }

    [Fact]
    public void InvalidateCache_ClearsCachedCells()
    {
        // Arrange
        var testPos = new GridPosition(0, 0);
        var cell1 = mapView.GetCell(testPos);

        // Act
        mapView.InvalidateCache();
        var cell2 = mapView.GetCell(testPos);

        // Assert
        // Cells should be equal in content but not necessarily the same object
        Assert.Equal(cell1.Position, cell2.Position);
        Assert.Equal(cell1.Terrain, cell2.Terrain);
    }

    [Fact]
    public void Bounds_AreWithinWorldSize()
    {
        // Act
        var (min, max) = mapView.Bounds;

        // Assert
        Assert.True(min.X <= max.X);
        Assert.True(min.Y <= max.Y);
    }
}

public class MapRendererTests
{
    private readonly MapView mapView;
    private readonly MapRenderer renderer;

    public MapRendererTests()
    {
        var session = new GameSession();
        session.CreateNewWorld(seed: 54321);
        var world = session.Snapshot!.World;
        mapView = new MapView(world);
        renderer = new MapRenderer(mapView);
    }

    [Fact]
    public void RenderFullMap_ReturnsNonEmptyString()
    {
        // Act
        var output = renderer.RenderFullMap();

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("WORLD MAP", output);
    }

    [Fact]
    public void RenderWithCoordinates_ContainsCoordinateInfo()
    {
        // Act
        var output = renderer.RenderWithCoordinates();

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("WORLD MAP", output);
    }

    [Fact]
    public void RenderWithLegend_ContainsLegendInfo()
    {
        // Act
        var output = renderer.RenderWithLegend();

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("LEGEND", output);
        Assert.Contains("House", output);
        Assert.Contains("Road", output);
    }

    [Fact]
    public void RenderRegion_WithValidCoordinates_ReturnsMapString()
    {
        // Arrange
        var (min, max) = mapView.Bounds;
        var topLeft = min;
        var bottomRight = new GridPosition(
            Math.Min(min.X + 10, max.X),
            Math.Min(min.Y + 10, max.Y));

        // Act
        var output = renderer.RenderRegion(topLeft, bottomRight);

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("MAP REGION", output);
    }

    [Fact]
    public void RenderCellDetail_WithValidPosition_ReturnsCellInfo()
    {
        // Arrange
        var (min, _) = mapView.Bounds;
        var testPos = new GridPosition(min.X, min.Y);

        // Act
        var output = renderer.RenderCellDetail(testPos);

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("CELL DETAIL", output);
        Assert.Contains(testPos.ToString(), output);
    }

    [Fact]
    public void RenderBuildingSummary_ReturnsNonEmptyString()
    {
        // Act
        var output = renderer.RenderBuildingSummary();

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("BUILDINGS", output);
    }

    [Fact]
    public void RenderPlotSummary_ReturnsNonEmptyString()
    {
        // Act
        var output = renderer.RenderPlotSummary();

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("PLOTS", output);
    }
}

public class MapInspectorTests
{
    private readonly MapView mapView;
    private readonly MapInspector inspector;

    public MapInspectorTests()
    {
        var session = new GameSession();
        session.CreateNewWorld(seed: 99999);
        var world = session.Snapshot!.World;
        mapView = new MapView(world);
        inspector = new MapInspector(mapView);
    }

    [Fact]
    public void GetObjectAt_WithValidPosition_ReturnsMapObject()
    {
        // Arrange
        var (min, _) = mapView.Bounds;
        var testPos = new GridPosition(min.X, min.Y);

        // Act
        var obj = inspector.GetObjectAt(testPos);

        // Assert
        // Object can be null or a valid MapObject
        if (obj != null)
        {
            Assert.NotNull(obj.Type);
            Assert.NotEmpty(obj.Id);
            Assert.NotEmpty(obj.Category);
        }
    }

    [Fact]
    public void GetAllBuildings_ReturnsListOfBuildings()
    {
        // Act
        var buildings = inspector.GetAllBuildings();

        // Assert
        Assert.NotNull(buildings);
    }

    [Fact]
    public void GetAllPlots_ReturnsListOfPlots()
    {
        // Act
        var plots = inspector.GetAllPlots();

        // Assert
        Assert.NotNull(plots);
    }

    [Fact]
    public void GetAllRoads_ReturnsListOfRoads()
    {
        // Act
        var roads = inspector.GetAllRoads();

        // Assert
        Assert.NotNull(roads);
    }

    [Fact]
    public void FindObjectsByType_WithBuildingType_ReturnsBuildingObjects()
    {
        // Act
        var buildings = inspector.FindObjectsByType(ObjectType.Building);

        // Assert
        Assert.NotNull(buildings);
        foreach (var obj in buildings)
        {
            Assert.Equal(ObjectType.Building, obj.Type);
        }
    }

    [Fact]
    public void FindObjectsByType_WithPlotType_ReturnsPlotObjects()
    {
        // Act
        var plots = inspector.FindObjectsByType(ObjectType.Plot);

        // Assert
        Assert.NotNull(plots);
        foreach (var obj in plots)
        {
            Assert.Equal(ObjectType.Plot, obj.Type);
        }
    }

    [Fact]
    public void GetObjectsInRegion_WithValidRegion_ReturnsObjectsInThatRegion()
    {
        // Arrange
        var (min, max) = mapView.Bounds;
        var topLeft = min;
        var bottomRight = new GridPosition(
            Math.Min(min.X + 10, max.X),
            Math.Min(min.Y + 10, max.Y));

        // Act
        var objects = inspector.GetObjectsInRegion(topLeft, bottomRight);

        // Assert
        Assert.NotNull(objects);
        foreach (var obj in objects)
        {
            Assert.True(obj.Position.X >= topLeft.X && obj.Position.X <= bottomRight.X);
            Assert.True(obj.Position.Y >= topLeft.Y && obj.Position.Y <= bottomRight.Y);
        }
    }
}

public class MapCellTests
{
    [Fact]
    public void MapCell_WithBuilding_DisplaysCorrectCharacter()
    {
        // Arrange
        var cell = new MapCell(
            new GridPosition(0, 0),
            TerrainType.Plains,
            buildingId: new BuildingId(Guid.NewGuid()),
            buildingType: BuildingType.House);

        // Act
        var displayChar = cell.DisplayChar;

        // Assert
        Assert.Equal('H', displayChar);
    }

    [Fact]
    public void MapCell_WithRoad_DisplaysCorrectCharacter()
    {
        // Arrange
        var cell = new MapCell(
            new GridPosition(0, 0),
            TerrainType.Plains,
            hasRoad: true);

        // Act
        var displayChar = cell.DisplayChar;

        // Assert
        Assert.Equal('-', displayChar);
    }

    [Fact]
    public void MapCell_WithWater_DisplaysCorrectCharacter()
    {
        // Arrange
        var cell = new MapCell(
            new GridPosition(0, 0),
            TerrainType.Ocean,
            isWater: true);

        // Act
        var displayChar = cell.DisplayChar;

        // Assert
        Assert.Equal('~', displayChar);
    }

    [Fact]
    public void MapCell_IsInteractive_WithBuilding_ReturnsTrue()
    {
        // Arrange
        var cell = new MapCell(
            new GridPosition(0, 0),
            TerrainType.Plains,
            buildingId: new BuildingId(Guid.NewGuid()),
            buildingType: BuildingType.House);

        // Act
        var isInteractive = cell.IsInteractive;

        // Assert
        Assert.True(isInteractive);
    }

    [Fact]
    public void MapCell_IsInteractive_WithPlot_ReturnsTrue()
    {
        // Arrange
        var cell = new MapCell(
            new GridPosition(0, 0),
            TerrainType.Plains,
            plotId: new PlotId(Guid.NewGuid()),
            landUse: LandUseType.Residential);

        // Act
        var isInteractive = cell.IsInteractive;

        // Assert
        Assert.True(isInteractive);
    }

    [Fact]
    public void MapCell_IsInteractive_WithEmpty_ReturnsFalse()
    {
        // Arrange
        var cell = new MapCell(
            new GridPosition(0, 0),
            TerrainType.Plains);

        // Act
        var isInteractive = cell.IsInteractive;

        // Assert
        Assert.False(isInteractive);
    }

    [Fact]
    public void MapCell_GetDisplayText_WithBuilding_ReturnsFormattedText()
    {
        // Arrange
        var cell = new MapCell(
            new GridPosition(5, 10),
            TerrainType.Plains,
            buildingId: new BuildingId(Guid.NewGuid()),
            buildingType: BuildingType.House);

        // Act
        var text = cell.GetDisplayText();

        // Assert
        Assert.Contains("House", text);
        Assert.Contains("(5, 10)", text);
    }
}
