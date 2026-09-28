using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public interface ITransportJourneyPlanner
{
    TransportJourneyPlanResult RequestJourney(TransportJourneyRequest request, SimulationTime currentTime);
}
