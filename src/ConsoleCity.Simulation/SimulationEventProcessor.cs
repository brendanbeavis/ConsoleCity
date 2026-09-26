using ConsoleCity.Core;

namespace ConsoleCity.Simulation;

public sealed class SimulationEventProcessor
{
    private readonly Queue<SimulationEventRecord> pending = new();
    private readonly List<SimulationEventRecord> history = new();

    public IReadOnlyList<SimulationEventRecord> History => history;

    public void Enqueue(SimulationEvent simulationEvent)
    {
        ArgumentNullException.ThrowIfNull(simulationEvent);
        pending.Enqueue(new SimulationEventRecord(simulationEvent, false, new SimulationTime(0)));
    }

    public IReadOnlyList<SimulationEventRecord> Drain(SimulationTime processedAt)
    {
        var processed = new List<SimulationEventRecord>();
        while (pending.Count > 0)
        {
            var record = pending.Dequeue().MarkProcessed(processedAt);
            history.Add(record);
            processed.Add(record);
        }

        return processed;
    }
}
