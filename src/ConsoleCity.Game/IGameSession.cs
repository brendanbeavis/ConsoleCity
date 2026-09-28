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

    void Save(string name, string? baseDirectory = null);

    void Load(string name, string? baseDirectory = null);

    IReadOnlyList<TechnologyDefinition> GetAvailableTechnologies();

    bool ResearchTechnology(string technologyId);

    IReadOnlyList<GameModifierDefinition> GetAvailableModifiers();

    bool PurchaseModifier(string modifierId);

    IReadOnlyList<GamePolicyDefinition> GetAvailablePolicies();

    bool SetPolicy(string policyId, decimal intensity);

    IReadOnlyList<GameEventRecord> GetRecentEvents(int maxCount = 20);

    IReadOnlyList<string> ListSaves(string? baseDirectory = null);
}
