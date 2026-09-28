using ConsoleCity.Core;
using ConsoleCity.Services;

namespace ConsoleCity.Services.Tests;

public class EmergencyResponseAllocatorTests
{
    private readonly SimpleEmergencyResponseAllocator allocator = new();

    [Fact]
    public void FindAvailableUnit_ReturnsClosestUnit()
    {
        var incidentLocation = new GridPosition(10, 10);
        var unit1 = new EmergencyResponseUnit(
            "unit-1", ServiceType.Fire, "station-1",
            new GridPosition(5, 5),
            null,
            EmergencyResponseState.Available,
            null, null, 10
        );
        var unit2 = new EmergencyResponseUnit(
            "unit-2", ServiceType.Fire, "station-2",
            new GridPosition(20, 20),
            null,
            EmergencyResponseState.Available,
            null, null, 10
        );

        var units = new List<EmergencyResponseUnit> { unit1, unit2 };
        var selected = allocator.FindAvailableUnit(ServiceType.Fire, incidentLocation, units);

        Assert.NotNull(selected);
        Assert.Equal("unit-1", selected.Id); // Closer unit
    }

    [Fact]
    public void FindAvailableUnit_SkipsUnavailableUnits()
    {
        var incidentLocation = new GridPosition(10, 10);
        var unit1 = new EmergencyResponseUnit(
            "unit-1", ServiceType.Fire, "station-1",
            new GridPosition(5, 5),
            null,
            EmergencyResponseState.OnScene, // Not available
            null, null, 10
        );
        var unit2 = new EmergencyResponseUnit(
            "unit-2", ServiceType.Fire, "station-2",
            new GridPosition(20, 20),
            null,
            EmergencyResponseState.Available,
            null, null, 10
        );

        var units = new List<EmergencyResponseUnit> { unit1, unit2 };
        var selected = allocator.FindAvailableUnit(ServiceType.Fire, incidentLocation, units);

        Assert.NotNull(selected);
        Assert.Equal("unit-2", selected.Id); // Only available unit
    }

    [Fact]
    public void FindAvailableUnit_SkipsWrongServiceType()
    {
        var incidentLocation = new GridPosition(10, 10);
        var unit1 = new EmergencyResponseUnit(
            "unit-1", ServiceType.Police, "station-1", // Wrong type
            new GridPosition(5, 5),
            null,
            EmergencyResponseState.Available,
            null, null, 10
        );
        var unit2 = new EmergencyResponseUnit(
            "unit-2", ServiceType.Fire, "station-2",
            new GridPosition(20, 20),
            null,
            EmergencyResponseState.Available,
            null, null, 10
        );

        var units = new List<EmergencyResponseUnit> { unit1, unit2 };
        var selected = allocator.FindAvailableUnit(ServiceType.Fire, incidentLocation, units);

        Assert.NotNull(selected);
        Assert.Equal("unit-2", selected.Id);
    }

    [Fact]
    public void FindAvailableUnit_NoUnitsAvailable_ReturnsNull()
    {
        var incidentLocation = new GridPosition(10, 10);
        var units = new List<EmergencyResponseUnit>();

        var selected = allocator.FindAvailableUnit(ServiceType.Fire, incidentLocation, units);

        Assert.Null(selected);
    }

    [Fact]
    public void CalculateDistance_BetweenTwoLocations_ReturnsEuclideanDistance()
    {
        var from = new GridPosition(0, 0);
        var to = new GridPosition(3, 4);

        double distance = allocator.CalculateDistance(from, to);

        Assert.Equal(5.0d, distance, 2); // 3-4-5 triangle
    }

    [Fact]
    public void CalculateDistance_SameLocation_ReturnsZero()
    {
        var location = new GridPosition(5, 5);

        double distance = allocator.CalculateDistance(location, location);

        Assert.Equal(0d, distance, 2);
    }

    [Fact]
    public void CalculateEstimatedResponseTime_FireService_SlightlyFaster()
    {
        var from = new GridPosition(0, 0);
        var to = new GridPosition(5, 0);

        int fireTime = allocator.CalculateEstimatedResponseTime(from, to, ServiceType.Fire);
        int ambulanceTime = allocator.CalculateEstimatedResponseTime(from, to, ServiceType.Ambulance);
        int policeTime = allocator.CalculateEstimatedResponseTime(from, to, ServiceType.Police);

        // Fire should be slightly faster than ambulance
        Assert.True(fireTime < ambulanceTime);
        Assert.True(fireTime <= policeTime);
    }

    [Fact]
    public void CalculateEstimatedResponseTime_IncreaseWithDistance()
    {
        var from = new GridPosition(0, 0);
        var close = new GridPosition(1, 0);
        var far = new GridPosition(10, 0);

        int closeTime = allocator.CalculateEstimatedResponseTime(from, close, ServiceType.Ambulance);
        int farTime = allocator.CalculateEstimatedResponseTime(from, far, ServiceType.Ambulance);

        Assert.True(farTime > closeTime);
    }
}

public class EmergencyResponseTests
{
    [Fact]
    public void EmergencyResponseUnit_InvalidId_Throws()
    {
        Assert.Throws<ArgumentException>(() => new EmergencyResponseUnit(
            "", ServiceType.Fire, "station-1",
            new GridPosition(0, 0), null,
            EmergencyResponseState.Available,
            null, null, 10
        ));
    }

    [Fact]
    public void EmergencyResponseUnit_InvalidFacilityId_Throws()
    {
        Assert.Throws<ArgumentException>(() => new EmergencyResponseUnit(
            "unit-1", ServiceType.Fire, "",
            new GridPosition(0, 0), null,
            EmergencyResponseState.Available,
            null, null, 10
        ));
    }

    [Fact]
    public void EmergencyResponseUnit_NegativeCapacity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmergencyResponseUnit(
            "unit-1", ServiceType.Fire, "station-1",
            new GridPosition(0, 0), null,
            EmergencyResponseState.Available,
            null, null, -1
        ));
    }

    [Fact]
    public void EmergencyResponse_InvalidUrgency_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmergencyResponse(
            "response-1", ServiceType.Fire, new GridPosition(0, 0),
            1.5d, // Invalid urgency > 1
            new SimulationTime(0)
        ));
    }

    [Fact]
    public void EmergencyResponse_NegativeResponseTime_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EmergencyResponse(
            "response-1", ServiceType.Fire, new GridPosition(0, 0),
            0.8d, new SimulationTime(0),
            estimatedResponseTimeMinutes: -5
        ));
    }

    [Fact]
    public void EmergencyResponse_ValidConstruction_Succeeds()
    {
        var response = new EmergencyResponse(
            "response-1", ServiceType.Fire, new GridPosition(5, 5),
            0.9d, new SimulationTime(0),
            estimatedResponseTimeMinutes: 10,
            description: "Structure fire at location"
        );

        Assert.Equal("response-1", response.Id);
        Assert.Equal(ServiceType.Fire, response.ServiceType);
        Assert.Equal(0.9d, response.Urgency);
        Assert.Equal(10, response.EstimatedResponseTimeMinutes);
    }
}
