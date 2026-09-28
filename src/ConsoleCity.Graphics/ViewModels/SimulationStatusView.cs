using ConsoleCity.Core;

namespace ConsoleCity.Graphics.ViewModels;

public sealed record class SimulationStatusView
{
    public string Title { get; }

    public string CityName { get; }

    public GameDateTime DateTime { get; }

    public bool IsPaused { get; }

    public string SpeedLabel { get; }

    public SimulationStatusView(string title, string cityName, GameDateTime dateTime, bool isPaused, string speedLabel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(speedLabel);

        Title = title;
        CityName = cityName;
        DateTime = dateTime;
        IsPaused = isPaused;
        SpeedLabel = speedLabel;
    }
}
