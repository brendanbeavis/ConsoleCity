using ConsoleCity.Core;
using ConsoleCity.Services;

namespace ConsoleCity.Services.Tests;

public class ServicesModelTests
{
    [Fact]
    public void SubmitRequest_ProvidesResponseWhenCoveredAndCapacityAvailable()
    {
        var model = CreateModel();
        var request = new ServiceRequest(Guid.NewGuid(), new ServiceDemand(EntityId.New(), ServiceType.Police, new GridPosition(2, 2), 1, 0.8d), new SimulationTime(0));

        var response = model.SubmitRequest(request);

        Assert.True(response.Success);
        Assert.Equal(1, response.Provided);
        Assert.Equal(ServiceType.Police, response.ServiceType);
        Assert.True(response.Coverage.IsCovered);
    }

    [Fact]
    public void SubmitRequest_ReportsInsufficientCapacityWhenDemandExceedsCapacity()
    {
        var model = CreateModel();
        var request1 = new ServiceRequest(Guid.NewGuid(), new ServiceDemand(EntityId.New(), ServiceType.Hospital, new GridPosition(3, 3), 2, 1d), new SimulationTime(0));
        var request2 = new ServiceRequest(Guid.NewGuid(), new ServiceDemand(EntityId.New(), ServiceType.Hospital, new GridPosition(3, 3), 1, 1d), new SimulationTime(0));

        var first = model.SubmitRequest(request1);
        var second = model.SubmitRequest(request2);

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.True(second.Provided < request2.Demand.Amount);
    }

    [Fact]
    public void Advance_TracksQueuedDemandInSnapshot()
    {
        var model = CreateModel();
        var request = new ServiceRequest(Guid.NewGuid(), new ServiceDemand(EntityId.New(), ServiceType.Fire, new GridPosition(1, 1), 2, 0.5d), new SimulationTime(0));

        model.SubmitRequest(request);
        var snapshot = model.Advance(new SimulationTime(1));

        Assert.Single(snapshot.Requests);
        Assert.Single(snapshot.Responses);
        Assert.Equal(1, snapshot.CapturedAt.Tick);
    }

    [Fact]
    public void Evaluate_ReportsCoverageAndCapacity()
    {
        var model = CreateModel();
        var demand = new ServiceDemand(EntityId.New(), ServiceType.PrimaryEducation, new GridPosition(4, 4), 2, 0.4d);

        var outcome = model.Evaluate(demand, new SimulationTime(0));

        Assert.Equal(2, outcome.Capacity.Available);
        Assert.True(outcome.Coverage.IsCovered);
        Assert.True(outcome.Response.Success);
    }

    private static SimpleServiceModel CreateModel()
    {
        var provider1 = new ServiceProvider("police-1", ServiceType.Police, BuildingId.New(), new GridPosition(0, 0), 3, 4, 0.9d, 10d, true);
        var provider2 = new ServiceProvider("hospital-1", ServiceType.Hospital, BuildingId.New(), new GridPosition(5, 5), 2, 6, 0.8d, 12d, true);
        var provider3 = new ServiceProvider("school-1", ServiceType.PrimaryEducation, BuildingId.New(), new GridPosition(4, 4), 2, 5, 0.95d, 8d, true);
        var provider4 = new ServiceProvider("fire-1", ServiceType.Fire, BuildingId.New(), new GridPosition(1, 1), 2, 3, 0.85d, 10d, true);
        var model = new SimpleServiceModel();
        model.RegisterProvider(provider1);
        model.RegisterProvider(provider2);
        model.RegisterProvider(provider3);
        model.RegisterProvider(provider4);
        return model;
    }
}
