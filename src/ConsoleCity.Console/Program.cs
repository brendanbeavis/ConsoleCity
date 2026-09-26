using ConsoleCity.Core;
using ConsoleCity.Game;
using ConsoleCity.World;
using ConsoleCity.Agents;

namespace ConsoleCity.Console;

internal static class Program
{
    private static async Task Main()
    {
        var session = new GameSession();
        using var cancellationTokenSource = new CancellationTokenSource();
        var runLoop = RunSimulationLoopAsync(session, cancellationTokenSource.Token);

        WriteBanner();
        WriteHelp();

        while (true)
        {
            System.Console.Write($"[{FormatTime(session.Time)}] > ");
            var input = System.Console.ReadLine();
            if (input is null)
            {
                break;
            }

            var command = input.Trim();
            if (command.Length == 0)
            {
                continue;
            }

            try
            {
                var output = ProcessCommand(session, command);
                if (!string.IsNullOrEmpty(output))
                {
                    foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        WriteLine(line);
                    }
                }

                if (command.Equals("exit", StringComparison.OrdinalIgnoreCase) || command.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
            catch (Exception exception)
            {
                WriteLine($"Error: {exception.Message}");
            }
        }

        cancellationTokenSource.Cancel();
        await runLoop.ConfigureAwait(false);
    }

    private static async Task RunSimulationLoopAsync(GameSession session, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (session.IsWorldCreated && session.IsRunning)
                {
                    session.Advance(1);
                }

                await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    // Public for integration tests
    public static string ProcessCommand(GameSession session, string command)
    {
        if (session is null) throw new ArgumentNullException(nameof(session));
        if (string.IsNullOrWhiteSpace(command)) return string.Empty;

        if (command.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            return string.Join(Environment.NewLine, new[]
            {
                "Commands:",
                "  new [seed]                Create a new world",
                "  start                     Start simulation",
                "  pause                     Pause simulation",
                "  step [hours]              Advance simulation time",
                "  stats                     Show city statistics",
                "  summary                   Show richer city summary",
                "  people                    List people for inspection",
                "  households                List households for inspection",
                "  buildings                 List buildings for inspection",
                "  find person <query>       Find people by name or id",
                "  find household <query>    Find households by id or member name",
                "  find building <query>     Find buildings by name or id",
                "  inspect person <query>    Inspect a person by name or id",
                "  inspect household <query> Inspect a household by id or member name",
                "  inspect building <query>  Inspect a building by name or id",
                "  time                      Show simulation time",
                "  help                      Show commands",
                "  exit                      Quit"
            });
        }

        if (command.StartsWith("new", StringComparison.OrdinalIgnoreCase))
        {
            var seed = TryParseTrailingInt(command) ?? 42;
            session.CreateNewWorld(seed);
            var stats = session.GetCityStatistics();
            return $"Created new world with seed {seed}.\n{FormatStatisticsForDisplay(stats)}";
        }

        if (command.Equals("start", StringComparison.OrdinalIgnoreCase))
        {
            session.Start();
            return "Simulation started.";
        }

        if (command.Equals("pause", StringComparison.OrdinalIgnoreCase))
        {
            session.Pause();
            return "Simulation paused.";
        }

        if (command.StartsWith("step", StringComparison.OrdinalIgnoreCase))
        {
            var ticks = TryParseTrailingInt(command) ?? 1;
            session.Advance(Math.Max(1, ticks));
            return $"Advanced {ticks} hour(s). Current time: {FormatTime(session.Time)}.";
        }

        if (command.Equals("time", StringComparison.OrdinalIgnoreCase))
        {
            return $"Simulation time: {FormatTime(session.Time)}";
        }

        if (command.Equals("stats", StringComparison.OrdinalIgnoreCase) || command.Equals("city", StringComparison.OrdinalIgnoreCase))
        {
            var stats = session.GetCityStatistics();
            return FormatStatisticsForDisplay(stats);
        }

        if (command.Equals("summary", StringComparison.OrdinalIgnoreCase))
        {
            var snapshot = session.Snapshot ?? throw new InvalidOperationException("Create a world first.");
            return BuildSummary(snapshot);
        }

        if (command.Equals("people", StringComparison.OrdinalIgnoreCase))
        {
            var snapshot = RequireSnapshot(session);
            var lines = new List<string> { "People:" };
            foreach (var person in snapshot.Population.People.OrderBy(p => p.DisplayName, StringComparer.Ordinal))
            {
                lines.Add($"{person.DisplayName} | {person.Id} | {person.CurrentActivity} | {person.CurrentLocation}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        if (command.Equals("households", StringComparison.OrdinalIgnoreCase))
        {
            var snapshot = RequireSnapshot(session);
            var lines = new List<string> { "Households:" };
            foreach (var household in snapshot.Population.Households.OrderBy(h => h.Id.Value))
            {
                var members = snapshot.Population.People.Where(person => person.HouseholdId == household.Id).Select(person => person.DisplayName).OrderBy(n => n, StringComparer.Ordinal);
                lines.Add($"{household.Id} | Home {household.HomeLocation} | Members: {string.Join(", ", members)}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        if (command.Equals("buildings", StringComparison.OrdinalIgnoreCase))
        {
            var snapshot = RequireSnapshot(session);
            var lines = new List<string> { "Buildings:" };
            var buildings = snapshot.World.Regions[0].Cities[0].Districts[0].Plots.SelectMany(plot => plot.Buildings);
            foreach (var building in buildings.OrderBy(b => b.Name, StringComparer.Ordinal))
            {
                lines.Add($"{building.Name} | {building.Id} | {building.BuildingType} | {building.Location}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        if (command.StartsWith("inspect person ", StringComparison.OrdinalIgnoreCase))
        {
            return session.InspectPerson(command[15..].Trim());
        }

        if (command.StartsWith("inspect household ", StringComparison.OrdinalIgnoreCase))
        {
            return session.InspectHousehold(command[18..].Trim());
        }

        if (command.StartsWith("inspect building ", StringComparison.OrdinalIgnoreCase))
        {
            return session.InspectBuilding(command[17..].Trim());
        }

        if (command.StartsWith("find person ", StringComparison.OrdinalIgnoreCase))
        {
            var q = command[12..].Trim();
            var snapshot = RequireSnapshot(session);
            var matches = snapshot.Population.People.Where(p => MatchesSimple(q, p.DisplayName) || p.Id.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)).OrderBy(p => p.DisplayName);
            var lines = new List<string> { $"People matching '{q}':" };
            lines.AddRange(matches.Select(p => $"{p.DisplayName} | {p.Id} | Age {p.Age} | {p.CurrentActivity} | Sat {p.Satisfaction:0.00}"));
            return string.Join(Environment.NewLine, lines);
        }

        if (command.StartsWith("find household ", StringComparison.OrdinalIgnoreCase))
        {
            var q = command[15..].Trim();
            var snapshot = RequireSnapshot(session);
            var matches = snapshot.Population.Households.Where(h => h.Id.ToString().Contains(q, StringComparison.OrdinalIgnoreCase) || h.Members.Any(m => MatchesSimple(q, snapshot.Population.People.First(p => p.Id == m).DisplayName))).OrderBy(h => h.Id.Value);
            var lines = new List<string> { $"Households matching '{q}':" };
            foreach (var h in matches)
            {
                var members = h.Members.Select(m => snapshot.Population.People.First(p => p.Id == m).DisplayName);
                lines.Add($"{h.Id} | Home {h.HomeLocation} | Members: {string.Join(", ", members)}");
            }

            return string.Join(Environment.NewLine, lines);
        }

        if (command.StartsWith("find building ", StringComparison.OrdinalIgnoreCase))
        {
            var q = command[14..].Trim();
            var snapshot = RequireSnapshot(session);
            var buildings = snapshot.World.Regions[0].Cities[0].Districts[0].Plots.SelectMany(plot => plot.Buildings);
            var matches = buildings.Where(b => MatchesSimple(q, b.Name) || b.Id.ToString().Contains(q, StringComparison.OrdinalIgnoreCase) || b.BuildingType.ToString().Contains(q, StringComparison.OrdinalIgnoreCase)).OrderBy(b => b.Name);
            var lines = new List<string> { $"Buildings matching '{q}':" };
            lines.AddRange(matches.Select(b => $"{b.Name} | {b.Id} | {b.BuildingType} | {b.Location}"));
            return string.Join(Environment.NewLine, lines);
        }

        return "Unknown command. Type 'help' for available commands.";
    }

    private static string BuildSummary(SimulationSliceSnapshot snapshot)
    {
        var lines = new List<string>();
        var allPeople = snapshot.Population.People;
        var allHouseholds = snapshot.Population.Households;
        var allBuildings = snapshot.World.Regions[0].Cities[0].Districts[0].Plots.SelectMany(p => p.Buildings).ToList();

        lines.Add(FormatStatisticsForDisplay(new CityStatistics(
            $"World Seed {snapshot.World.Seed}",
            snapshot.World.Regions[0].Cities[0].Name,
            snapshot.CurrentTime,
            allPeople.Count,
            allHouseholds.Count,
            allBuildings.Count(b => b.BuildingType is BuildingType.House or BuildingType.Apartment),
            allBuildings.Count(b => b.BuildingType is BuildingType.Shop or BuildingType.Office or BuildingType.Factory),
            allPeople.Count(p => p.EmploymentState == EmploymentState.Employed),
            snapshot.ActiveTrips.Count,
            snapshot.CompletedTrips,
            snapshot.HouseholdPurchases,
            Money.Zero,
            Money.Zero,
            Money.Zero,
            Money.Zero,
            0)));

        var lifeStageCounts = allPeople.GroupBy(p => p.LifeStage).ToDictionary(g => g.Key, g => g.Count());
        lines.Add("Life stages:");
        foreach (var kv in lifeStageCounts.OrderBy(kv => kv.Key.ToString()))
        {
            lines.Add($"  {kv.Key}: {kv.Value}");
        }

        var avgSatisfaction = allPeople.Average(p => p.Satisfaction);
        var avgIncome = allPeople.Average(p => (double)p.Income.Amount);
        lines.Add($"Average satisfaction: {avgSatisfaction:0.00}");
        lines.Add($"Average income: ${avgIncome:0.00}");

        var buildingTypeCounts = allBuildings.GroupBy(b => b.BuildingType).ToDictionary(g => g.Key, g => g.Count());
        lines.Add("Building types:");
        foreach (var kv in buildingTypeCounts)
        {
            lines.Add($"  {kv.Key}: {kv.Value}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatStatisticsForDisplay(CityStatistics statistics)
    {
        var lines = new List<string>
        {
            $"World: {statistics.WorldName}",
            $"City: {statistics.CityName}",
            $"Time: {FormatTime(statistics.CurrentTime)}",
            $"Population: {statistics.Population} | Households: {statistics.Households} | Businesses: {statistics.Businesses}",
            $"Employment: {statistics.EmployedPeople}/{statistics.Population} | Residential buildings: {statistics.ResidentialBuildings}",
            $"Trips: active {statistics.ActiveTrips}, completed {statistics.CompletedTrips} | Purchases: {statistics.HouseholdPurchases}",
            $"Household savings: ${statistics.TotalHouseholdSavings.Amount:0.00} | Household expenses: ${statistics.TotalHouseholdExpenses.Amount:0.00}",
            $"Business cash: ${statistics.TotalBusinessCash.Amount:0.00} | Wages earned: ${statistics.TotalWagesEarned.Amount:0.00}",
            $"Food inventory: {statistics.FoodInventoryUnits}"
        };

        return string.Join(Environment.NewLine, lines);
    }

    private static bool MatchesSimple(string query, string text)
        => !string.IsNullOrWhiteSpace(query) && !string.IsNullOrWhiteSpace(text) && text.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase);

    private static int? TryParseTrailingInt(string command)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            return null;
        }

        return int.TryParse(parts[^1], out var value) ? value : null;
    }

    private static string FormatTime(SimulationTime time) => time.ToGameDateTime().ToString();

    private static void WriteBanner()
    {
        WriteLine("CONSOLECITY");
        WriteLine("First playable slice");
    }

    private static void WriteHelp()
    {
        WriteLine("Commands:");
        WriteLine("  new [seed]                Create a new world");
        WriteLine("  start                     Start simulation");
        WriteLine("  pause                     Pause simulation");
        WriteLine("  step [hours]              Advance simulation time");
        WriteLine("  stats                     Show city statistics");
        WriteLine("  people                    List people for inspection");
        WriteLine("  households                List households for inspection");
        WriteLine("  buildings                 List buildings for inspection");
        WriteLine("  time                      Show simulation time");
        WriteLine("  inspect person <query>    Inspect a person by name or id");
        WriteLine("  inspect household <query> Inspect a household by id or member name");
        WriteLine("  inspect building <query>  Inspect a building by name or id");
        WriteLine("  help                      Show commands");
        WriteLine("  exit                      Quit");
    }

    private static SimulationSliceSnapshot RequireSnapshot(GameSession session)
        => session.Snapshot ?? throw new InvalidOperationException("Create a world first.");

    private static void WriteLine(string text) => System.Console.WriteLine(text);
}
