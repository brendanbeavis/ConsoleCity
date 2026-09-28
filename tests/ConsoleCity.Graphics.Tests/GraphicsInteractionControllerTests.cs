using System.Numerics;
using ConsoleCity.Game;
using ConsoleCity.Graphics.Controllers;
using ConsoleCity.Graphics.Input;
using ConsoleCity.Graphics.Models;

namespace ConsoleCity.Graphics.Tests;

public sealed class GraphicsInteractionControllerTests
{
    [Fact]
    public void Update_AdvancesSimulationBasedOnConfiguredSpeed()
    {
        var session = new GameSession();
        var controller = new GraphicsInteractionController(session);
        var viewport = new PixelRect(0, 0, 640, 360);
        controller.EnsureWorld(viewport: viewport);

        controller.HandleInput(CreateInput(togglePausePressed: true), viewport);
        controller.HandleInput(CreateInput(fasterPressed: true), viewport);

        var before = session.Time;
        controller.Update(1.1f);

        Assert.True(session.Time.Tick >= before.Tick + 2);
    }

    [Fact]
    public void HandleInput_LeftClickSelectsHoveredMapObject()
    {
        var session = new GameSession();
        var controller = new GraphicsInteractionController(session);
        var viewport = new PixelRect(0, 0, 640, 360);
        controller.EnsureWorld(viewport: viewport);

        var buildingScreen = controller.Camera.WorldToScreen(new ConsoleCity.Core.GridPosition(1, 1), viewport) + new Vector2(controller.Camera.CellSize / 2f, controller.Camera.CellSize / 2f);

        controller.HandleInput(CreateInput(mousePosition: buildingScreen, selectPressed: true), viewport);

        Assert.NotNull(controller.UiState.SelectedObject);
        Assert.Equal(ConsoleCity.World.ObjectType.Building, controller.UiState.SelectedObject!.Type);
    }

    [Fact]
    public void HandleInput_ConfirmInBuildModeReportsValidationErrorOnOccupiedPlot()
    {
        var session = new GameSession();
        var controller = new GraphicsInteractionController(session);
        var viewport = new PixelRect(0, 0, 640, 360);
        controller.EnsureWorld(viewport: viewport);

        var buildingScreen = controller.Camera.WorldToScreen(new ConsoleCity.Core.GridPosition(0, 0), viewport) + new Vector2(controller.Camera.CellSize / 2f, controller.Camera.CellSize / 2f);
        controller.HandleInput(CreateInput(toggleBuildModePressed: true), viewport);
        controller.HandleInput(CreateInput(mousePosition: buildingScreen, confirmPressed: true), viewport);

        Assert.Contains(controller.UiState.Notifications, message => message.Contains("occupied", StringComparison.OrdinalIgnoreCase));
    }

    private static GraphicsInputState CreateInput(
        Vector2? mousePosition = null,
        Vector2? mouseDelta = null,
        float mouseWheelDelta = 0f,
        bool isPanning = false,
        bool selectPressed = false,
        bool confirmPressed = false,
        bool cancelPressed = false,
        bool togglePausePressed = false,
        bool stepPressed = false,
        bool fasterPressed = false,
        bool slowerPressed = false,
        bool toggleBuildModePressed = false,
        bool nextBuildTypePressed = false,
        bool previousBuildTypePressed = false)
        => new(
            mousePosition ?? Vector2.Zero,
            mouseDelta ?? Vector2.Zero,
            mouseWheelDelta,
            isPanning,
            selectPressed,
            confirmPressed,
            cancelPressed,
            togglePausePressed,
            stepPressed,
            fasterPressed,
            slowerPressed,
            toggleBuildModePressed,
            nextBuildTypePressed,
            previousBuildTypePressed);
}
