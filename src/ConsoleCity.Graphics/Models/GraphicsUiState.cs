using ConsoleCity.World;

namespace ConsoleCity.Graphics.Models;

public sealed record GraphicsUiState(
    SimulationSpeed Speed,
    MapObject? SelectedObject,
    MapObject? HoveredObject,
    bool IsBuildMode,
    BuildingType SelectedBuildingType,
    IReadOnlyList<string> Notifications)
{
    public static GraphicsUiState Default { get; } = new(
        SimulationSpeed.OneX,
        null,
        null,
        false,
        BuildingType.House,
        []);

    public GraphicsUiState PushNotification(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return this;
        }

        var notifications = Notifications.Concat([message.Trim()]).TakeLast(6).ToList();
        return this with { Notifications = notifications };
    }
}
