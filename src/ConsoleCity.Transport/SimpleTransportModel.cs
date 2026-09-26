using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed class SimpleTransportModel : ITransportModel, ITransportRoutePlanner, ITransportJourneyPlanner, ITransportJourneyUpdater
{
    private TransportSnapshot snapshot;

    public TransportSnapshot Snapshot => snapshot;

    public SimpleTransportModel(TransportSnapshot? snapshot = null)
    {
        this.snapshot = snapshot ?? TransportSnapshot.Empty;
    }

    public TransportRoutePlanResult PlanRoute(TransportTravelDemand demand)
    {
        if (snapshot.Network.GetNode(demand.OriginNodeId) is null)
        {
            return new TransportRoutePlanResult(false, null, "Origin node not found.");
        }

        if (snapshot.Network.GetNode(demand.DestinationNodeId) is null)
        {
            return new TransportRoutePlanResult(false, null, "Destination node not found.");
        }

        var route = FindShortestRoute(snapshot.Network, demand, snapshot.Traffic);
        return route is null
            ? new TransportRoutePlanResult(false, null, "No route available.")
            : new TransportRoutePlanResult(true, route, "Route found.");
    }

    public TransportJourneyPlanResult RequestJourney(TransportJourneyRequest request, SimulationTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(request);

        var routeResult = PlanRoute(request.Demand);
        if (!routeResult.Success || routeResult.Route is null)
        {
            return new TransportJourneyPlanResult(false, null, null, routeResult.Message, Array.Empty<TransportEvent>());
        }

        var journey = new TransportJourney(EntityId.New(), request, routeResult.Route, TransportJourneyStatus.Requested, currentTime);
        snapshot = new TransportSnapshot(currentTime, snapshot.Network, snapshot.Traffic, snapshot.Vehicles, snapshot.Journeys.Append(journey).ToList());
        var ev = new TransportEvent("JourneyRequested", new SimulationTick(currentTime.Tick), $"Journey {journey.Id.Value} requested.", 0.1d);
        return new TransportJourneyPlanResult(true, journey, routeResult.Route, "Journey requested.", new[] { ev });
    }

    public TransportAdvanceResult Advance(SimulationTime currentTime)
    {
        var completed = new List<TransportJourney>();
        var events = new List<TransportEvent>();
        var journeys = new List<TransportJourney>();

        foreach (var journey in snapshot.Journeys)
        {
            var updated = journey;
            if (journey.Status == TransportJourneyStatus.Requested)
            {
                updated = journey.Start(currentTime);
                events.Add(new TransportEvent("JourneyStarted", new SimulationTick(currentTime.Tick), $"Journey {journey.Id.Value} started.", 0.1d));
                updated = updated.Advance(TimeSpan.FromHours(1));
            }
            else if (journey.Status == TransportJourneyStatus.InTransit)
            {
                updated = journey.Advance(TimeSpan.FromHours(1));
            }

            if (updated.Status == TransportJourneyStatus.Completed)
            {
                updated = new TransportJourney(updated.Id, updated.Request, updated.Route, TransportJourneyStatus.Completed, updated.RequestedAt, updated.DepartedAt, currentTime, updated.Route.EstimatedTravelTime, updated.VehicleId);
                completed.Add(updated);
                events.Add(new TransportEvent("JourneyCompleted", new SimulationTick(currentTime.Tick), $"Journey {journey.Id.Value} completed.", 0.1d));
            }

            journeys.Add(updated);
        }

        snapshot = new TransportSnapshot(currentTime, snapshot.Network, RecomputeTraffic(snapshot.Network, journeys), snapshot.Vehicles, journeys);
        return new TransportAdvanceResult(snapshot, completed, events);
    }

    private static TransportRoute? FindShortestRoute(TransportNetwork network, TransportTravelDemand demand, TransportTrafficState traffic)
    {
        var start = demand.OriginNodeId;
        var end = demand.DestinationNodeId;
        var frontier = new PriorityQueue<EntityId, double>();
        var costs = new Dictionary<EntityId, double> { [start] = 0d };
        var previousNode = new Dictionary<EntityId, EntityId>();
        var previousLink = new Dictionary<EntityId, EntityId>();
        frontier.Enqueue(start, 0d);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current == end)
            {
                break;
            }

            foreach (var link in network.GetOutgoingLinks(current))
            {
                if (!CanUseLink(demand.PreferredMode, link.Mode))
                {
                    continue;
                }

                var edgeCost = EstimateLinkTravelTime(link, traffic).TotalHours;
                var nextCost = costs[current] + edgeCost;
                if (costs.TryGetValue(link.ToNodeId, out var known) && nextCost >= known)
                {
                    continue;
                }

                costs[link.ToNodeId] = nextCost;
                previousNode[link.ToNodeId] = current;
                previousLink[link.ToNodeId] = link.Id;
                frontier.Enqueue(link.ToNodeId, nextCost);
            }
        }

        if (!previousNode.ContainsKey(end))
        {
            return null;
        }

        var nodeIds = new List<EntityId> { end };
        var linkIds = new List<EntityId>();
        var cursor = end;
        while (cursor != start)
        {
            var previous = previousNode[cursor];
            nodeIds.Add(previous);
            linkIds.Add(previousLink[cursor]);
            cursor = previous;
        }

        nodeIds.Reverse();
        linkIds.Reverse();

        var distance = new Distance(linkIds.Sum(linkId => network.GetLink(linkId)?.Length.Value ?? 0m));
        var travelTime = TimeSpan.FromHours(costs[end]);
        var cost = linkIds.Sum(linkId => EstimateLinkCost(network.GetLink(linkId)));

        return new TransportRoute(EntityId.New(), start, end, nodeIds, linkIds, demand.PreferredMode, distance, travelTime, cost);
    }

    private static bool CanUseLink(TransportMode preferredMode, TransportMode linkMode)
        => preferredMode switch
        {
            TransportMode.Pedestrian => linkMode == TransportMode.Pedestrian,
            TransportMode.Bus or TransportMode.Tram or TransportMode.Metro or TransportMode.Freight => linkMode == preferredMode || linkMode == TransportMode.Road,
            _ => linkMode == preferredMode || linkMode == TransportMode.Road
        };

    private static TimeSpan EstimateLinkTravelTime(TransportLink link, TransportTrafficState traffic)
    {
        var linkTraffic = traffic.GetLinkState(link.Id);
        var speed = Math.Max(1d, link.SpeedKph);
        var baseHours = (double)link.Length.Value / speed;
        var congestionMultiplier = 1d + linkTraffic.Congestion + (1d - link.Condition);
        return TimeSpan.FromHours(baseHours * congestionMultiplier);
    }

    private static double EstimateLinkCost(TransportLink? link)
        => link is null ? 0d : (double)link.Length.Value * 0.1d;

    private static TransportTrafficState RecomputeTraffic(TransportNetwork network, IReadOnlyList<TransportJourney> journeys)
    {
        var linkStates = new List<TransportLinkTrafficState>();
        foreach (var link in network.Links)
        {
            var demand = journeys.Count(journey => journey.Route.LinkIds.Contains(link.Id) && journey.Status != TransportJourneyStatus.Completed);
            var congestion = link.Capacity.Limit <= 0d ? 0d : demand / link.Capacity.Limit;
            linkStates.Add(new TransportLinkTrafficState(link.Id, demand, congestion));
        }

        return new TransportTrafficState(linkStates, journeys);
    }
}
