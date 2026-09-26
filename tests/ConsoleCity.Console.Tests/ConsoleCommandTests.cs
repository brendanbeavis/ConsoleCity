using System.Linq;
using ConsoleCity.Game;
using ConsoleCity.Console;

namespace ConsoleCity.Console.Tests;

public class ConsoleCommandTests
{
    [Fact]
    public void NewWorld_ProducesStatistics()
    {
        var session = new GameSession();
        var output = Program.ProcessCommand(session, "new 42");
        Assert.NotNull(output);
        Assert.Contains("Created new world", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Population:", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PeopleCommand_ListsPeople()
    {
        var session = new GameSession();
        Program.ProcessCommand(session, "new 42");
        var output = Program.ProcessCommand(session, "people");
        Assert.NotNull(output);
        Assert.Contains("People:", output, StringComparison.OrdinalIgnoreCase);
        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length > 1);
    }

    [Fact]
    public void Help_IncludesNewCommands()
    {
        var session = new GameSession();
        var output = Program.ProcessCommand(session, "help");
        Assert.Contains("summary", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("find person", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SummaryCommand_ReturnsLifeStagesAndAverages()
    {
        var session = new GameSession();
        Program.ProcessCommand(session, "new 42");
        var output = Program.ProcessCommand(session, "summary");
        Assert.Contains("Life stages:", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Average satisfaction", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FindPerson_ReturnsMatchingPerson()
    {
        var session = new GameSession();
        Program.ProcessCommand(session, "new 42");
        var snapshot = session.Snapshot!;
        var person = snapshot.Population.People.First();
        var fragment = person.DisplayName.Length > 3 ? person.DisplayName.Substring(0, 3) : person.DisplayName;
        var output = Program.ProcessCommand(session, $"find person {fragment}");
        Assert.Contains(person.DisplayName, output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FindHousehold_ById_ReturnsHousehold()
    {
        var session = new GameSession();
        Program.ProcessCommand(session, "new 42");
        var snapshot = session.Snapshot!;
        var household = snapshot.Population.Households.First();
        var output = Program.ProcessCommand(session, $"find household {household.Id}");
        Assert.Contains(household.Id.ToString(), output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FindBuilding_ByNameOrId_ReturnsMatch()
    {
        var session = new GameSession();
        Program.ProcessCommand(session, "new 42");
        var snapshot = session.Snapshot!;
        var building = snapshot.World.Regions[0].Cities[0].Districts[0].Plots.SelectMany(p => p.Buildings).First();
        var frag = building.Name.Length > 4 ? building.Name.Substring(0, 4) : building.Name;
        var output = Program.ProcessCommand(session, $"find building {frag}");
        Assert.Contains(building.Name, output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void InspectPerson_ById_ReturnsDetails()
    {
        var session = new GameSession();
        Program.ProcessCommand(session, "new 42");
        var snapshot = session.Snapshot!;
        var person = snapshot.Population.People.First();
        var output = Program.ProcessCommand(session, $"inspect person {person.Id}");
        Assert.Contains("PERSON", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(person.DisplayName, output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnknownCommand_ReturnsHelpfulMessage()
    {
        var session = new GameSession();
        var output = Program.ProcessCommand(session, "thiscommanddoesnotexist");
        Assert.Contains("Unknown command", output, StringComparison.OrdinalIgnoreCase);
    }
}
