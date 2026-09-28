using ConsoleCity.Game;
using Xunit;

namespace ConsoleCity.Console.Tests;

public class MapConsoleCommandTests
{
    [Fact]
    public void MapCommand_WithoutArgs_DisplaysMapWithLegend()
    {
        // Arrange
        var session = new GameSession();
        session.CreateNewWorld(seed: 42);

        // Act
        var output = Program.ProcessCommand(session, "map");

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("WORLD MAP", output);
        Assert.Contains("LEGEND", output);
    }

    [Fact]
    public void MapCommand_WithCoords_DisplaysMapWithCoordinates()
    {
        // Arrange
        var session = new GameSession();
        session.CreateNewWorld(seed: 42);

        // Act
        var output = Program.ProcessCommand(session, "map coords");

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("WORLD MAP", output);
    }

    [Fact]
    public void MapCommand_WithLegend_DisplaysMapWithLegend()
    {
        // Arrange
        var session = new GameSession();
        session.CreateNewWorld(seed: 42);

        // Act
        var output = Program.ProcessCommand(session, "map legend");

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("LEGEND", output);
        Assert.Contains("House", output);
    }

    [Fact]
    public void MapCommand_WithBuildings_DisplaysBuildingList()
    {
        // Arrange
        var session = new GameSession();
        session.CreateNewWorld(seed: 42);

        // Act
        var output = Program.ProcessCommand(session, "map buildings");

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("BUILDINGS", output);
    }

    [Fact]
    public void MapCommand_WithPlots_DisplaysPlotList()
    {
        // Arrange
        var session = new GameSession();
        session.CreateNewWorld(seed: 42);

        // Act
        var output = Program.ProcessCommand(session, "map plots");

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("PLOTS", output);
    }

    [Fact]
    public void MapCommand_WithCell_DisplaysCellDetails()
    {
        // Arrange
        var session = new GameSession();
        session.CreateNewWorld(seed: 42);

        // Act
        var output = Program.ProcessCommand(session, "map cell 5 5");

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("CELL DETAIL", output);
    }

    [Fact]
    public void MapCommand_WithRegion_DisplaysRegion()
    {
        // Arrange
        var session = new GameSession();
        session.CreateNewWorld(seed: 42);

        // Act
        var output = Program.ProcessCommand(session, "map region 0 0 10 10");

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("MAP REGION", output);
    }

    [Fact]
    public void MapCommand_WithoutWorld_ReturnsError()
    {
        // Arrange
        var session = new GameSession();

        // Act
        var output = Program.ProcessCommand(session, "map");

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("map available", output.ToLower());
    }

    [Fact]
    public void HelpCommand_IncludesMapCommands()
    {
        // Arrange
        var session = new GameSession();
        session.CreateNewWorld(seed: 42);

        // Act
        var output = Program.ProcessCommand(session, "help");

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("map", output.ToLower());
        Assert.Contains("map cell", output.ToLower());
        Assert.Contains("map buildings", output.ToLower());
        Assert.Contains("map plots", output.ToLower());
    }

    [Fact]
    public void MapCommand_DisplaysConsistentStateBeforeAndAfterAdvance()
    {
        // Arrange
        var session = new GameSession();
        session.CreateNewWorld(seed: 42);

        var mapBefore = Program.ProcessCommand(session, "map");

        // Act: Advance simulation
        session.Advance(5);

        // Assert
        var mapAfter = Program.ProcessCommand(session, "map");

        Assert.NotEmpty(mapBefore);
        Assert.NotEmpty(mapAfter);
        // Both should have the same structure (map might have changed, but should still be valid)
        Assert.Contains("WORLD MAP", mapBefore);
        Assert.Contains("WORLD MAP", mapAfter);
    }
}
