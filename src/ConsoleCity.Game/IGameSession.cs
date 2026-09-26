using ConsoleCity.Core;

namespace ConsoleCity.Game;

public interface IGameSession
{
    SimulationTime Time { get; }

    bool IsWorldCreated { get; }

    bool IsRunning { get; }

    SimulationSliceSnapshot? Snapshot { get; }

    void CreateNewWorld(int seed = 42);

    void Start();

    void Pause();

    void Advance(int ticks = 1);

    CityStatistics GetCityStatistics();

    string InspectPerson(string query);

    string InspectHousehold(string query);

    string InspectBuilding(string query);
}
