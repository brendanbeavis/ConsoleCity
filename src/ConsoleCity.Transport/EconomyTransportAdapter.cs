using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.World;

namespace ConsoleCity.Transport;

public sealed class EconomyTransportAdapter : IEconomyTransportBridge
{
    private readonly SimpleTransportModel transportModel;
    private readonly Dictionary<EntityId, GoodsTransportRequest> requestsByJourneyId = new();

    public EconomyTransportAdapter(SimpleTransportModel transportModel)
    {
        ArgumentNullException.ThrowIfNull(transportModel);
        this.transportModel = transportModel;
    }

    public void RequestTransport(GoodsTransportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var originNode = request.From is not null
            ? FindOrCreateTransportNode(request.From.Value)
            : null;
        var destinationNode = FindOrCreateTransportNode(request.To ?? new GridPosition(0, 0));

        if (originNode is null || destinationNode is null)
        {
            return;
        }

        var demand = new TransportTravelDemand(
            EntityId.New(),
            originNode.Id,
            destinationNode.Id,
            TransportMode.Freight,
            0.5d,
            0,
            request.Quantity.Value);
        var journeyRequest = new TransportJourneyRequest(EntityId.New(), demand, new SimulationTime(0));
        var result = transportModel.RequestJourney(journeyRequest, new SimulationTime(0));

        if (result.Journey is not null)
        {
            requestsByJourneyId[result.Journey.Id] = request;
        }
    }

    public IReadOnlyList<Economy.DeliveredGoods> GetCompletedDeliveries()
    {
        var completed = new List<Economy.DeliveredGoods>();
        var snapshot = transportModel.Snapshot;

        foreach (var journey in snapshot.Journeys.Where(j => j.Status == TransportJourneyStatus.Completed))
        {
            if (requestsByJourneyId.TryGetValue(journey.Id, out var request))
            {
                completed.Add(new Economy.DeliveredGoods(journey.Id, request.ResourceId, request.Quantity, request.To));
                requestsByJourneyId.Remove(journey.Id);
            }
        }

        return completed;
    }

    private TransportNode? FindOrCreateTransportNode(GridPosition position)
    {
        var network = transportModel.Snapshot.Network;
        var existing = network.Nodes.FirstOrDefault(n => n.Position == position);
        if (existing is not null)
        {
            return existing;
        }

        // For now, return null if node doesn't exist. In a real implementation,
        // we might create a supply/demand node or use a default market location.
        return null;
    }
}
