using ConsoleCity.Agents;
using ConsoleCity.Game;

namespace ConsoleCity.Simulation.Tests;

public class PlayableSliceTests
{
    [Fact]
    public void CreateNewWorld_WithSameSeed_BuildsDeterministicSlice()
    {
        var firstSession = new GameSession();
        var secondSession = new GameSession();

        firstSession.CreateNewWorld(42);
        secondSession.CreateNewWorld(42);

        var first = Assert.IsType<SimulationSliceSnapshot>(firstSession.Snapshot);
        var second = Assert.IsType<SimulationSliceSnapshot>(secondSession.Snapshot);

        Assert.Equal(first.World.Seed, second.World.Seed);
        Assert.Equal(first.World.Regions[0].Name, second.World.Regions[0].Name);
        Assert.Equal(first.World.Regions[0].Cities[0].Name, second.World.Regions[0].Cities[0].Name);
        Assert.Equal(first.World.Regions[0].Cities[0].Districts[0].Name, second.World.Regions[0].Cities[0].Districts[0].Name);
        Assert.Equal(
            first.Population.People.Select(person => person.Id).ToArray(),
            second.Population.People.Select(person => person.Id).ToArray());
        Assert.Equal(
            first.Population.People.Select(person => person.DisplayName).ToArray(),
            second.Population.People.Select(person => person.DisplayName).ToArray());
        Assert.Equal(
            first.Economy.Businesses.Select(business => business.Id).ToArray(),
            second.Economy.Businesses.Select(business => business.Id).ToArray());
        Assert.Equal(
            first.World.Regions[0].Cities[0].Districts[0].Plots.Select(plot => plot.Name).ToArray(),
            second.World.Regions[0].Cities[0].Districts[0].Plots.Select(plot => plot.Name).ToArray());
    }

    [Fact]
    public void Advance_MultipleDays_RunsLivingLoop()
    {
        var session = new GameSession();
        session.CreateNewWorld(42);

        session.Advance(72);

        var snapshot = Assert.IsType<SimulationSliceSnapshot>(session.Snapshot);
        var stats = session.GetCityStatistics();

        Assert.Equal(72, session.Time.Tick);
        Assert.Equal(72, snapshot.CurrentTime.Tick);
        Assert.Equal(6, stats.Population);
        Assert.Equal(3, stats.Households);
        Assert.Equal(1, stats.Businesses);
        Assert.True(stats.CompletedTrips >= 18);
        Assert.True(stats.TotalWagesEarned.Amount > 0m);
        Assert.True(stats.TotalHouseholdExpenses.Amount > 0m);
        Assert.True(stats.TotalBusinessCash.Amount > 0m);
        Assert.All(snapshot.Population.People, person => Assert.NotEqual(EmploymentState.Unemployed, person.EmploymentState));
        Assert.Contains(snapshot.Population.People, person => person.CurrentActivity is AgentActivity.AtHome or AgentActivity.Resting or AgentActivity.BuyingFood);
    }

    [Fact]
    public async Task StartPauseLoop_AllowsAutonomousAdvancement()
    {
        var session = new GameSession();
        session.CreateNewWorld(42);
        session.Start();

        using var cancellationTokenSource = new CancellationTokenSource();
        var loopTask = Task.Run(async () =>
        {
            for (var iteration = 0; iteration < 10 && !cancellationTokenSource.Token.IsCancellationRequested; iteration++)
            {
                if (session.IsWorldCreated && session.IsRunning)
                {
                    session.Advance(1);
                }

                await Task.Delay(10, cancellationTokenSource.Token);
            }
        }, cancellationTokenSource.Token);

        await loopTask;
        session.Pause();

        Assert.True(session.Time.Tick > 0);
    }

    [Fact]
    public void InspectCommands_ReturnReadableDetails()
    {
        var session = new GameSession();
        session.CreateNewWorld(42);

        var snapshot = Assert.IsType<SimulationSliceSnapshot>(session.Snapshot);
        var person = snapshot.Population.People[0];
        var household = snapshot.Population.Households[0];
        var building = snapshot.World.Regions[0].Cities[0].Districts[0].Plots[0].Buildings[0];

        var personDetails = session.InspectPerson(person.DisplayName);
        var householdDetails = session.InspectHousehold(household.Id.ToString());
        var buildingDetails = session.InspectBuilding(building.Name);

        Assert.Contains("PERSON", personDetails);
        Assert.Contains(person.DisplayName, personDetails);
        Assert.Contains("HOUSEHOLD", householdDetails);
        Assert.Contains("BUILDING", buildingDetails);
        Assert.Contains(building.Name, buildingDetails);
    }
}
