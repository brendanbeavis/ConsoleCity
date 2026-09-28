using ConsoleCity.Core;
using ConsoleCity.Services;
using ConsoleCity.Simulation;
using ConsoleCity.World;

namespace ConsoleCity.Services.Tests;

public class EnhancedServiceModelTests
{
    [Fact]
    public void RegisterFacility_AddsToModel()
    {
        var model = new EnhancedServiceModel(CreateRandomSource());
        var facility = CreateTestFacility();

        model.RegisterFacility(facility);

        Assert.Single(model.Facilities);
        Assert.Equal(facility.Id, model.Facilities.First().Id);
    }

    [Fact]
    public void RegisterFacility_DuplicateIdThrows()
    {
        var model = new EnhancedServiceModel(CreateRandomSource());
        var facility = CreateTestFacility("facility-1");

        model.RegisterFacility(facility);

        Assert.Throws<InvalidOperationException>(() => model.RegisterFacility(facility));
    }

    [Fact]
    public void SubmitRequest_WithCoverage_ReturnsSuccess()
    {
        var model = new EnhancedServiceModel(CreateRandomSource());
        var facility = CreateTestFacility("police-1", ServiceType.Police, new GridPosition(0, 0), capacity: 10);
        model.RegisterFacility(facility);

        var demand = new ServiceDemand(EntityId.New(), ServiceType.Police, new GridPosition(1, 1), 2, 0.8d);
        var request = new ServiceRequest(Guid.NewGuid(), demand, new SimulationTime(0));

        var response = model.SubmitRequest(request);

        Assert.True(response.Success);
        Assert.Equal(2, response.Provided);
        Assert.True(response.Coverage.IsCovered);
    }

    [Fact]
    public void SubmitRequest_WithoutCoverage_ReturnsFalse()
    {
        var model = new EnhancedServiceModel(CreateRandomSource());
        var facility = CreateTestFacility("police-1", ServiceType.Police, new GridPosition(0, 0), coverageRadius: 5d);
        model.RegisterFacility(facility);

        // Request is very far away
        var demand = new ServiceDemand(EntityId.New(), ServiceType.Police, new GridPosition(50, 50), 2, 0.8d);
        var request = new ServiceRequest(Guid.NewGuid(), demand, new SimulationTime(0));

        var response = model.SubmitRequest(request);

        Assert.False(response.Success);
        Assert.Equal(0, response.Provided);
    }

    [Fact]
    public void SubmitRequest_ExceedingCapacity_ReturnsPartial()
    {
        var model = new EnhancedServiceModel(CreateRandomSource());
        var facility = CreateTestFacility("hospital-1", ServiceType.Hospital, new GridPosition(0, 0), capacity: 5);
        model.RegisterFacility(facility);

        var demand1 = new ServiceDemand(EntityId.New(), ServiceType.Hospital, new GridPosition(0, 0), 3, 0.8d);
        var request1 = new ServiceRequest(Guid.NewGuid(), demand1, new SimulationTime(0));
        var response1 = model.SubmitRequest(request1);

        var demand2 = new ServiceDemand(EntityId.New(), ServiceType.Hospital, new GridPosition(0, 0), 4, 0.8d);
        var request2 = new ServiceRequest(Guid.NewGuid(), demand2, new SimulationTime(1));
        var response2 = model.SubmitRequest(request2);

        Assert.True(response1.Success);
        Assert.Equal(3, response1.Provided);

        // Second request might be partial or denied due to capacity
        Assert.True(response2.Provided <= 4);
    }

    [Fact]
    public void GetServiceStatistics_ReturnsAggregateMetrics()
    {
        var model = new EnhancedServiceModel(CreateRandomSource());

        var facility1 = CreateTestFacility("school-1", ServiceType.PrimaryEducation, capacity: 100);
        var facility2 = CreateTestFacility("school-2", ServiceType.PrimaryEducation, capacity: 80);
        model.RegisterFacility(facility1);
        model.RegisterFacility(facility2);

        var stats = model.GetServiceStatistics(ServiceType.PrimaryEducation);

        Assert.Equal(2, stats.OperationalFacilities);
        Assert.Equal(180, stats.TotalCapacity); // 100 + 80
    }

    [Fact]
    public void Advance_UpdatesFacilityStates()
    {
        var model = new EnhancedServiceModel(CreateRandomSource());
        var facility = CreateTestFacility("clinic-1", ServiceType.Hospital);
        model.RegisterFacility(facility);

        var snapshot1 = model.Snapshot;
        var snapshot2 = model.Advance(new SimulationTime(1));

        Assert.NotEqual(snapshot1.CapturedAt.Tick, snapshot2.CapturedAt.Tick);
    }

    [Fact]
    public void ComputeEffectiveQuality_ConsidersMultipleFactors()
    {
        var facility = CreateTestFacility(
            baseQuality: 0.8d,
            condition: 0.8d,
            staffAssigned: 5,
            maxStaff: 10,
            fundingLevel: 0.8d
        );

        double quality = facility.ComputeEffectiveQuality();

        // Should be reduced from base quality due to non-perfect conditions
        Assert.True(quality < 0.8d);
        Assert.True(quality > 0d); // But still positive
    }

    [Fact]
    public void GetUtilizationRate_CalculatesCorrectly()
    {
        var facility = CreateTestFacility(capacity: 100) with
        {
            CurrentDemand = 75
        };

        double utilization = facility.GetUtilizationRate();

        Assert.Equal(0.75d, utilization, 2);
    }

    [Fact]
    public void GetFulfillmentRate_CalculatesCorrectly()
    {
        var facility = CreateTestFacility(capacity: 100) with
        {
            CurrentDemand = 80,
            UnservedDemand = 20
        };

        double fulfillment = facility.GetFulfillmentRate();

        Assert.Equal(0.8d, fulfillment, 2);
    }

    // Helper methods
    private IRandomSource CreateRandomSource()
    {
        return new DeterministicRandomSource(42);
    }

    private ServiceFacilityState CreateTestFacility(
        string id = "test-facility",
        ServiceType serviceType = ServiceType.Police,
        GridPosition? location = null,
        int capacity = 10,
        int staffAssigned = 5,
        int maxStaff = 10,
        double baseQuality = 0.8d,
        double coverageRadius = 10d,
        double condition = 1.0d,
        double fundingLevel = 1.0d)
    {
        return new ServiceFacilityState(
            id: id,
            buildingId: BuildingId.New(),
            serviceType: serviceType,
            category: ServiceCategory.PoliceStation,
            location: location ?? new GridPosition(0, 0),
            capacity: capacity,
            staffAssigned: staffAssigned,
            maxStaff: maxStaff,
            baseQuality: baseQuality,
            coverageRadius: coverageRadius,
            isOperational: true,
            lifecycleState: ObjectLifecycleState.Operational,
            condition: condition,
            fundingLevel: fundingLevel,
            currentDemand: 0,
            unservedDemand: 0,
            openedAt: new SimulationTime(0)
        );
    }
}
