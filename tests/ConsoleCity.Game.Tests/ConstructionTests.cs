using ConsoleCity.Core;
using ConsoleCity.Game.Construction;
using ConsoleCity.World;

namespace ConsoleCity.Game.Tests;

public class ConstructionCostsTests
{
    [Fact]
    public void GetCost_House_Returns5000()
    {
        var cost = ConstructionCosts.GetCost(BuildingType.House);
        Assert.Equal(new Money(5000), cost);
    }

    [Fact]
    public void GetCost_Factory_Returns50000()
    {
        var cost = ConstructionCosts.GetCost(BuildingType.Factory);
        Assert.Equal(new Money(50000), cost);
    }

    [Fact]
    public void GetDurationTicks_House_Returns10()
    {
        var duration = ConstructionCosts.GetDurationTicks(BuildingType.House);
        Assert.Equal(10, duration);
    }

    [Fact]
    public void GetDurationTicks_Factory_Returns60()
    {
        var duration = ConstructionCosts.GetDurationTicks(BuildingType.Factory);
        Assert.Equal(60, duration);
    }
}

public class BuildingConstructionStateTests
{
    [Fact]
    public void Constructor_ValidInput_CreatesRequestedConstruction()
    {
        var now = new SimulationTime(100);
        var construction = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            10,
            now);

        Assert.Equal(BuildingConstructionState.Requested, construction.State);
        Assert.Equal(0, construction.TicksElapsed);
        Assert.Null(construction.StartedAt);
    }

    [Fact]
    public void Validate_FromRequested_TransitionsToValidated()
    {
        var construction = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            10,
            new SimulationTime(100));

        var validated = construction.Validate();
        Assert.Equal(BuildingConstructionState.Validated, validated.State);
    }

    [Fact]
    public void Validate_NotFromRequested_Throws()
    {
        var construction = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            10,
            new SimulationTime(100));

        var validated = construction.Validate();
        Assert.Throws<InvalidOperationException>(() => validated.Validate());
    }

    [Fact]
    public void StateTransitions_CompleteLifecycle()
    {
        var id = ConstructionId.New();
        var now = new SimulationTime(100);

        var requested = new BuildingConstruction(
            id,
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            10,
            now);

        var validated = requested.Validate();
        var funded = validated.Fund();
        var started = funded.StartConstruction(now);

        Assert.Equal(BuildingConstructionState.UnderConstruction, started.State);
        Assert.NotNull(started.StartedAt);

        // Advance through construction
        var progressed = started;
        for (int i = 0; i < 10; i++)
        {
            progressed = progressed.AdvanceTick();
        }

        Assert.Equal(BuildingConstructionState.Completed, progressed.State);
        Assert.NotNull(progressed.CompletedAt);
    }

    [Fact]
    public void Cancel_InValidStates_Succeeds()
    {
        var construction = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            10,
            new SimulationTime(100));

        var cancelled = construction.Cancel("Test cancellation");
        Assert.Equal(BuildingConstructionState.Cancelled, cancelled.State);
        Assert.Equal("Test cancellation", cancelled.CancellationReason);
    }

    [Fact]
    public void Cancel_WhenCompleted_Throws()
    {
        var construction = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            1,
            new SimulationTime(100));

        var validated = construction.Validate();
        var funded = validated.Fund();
        var started = funded.StartConstruction(new SimulationTime(100));
        var completed = started.AdvanceTick();

        Assert.Throws<InvalidOperationException>(() => completed.Cancel("Too late"));
    }

    [Fact]
    public void GetProgress_HalfwayThroughConstruction_Returns50()
    {
        var construction = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            10,
            new SimulationTime(100));

        var validated = construction.Validate();
        var funded = validated.Fund();
        var started = funded.StartConstruction(new SimulationTime(100));

        // Advance halfway
        var halfway = started;
        for (int i = 0; i < 5; i++)
        {
            halfway = halfway.AdvanceTick();
        }

        Assert.True(halfway.GetProgress() >= 50.0 && halfway.GetProgress() <= 51.0);
    }
}

public class ConstructionRequestTests
{
    [Fact]
    public void Constructor_CreatesValidRequest()
    {
        var location = new GridPosition(10, 20);
        var now = new SimulationTime(100);

        var request = new ConstructionRequest(BuildingType.House, location, now);

        Assert.Equal(BuildingType.House, request.BuildingType);
        Assert.Equal(location, request.Location);
        Assert.True(request.IsValid);
        Assert.Empty(request.ValidationErrors);
    }

    [Fact]
    public void WithValidationError_AddsError()
    {
        var request = new ConstructionRequest(BuildingType.House, new GridPosition(10, 20), new SimulationTime(100));
        var withError = request.WithValidationError("Plot occupied");

        Assert.False(withError.IsValid);
        Assert.Single(withError.ValidationErrors);
        Assert.Contains("Plot occupied", withError.ValidationErrors);
    }

    [Fact]
    public void WithValidationErrors_AddsMultipleErrors()
    {
        var request = new ConstructionRequest(BuildingType.House, new GridPosition(10, 20), new SimulationTime(100));
        var withErrors = request.WithValidationErrors("Plot occupied", "Insufficient funds");

        Assert.False(withErrors.IsValid);
        Assert.Equal(2, withErrors.ValidationErrors.Count);
    }

    [Fact]
    public void ClearValidationErrors_MakesRequestValid()
    {
        var request = new ConstructionRequest(BuildingType.House, new GridPosition(10, 20), new SimulationTime(100));
        var withError = request.WithValidationError("Some error");
        var cleared = withError.ClearValidationErrors();

        Assert.True(cleared.IsValid);
        Assert.Empty(cleared.ValidationErrors);
    }
}

public class ConstructionManagerTests
{
    [Fact]
    public void RegisterConstruction_AddsToManager()
    {
        var manager = new ConstructionManager();
        var construction = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            10,
            new SimulationTime(100));

        manager.RegisterConstruction(construction);

        Assert.Single(manager.ActiveProjects);
        Assert.Contains(construction.Id, manager.ActiveProjects.Keys);
    }

    [Fact]
    public void GetConstruction_ReturnsRegisteredConstruction()
    {
        var manager = new ConstructionManager();
        var construction = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            10,
            new SimulationTime(100));

        manager.RegisterConstruction(construction);
        var retrieved = manager.GetConstruction(construction.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(construction.Id, retrieved.Id);
    }

    [Fact]
    public void AdvanceAllTicks_ProgressesUnderConstruction()
    {
        var manager = new ConstructionManager();
        var validated = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            10,
            new SimulationTime(100)).Validate().Fund().StartConstruction(new SimulationTime(100));

        manager.RegisterConstruction(validated);

        for (int i = 0; i < 9; i++)
        {
            manager.AdvanceAllTicks();
        }

        var construction = manager.GetConstruction(validated.Id);
        Assert.Equal(9, construction!.TicksElapsed);

        manager.AdvanceAllTicks();
        var completed = manager.GetConstruction(validated.Id);
        Assert.Equal(BuildingConstructionState.Completed, completed!.State);
    }

    [Fact]
    public void GetCompletedConstructions_ReturnsOnlyCompleted()
    {
        var manager = new ConstructionManager();

        var construction1 = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.House,
            new GridPosition(10, 20),
            new Money(5000),
            2,
            new SimulationTime(100)).Validate().Fund().StartConstruction(new SimulationTime(100));

        var construction2 = new BuildingConstruction(
            ConstructionId.New(),
            BuildingType.Shop,
            new GridPosition(30, 40),
            new Money(8000),
            10,
            new SimulationTime(100)).Validate().Fund().StartConstruction(new SimulationTime(100));

        manager.RegisterConstruction(construction1);
        manager.RegisterConstruction(construction2);

        manager.AdvanceAllTicks();
        manager.AdvanceAllTicks();

        var completed = manager.GetCompletedConstructions();
        Assert.Single(completed);
        Assert.Equal(construction1.Id, completed[0].Id);
    }
}
