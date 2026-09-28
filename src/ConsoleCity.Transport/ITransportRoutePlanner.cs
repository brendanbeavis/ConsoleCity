namespace ConsoleCity.Transport;

public interface ITransportRoutePlanner
{
    TransportRoutePlanResult PlanRoute(TransportTravelDemand demand);
}
