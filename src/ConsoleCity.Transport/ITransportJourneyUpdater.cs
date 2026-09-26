using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public interface ITransportJourneyUpdater
{
    TransportAdvanceResult Advance(SimulationTime currentTime);
}
