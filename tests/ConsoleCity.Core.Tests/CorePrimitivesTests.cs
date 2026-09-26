using ConsoleCity.Core;

namespace ConsoleCity.Core.Tests;

public class CorePrimitivesTests
{
    [Fact]
    public void EntityId_New_ProducesNonEmptyGuid()
    {
        var id = EntityId.New();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void EntityId_EmptyGuid_Throws()
    {
        Assert.Throws<ArgumentException>(() => new EntityId(Guid.Empty));
    }

    [Fact]
    public void SimulationTick_NegativeValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationTick(-1));
    }

    [Fact]
    public void SimulationTime_Advance_IncrementsTick()
    {
        var time = new SimulationTime(10);

        var updated = time.Advance(5);

        Assert.Equal(15, updated.Tick);
    }

    [Fact]
    public void SimulationTime_NegativeTick_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationTime(-1));
    }

    [Fact]
    public void GameDateTime_RoundTripsToSimulationTime()
    {
        var dateTime = new GameDateTime(2, 3, 4, 5);

        var roundTripped = GameDateTime.FromSimulationTime(dateTime.ToSimulationTime());

        Assert.Equal(dateTime, roundTripped);
    }

    [Fact]
    public void GridPosition_DistanceTo_UsesEuclideanDistance()
    {
        var origin = new GridPosition(0, 0);
        var other = new GridPosition(3, 4);

        var distance = origin.DistanceTo(other);

        Assert.Equal(5m, distance.Value);
    }

    [Fact]
    public void Coordinate2D_DistanceTo_UsesEuclideanDistance()
    {
        var origin = new Coordinate2D(0, 0);
        var other = new Coordinate2D(6, 8);

        var distance = origin.DistanceTo(other);

        Assert.Equal(10m, distance.Value);
    }

    [Fact]
    public void Coordinate2D_NonFiniteValues_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinate2D(double.NaN, 0));
    }

    [Fact]
    public void Distance_NegativeValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Distance(-1m));
    }

    [Fact]
    public void Quantity_NegativeValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Quantity(-1m));
    }

    [Fact]
    public void Quantity_Addition_ReturnsExpectedValue()
    {
        var total = new Quantity(2.5m) + new Quantity(1.25m);

        Assert.Equal(new Quantity(3.75m), total);
    }

    [Fact]
    public void Money_Arithmetic_ReturnsExpectedValue()
    {
        var balance = new Money(10m) - new Money(2.5m) + new Money(1m);

        Assert.Equal(new Money(8.5m), balance);
    }

    [Fact]
    public void ResourceId_TrimsAndRejectsEmpty()
    {
        var id = new ResourceId("  steel  ");

        Assert.Equal("steel", id.Value);
        Assert.Throws<ArgumentException>(() => new ResourceId(" "));
    }

    [Fact]
    public void SimulationEvent_RejectsEmptyType()
    {
        Assert.Throws<ArgumentException>(() => new SimulationEvent(EntityId.New(), "", new SimulationTick(0)));
    }

    [Fact]
    public void SimulationEvent_RejectsNonFiniteSeverity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationEvent(EntityId.New(), "test", new SimulationTick(0), severity: double.NaN));
    }
}
