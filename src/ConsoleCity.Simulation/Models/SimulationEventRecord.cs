using ConsoleCity.Core;

namespace ConsoleCity.Simulation;

public sealed record class SimulationEventRecord
{
    public SimulationEvent Event { get; }

    public bool Processed { get; }

    public SimulationTime ProcessedAt { get; }

    public SimulationEventRecord(SimulationEvent @event, bool processed, SimulationTime processedAt)
    {
        Event = @event;
        Processed = processed;
        ProcessedAt = processedAt;
    }

    public SimulationEventRecord MarkProcessed(SimulationTime processedAt) => new(Event, true, processedAt);
}
