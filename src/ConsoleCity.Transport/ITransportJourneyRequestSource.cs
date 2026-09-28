using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public interface ITransportJourneyRequestSource
{
    IReadOnlyList<TransportJourneyRequest> GetJourneyRequests(SimulationTime currentTime);
}
