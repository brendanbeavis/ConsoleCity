using ConsoleCity.Core;
using ConsoleCity.Transport;

namespace ConsoleCity.Transport.Tests;

public class TransportModelTests
{
    [Fact]
    public void PlanRoute_ChoosesShortestAvailablePath()
    {
        var network = CreateNetwork(out var origin, out var mid, out var destination, out var slowLink, out var fastLink1, out var fastLink2);
        var model = new SimpleTransportModel(new TransportSnapshot(new SimulationTime(0), network, TransportTrafficState.Empty, [], []));
        var demand = new TransportTravelDemand(EntityId.New(), origin, destination, TransportMode.Road, 0.5d, 1, 0m);

        var result = model.PlanRoute(demand);

        Assert.True(result.Success);
        Assert.NotNull(result.Route);
        Assert.Equal([origin, mid, destination], result.Route!.NodeIds);
        Assert.Equal([fastLink1, fastLink2], result.Route.LinkIds);
        Assert.Equal(TimeSpan.FromHours(2), result.Route.EstimatedTravelTime);
    }

    [Fact]
    public void RequestJourney_CreatesRequestedJourney()
    {
        var network = CreateNetwork(out var origin, out _, out var destination, out _, out var link1, out var link2);
        var model = new SimpleTransportModel(new TransportSnapshot(new SimulationTime(0), network, TransportTrafficState.Empty, [], []));
        var request = new TransportJourneyRequest(EntityId.New(), new TransportTravelDemand(EntityId.New(), origin, destination, TransportMode.Road, 0.5d, 1, 0m), new SimulationTime(0));

        var result = model.RequestJourney(request, new SimulationTime(0));

        Assert.True(result.Success);
        Assert.NotNull(result.Journey);
        Assert.Equal(TransportJourneyStatus.Requested, result.Journey!.Status);
        Assert.Equal(request.Id, result.Journey.Request.Id);
        Assert.Contains(result.Events, e => e.Type == "JourneyRequested");
    }

    [Fact]
    public void Advance_MovesJourneyToCompletion()
    {
        var network = CreateNetwork(out var origin, out _, out var destination, out _, out _, out _);
        var model = new SimpleTransportModel(new TransportSnapshot(new SimulationTime(0), network, TransportTrafficState.Empty, [], []));
        var request = new TransportJourneyRequest(EntityId.New(), new TransportTravelDemand(EntityId.New(), origin, destination, TransportMode.Road, 0.5d, 1, 0m), new SimulationTime(0));

        model.RequestJourney(request, new SimulationTime(0));
        var first = model.Advance(new SimulationTime(1));
        var second = model.Advance(new SimulationTime(2));

        Assert.Empty(first.CompletedJourneys);
        Assert.Single(second.CompletedJourneys);
        Assert.Equal(TransportJourneyStatus.Completed, second.CompletedJourneys[0].Status);
        Assert.Contains(second.Events, e => e.Type == "JourneyCompleted");
    }

    [Fact]
    public void TransportNetwork_ExposesOutgoingLinks()
    {
        var network = CreateNetwork(out var origin, out _, out _, out var slowLink, out var link1, out _);

        var outgoing = network.GetOutgoingLinks(origin).ToList();

        Assert.Equal(2, outgoing.Count);
        Assert.True(outgoing.Any(link => link.Id == slowLink));
        Assert.True(outgoing.Any(link => link.Id == link1));
    }

    private static TransportNetwork CreateNetwork(out EntityId origin, out EntityId mid, out EntityId destination, out EntityId slowLink, out EntityId fastLink1, out EntityId fastLink2)
    {
        origin = EntityId.New();
        mid = EntityId.New();
        destination = EntityId.New();
        slowLink = EntityId.New();
        fastLink1 = EntityId.New();
        fastLink2 = EntityId.New();

        var nodes = new[]
        {
            new TransportNode(origin, new GridPosition(0, 0), TransportNodeType.Intersection),
            new TransportNode(mid, new GridPosition(1, 0), TransportNodeType.Intersection),
            new TransportNode(destination, new GridPosition(2, 0), TransportNodeType.Intersection)
        };

        var intersections = new[]
        {
            new TransportIntersection(origin, new GridPosition(0, 0), [slowLink, fastLink1]),
            new TransportIntersection(mid, new GridPosition(1, 0), [fastLink1, fastLink2]),
            new TransportIntersection(destination, new GridPosition(2, 0), [slowLink, fastLink2])
        };

        var links = new[]
        {
            new TransportLink(slowLink, origin, destination, TransportMode.Road, new Distance(30m), 10d, new TransportCapacity(100d, 0d), 1d),
            new TransportLink(fastLink1, origin, mid, TransportMode.Road, new Distance(10m), 10d, new TransportCapacity(100d, 0d), 1d),
            new TransportLink(fastLink2, mid, destination, TransportMode.Road, new Distance(10m), 10d, new TransportCapacity(100d, 0d), 1d)
        };

        return new TransportNetwork(nodes, intersections, links);
    }
}
