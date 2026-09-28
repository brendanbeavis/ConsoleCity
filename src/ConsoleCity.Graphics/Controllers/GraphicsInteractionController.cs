using System.Numerics;
using ConsoleCity.Core;
using ConsoleCity.Game;
using ConsoleCity.Graphics.Input;
using ConsoleCity.Graphics.Models;
using ConsoleCity.Graphics.Rendering;
using ConsoleCity.Graphics.ViewModels;
using ConsoleCity.World;

namespace ConsoleCity.Graphics.Controllers;

public sealed class GraphicsInteractionController
{
    private static readonly BuildingType[] BuildTypes = Enum.GetValues<BuildingType>();
    private readonly MainScreenViewBuilder viewBuilder = new();
    private float simulationAccumulatorSeconds;

    public GameSession Session { get; }

    public MapCamera Camera { get; }

    public GraphicsUiState UiState { get; private set; }

    public GraphicsInteractionController(GameSession session, MapCamera? camera = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        Session = session;
        Camera = camera ?? new MapCamera();
        UiState = GraphicsUiState.Default;

        SyncCameraToWorld(new PixelRect(0, 0, 640, 360));
    }

    public void EnsureWorld(int seed = 42, PixelRect? viewport = null)
    {
        if (!Session.IsWorldCreated)
        {
            Session.CreateNewWorld(seed);
        }

        SyncCameraToWorld(viewport ?? new PixelRect(0, 0, 640, 360));
    }

    public void Update(float deltaSeconds)
    {
        if (deltaSeconds <= 0f || !Session.IsWorldCreated || !Session.IsRunning)
        {
            return;
        }

        simulationAccumulatorSeconds += deltaSeconds * UiState.Speed.GetTicksPerSecond();
        var ticksToAdvance = (int)simulationAccumulatorSeconds;
        if (ticksToAdvance <= 0)
        {
            return;
        }

        simulationAccumulatorSeconds -= ticksToAdvance;
        Session.Advance(ticksToAdvance);
    }

    public MainScreenView BuildView()
        => viewBuilder.Build(
            Session,
            UiState.Speed.ToDisplayLabel(),
            UiState.SelectedObject,
            UiState.HoveredObject,
            UiState.IsBuildMode,
            UiState.SelectedBuildingType,
            UiState.Notifications);

    public void HandleInput(GraphicsInputState input, PixelRect mapViewport)
    {
        ArgumentNullException.ThrowIfNull(input);

        EnsureWorld(viewport: mapViewport);
        SyncCameraToWorld(mapViewport);

        if (input.TogglePausePressed)
        {
            TogglePause();
        }

        if (input.StepPressed)
        {
            Session.Advance(1);
            UiState = UiState.PushNotification($"Advanced to {Session.Time.ToGameDateTime()}.");
        }

        if (input.FasterPressed)
        {
            var newSpeed = UiState.Speed.Faster();
            UiState = (UiState with { Speed = newSpeed })
                .PushNotification($"Simulation speed {newSpeed.ToDisplayLabel()}.");
        }

        if (input.SlowerPressed)
        {
            var newSpeed = UiState.Speed.Slower();
            UiState = (UiState with { Speed = newSpeed })
                .PushNotification($"Simulation speed {newSpeed.ToDisplayLabel()}.");
        }

        if (input.ToggleBuildModePressed)
        {
            var isBuildMode = !UiState.IsBuildMode;
            UiState = (UiState with { IsBuildMode = isBuildMode })
                .PushNotification(isBuildMode ? "Build mode enabled." : "Build mode disabled.");
        }

        if (input.NextBuildTypePressed)
        {
            var nextBuildType = GetNextBuildType(UiState.SelectedBuildingType, +1);
            UiState = (UiState with { SelectedBuildingType = nextBuildType })
                .PushNotification($"Build type {nextBuildType} selected.");
        }

        if (input.PreviousBuildTypePressed)
        {
            var previousBuildType = GetNextBuildType(UiState.SelectedBuildingType, -1);
            UiState = (UiState with { SelectedBuildingType = previousBuildType })
                .PushNotification($"Build type {previousBuildType} selected.");
        }

        if (mapViewport.Contains(input.MousePosition))
        {
            if (input.IsPanning)
            {
                Camera.PanPixels(input.MouseDelta, mapViewport);
            }

            if (input.MouseWheelDelta != 0f)
            {
                Camera.ZoomAt(input.MouseWheelDelta, input.MousePosition, mapViewport);
            }

            var hoveredWorld = Camera.ScreenToWorld(input.MousePosition, mapViewport);
            var hoveredObject = Session.GetMapInspector()?.GetObjectAt(hoveredWorld);
            UiState = UiState with { HoveredObject = hoveredObject };

            if (input.SelectPressed)
            {
                UiState = (UiState with { SelectedObject = hoveredObject })
                    .PushNotification(hoveredObject is null ? "Selection cleared." : $"Selected {hoveredObject.Type} at {hoveredObject.Position}.");
            }

            if (input.ConfirmPressed)
            {
                HandleConfirm(hoveredWorld);
            }
        }

        if (input.CancelPressed && UiState.IsBuildMode)
        {
            UiState = (UiState with { IsBuildMode = false })
                .PushNotification("Build mode disabled.");
        }
    }

    private void HandleConfirm(GridPosition worldPosition)
    {
        if (!UiState.IsBuildMode)
        {
            UiState = UiState with { SelectedObject = UiState.HoveredObject };
            return;
        }

        var result = Session.RequestConstruction(UiState.SelectedBuildingType, worldPosition);
        if (!result.Request.IsValid)
        {
            var message = result.Request.ValidationErrors.FirstOrDefault() ?? "Construction request failed.";
            UiState = UiState.PushNotification(message);
            return;
        }

        if (result.Construction is null)
        {
            UiState = UiState.PushNotification("Construction request was rejected.");
            return;
        }

        UiState = UiState.PushNotification($"Started {UiState.SelectedBuildingType} at {worldPosition}.");
    }

    private void TogglePause()
    {
        if (Session.IsRunning)
        {
            Session.Pause();
            UiState = UiState.PushNotification("Simulation paused.");
            return;
        }

        Session.Start();
        UiState = UiState.PushNotification("Simulation running.");
    }

    private void SyncCameraToWorld(PixelRect viewport)
    {
        var mapView = Session.GetMapView();
        if (mapView is null)
        {
            return;
        }

        if (Camera.MinimumBounds != mapView.Bounds.Min || Camera.MaximumBounds != mapView.Bounds.Max)
        {
            Camera.SetBounds(mapView.Bounds.Min, mapView.Bounds.Max, viewport);
            return;
        }

        Camera.PanPixels(Vector2.Zero, viewport);
    }

    private static BuildingType GetNextBuildType(BuildingType current, int delta)
    {
        var index = Array.IndexOf(BuildTypes, current);
        if (index < 0)
        {
            return BuildTypes[0];
        }

        var nextIndex = (index + delta + BuildTypes.Length) % BuildTypes.Length;
        return BuildTypes[nextIndex];
    }
}
