using ConsoleCity.Core;

namespace ConsoleCity.Simulation;

public interface ISimulationEngine
{
    SimulationMode Mode { get; }

    SimulationState State { get; }

    SimulationTime CurrentTime { get; }

    void Step(int ticks = 1);

    void Pause();

    void Resume();

    void SetMode(SimulationMode mode);

    void EnqueueEvent(SimulationEvent simulationEvent);

    IReadOnlyList<SimulationEventRecord> GetEventHistory();
}
