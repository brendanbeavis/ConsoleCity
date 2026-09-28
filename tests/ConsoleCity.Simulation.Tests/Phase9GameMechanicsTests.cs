using ConsoleCity.Game;

namespace ConsoleCity.Simulation.Tests;

public class Phase9GameMechanicsTests
{
    [Fact]
    public void Advance_RaisesResearchAndUnlocksTechnology()
    {
        var session = new GameSession();
        session.CreateNewWorld(42);

        session.Advance(200);

        var available = session.GetAvailableTechnologies();
        Assert.Contains(available, tech => tech.Id.Value == "basic-construction");
        Assert.True(session.ResearchTechnology("basic-construction"));
        Assert.Contains(session.Snapshot!.Progression.UnlockedTechnologies, tech => tech.Value == "basic-construction");
    }

    [Fact]
    public void PurchaseModifier_AddsActiveModifier_AndExpires()
    {
        var session = new GameSession();
        session.CreateNewWorld(42);

        Assert.True(session.PurchaseModifier("civic-momentum"));
        Assert.Contains(session.Snapshot!.Progression.ActiveModifiers, modifier => modifier.Id.Value == "civic-momentum");

        session.Advance(25);

        Assert.DoesNotContain(session.Snapshot!.Progression.ActiveModifiers, modifier => modifier.Id.Value == "civic-momentum");
    }

    [Fact]
    public void SetPolicy_UpdatesActivePolicyState()
    {
        var session = new GameSession();
        session.CreateNewWorld(42);

        Assert.True(session.SetPolicy("education-investment", 1m));

        Assert.Contains(session.Snapshot!.Progression.ActivePolicies, policy => policy.Id.Value == "education-investment" && policy.Intensity == 1m);
    }

    [Fact]
    public void Advance_TriggersCycleTransitionAndEventLog()
    {
        var session = new GameSession();
        session.CreateNewWorld(42);

        session.Advance(720);

        Assert.True(session.Snapshot!.Progression.CycleState.CycleNumber > 1);
        Assert.Contains(session.Snapshot.Progression.EventLog, ev => ev.Category == GameEventCategory.Cycle);
    }

    [Fact]
    public void SaveAndLoad_PreservesProgressionState()
    {
        var baseDirectory = Path.Combine(Path.GetTempPath(), "ConsoleCity.Phase9.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(baseDirectory);

        var session = new GameSession();
        session.CreateNewWorld(42);
        session.PurchaseModifier("research-grants");
        session.SetPolicy("development-incentives", 1m);
        session.Advance(120);
        session.Save("phase9", baseDirectory);

        var loaded = new GameSession();
        loaded.Load("phase9", baseDirectory);

        Assert.Equal(session.Snapshot!.Progression.DevelopmentCredits, loaded.Snapshot!.Progression.DevelopmentCredits);
        Assert.Equal(session.Snapshot.Progression.ResearchPoints, loaded.Snapshot.Progression.ResearchPoints);
        Assert.Equal(session.Snapshot.Progression.ActiveModifiers.Select(modifier => modifier.Id.Value), loaded.Snapshot.Progression.ActiveModifiers.Select(modifier => modifier.Id.Value));
        Assert.Equal(session.Snapshot.Progression.ActivePolicies.Select(policy => policy.Id.Value), loaded.Snapshot.Progression.ActivePolicies.Select(policy => policy.Id.Value));
    }
}
