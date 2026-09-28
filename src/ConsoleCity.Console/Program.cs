using ConsoleCity.Core;
using ConsoleCity.Game;
using ConsoleCity.Game.Construction;
using ConsoleCity.World;
using ConsoleCity.Agents;

namespace ConsoleCity.Console;

public static class Program
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
                "  build <type> <x> <y>     Construct a building at location (x, y)",
                "  demolish <building_id>   Demolish an existing building",
                "  constructions             List active construction projects",
                "  cancel <construction_id> Cancel an active construction",
                "  map                       Display ASCII map of world",
                "  map region <x1> <y1> <x2> <y2>  Display specific region",
                "  map coords                Display map with coordinates",
                "  map legend                Display map with legend",
                "  map buildings             List all buildings on map",
                "  map plots                 List all plots on map",
                "  map cell <x> <y>         Show details for specific cell",
                "  save <name>               Save the current simulation",
                "  load <name>               Load a saved simulation",
                "  saves                     List available saves",
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

        if (command.StartsWith("save ", StringComparison.OrdinalIgnoreCase))
        {
            var name = command[5..].Trim();
            if (string.IsNullOrEmpty(name)) return "Specify a save name.";
            session.Save(name);
            return $"Saved '{name}'.";
        }

        if (command.StartsWith("load ", StringComparison.OrdinalIgnoreCase))
        {
            var name = command[5..].Trim();
            if (string.IsNullOrEmpty(name)) return "Specify a save name.";
            session.Load(name);
            return $"Loaded '{name}'.";
        }

        if (command.Equals("saves", StringComparison.OrdinalIgnoreCase))
        {
            var saves = session.ListSaves();
            if (saves == null || saves.Count == 0) return "No saves found.";
            return string.Join(Environment.NewLine, new[] { "Saves:" }.Concat(saves));
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

        if (command.StartsWith("build ", StringComparison.OrdinalIgnoreCase))
        {
            return HandleBuildCommand(session, command[6..].Trim());
        }

        if (command.StartsWith("demolish ", StringComparison.OrdinalIgnoreCase))
        {
            return HandleDemolishCommand(session, command[9..].Trim());
        }

        if (command.Equals("constructions", StringComparison.OrdinalIgnoreCase) || command.Equals("construction", StringComparison.OrdinalIgnoreCase))
        {
            return HandleListConstructionsCommand(session);
        }

        if (command.StartsWith("cancel ", StringComparison.OrdinalIgnoreCase))
        {
            return HandleCancelConstructionCommand(session, command[7..].Trim());
        }

        if (command.StartsWith("map", StringComparison.OrdinalIgnoreCase))
        {
            return HandleMapCommand(session, command);
        }

        return "Unknown command. Type 'help' for available commands.";
    }

    private static string HandleBuildCommand(GameSession session, string args)
    {
        RequireSnapshot(session);

        var parts = args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            return "Usage: build <BuildingType> <x> <y>\nExample: build House 10 20";
        }

        if (!Enum.TryParse<BuildingType>(parts[0], ignoreCase: true, out var buildingType))
        {
            return $"Unknown building type: {parts[0]}. Valid types: {string.Join(", ", Enum.GetNames(typeof(BuildingType)))}";
        }

        if (!int.TryParse(parts[1], out var x) || !int.TryParse(parts[2], out var y))
        {
            return "Coordinates must be integers.";
        }

        var location = new GridPosition(x, y);
        var (request, construction) = session.RequestConstruction(buildingType, location);

        if (!request.IsValid)
        {
            var errors = string.Join(Environment.NewLine + "  ", request.ValidationErrors);
            return $"Construction request failed:\n  {errors}";
        }

        if (construction == null)
        {
            return "Construction request was rejected.";
        }

        var cost = ConstructionCosts.GetCost(buildingType);
        var duration = ConstructionCosts.GetDurationTicks(buildingType);
        return $"Construction started at {location}.\n" +
               $"  Type: {buildingType}\n" +
               $"  Cost: {cost}\n" +
               $"  Duration: {duration} ticks\n" +
               $"  Construction ID: {construction.Id}\n" +
               $"  Progress: {construction.GetProgress():F1}%";
    }

    private static string HandleDemolishCommand(GameSession session, string args)
    {
        RequireSnapshot(session);

        if (string.IsNullOrWhiteSpace(args))
        {
            return "Usage: demolish <building_id>";
        }

        var snapshot = session.Snapshot!;
        var buildings = snapshot.World.Regions[0].Cities[0].Districts[0].Plots.SelectMany(plot => plot.Buildings);
        var building = buildings.FirstOrDefault(b => b.Id.ToString().Contains(args, StringComparison.OrdinalIgnoreCase));

        if (building == null)
        {
            return $"Building not found: {args}";
        }

        if (session.DemolishBuilding(building.Id))
        {
            return $"Demolished {building.Name} at {building.Location}.";
        }

        return $"Failed to demolish {building.Name}. Check funds or building status.";
    }

    private static string HandleListConstructionsCommand(GameSession session)
    {
        if (!session.IsWorldCreated)
        {
            return "Create a world first.";
        }

        var constructions = session.GetActiveConstructions();
        if (constructions.Count == 0)
        {
            return "No active construction projects.";
        }

        var lines = new List<string> { "Active Construction Projects:" };
        foreach (var kvp in constructions)
        {
            var construction = kvp.Value;
            lines.Add($"  ID: {construction.Id}");
            lines.Add($"    Type: {construction.BuildingType} at {construction.Location}");
            lines.Add($"    State: {construction.State}");
            lines.Add($"    Progress: {construction.GetProgress():F1}% ({construction.TicksElapsed}/{construction.DurationTicks} ticks)");
            lines.Add($"    Cost: {construction.Cost}");
            lines.Add("");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string HandleCancelConstructionCommand(GameSession session, string args)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            return "Usage: cancel <construction_id>";
        }

        var constructions = session.GetActiveConstructions();
        var id = constructions.Keys.FirstOrDefault(k => k.Value.ToString().Contains(args, StringComparison.OrdinalIgnoreCase));

        if (id.Value == Guid.Empty)
        {
            return $"Construction not found: {args}";
        }

        if (session.CancelConstruction(id))
        {
            return $"Construction {id} cancelled and refunded.";
        }

        return $"Failed to cancel construction {id}. It may already be complete.";
    }

    private static string HandleMapCommand(GameSession session, string command)
    {
        var renderer = session.GetMapRenderer();
        if (renderer == null)
        {
            return "No map available. Create a world first.";
        }

        var args = command.Length > 3 ? command[4..].Trim() : string.Empty;

        if (args.Equals("coords", StringComparison.OrdinalIgnoreCase))
        {
            return renderer.RenderWithCoordinates();
        }

        if (args.Equals("legend", StringComparison.OrdinalIgnoreCase))
        {
            return renderer.RenderWithLegend();
        }

        if (args.Equals("buildings", StringComparison.OrdinalIgnoreCase))
        {
            return renderer.RenderBuildingSummary();
        }

        if (args.Equals("plots", StringComparison.OrdinalIgnoreCase))
        {
            return renderer.RenderPlotSummary();
        }

        if (args.StartsWith("region ", StringComparison.OrdinalIgnoreCase))
        {
            var regionArgs = args[7..].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (regionArgs.Length != 4 || !int.TryParse(regionArgs[0], out var x1) || !int.TryParse(regionArgs[1], out var y1) ||
                !int.TryParse(regionArgs[2], out var x2) || !int.TryParse(regionArgs[3], out var y2))
            {
                return "Usage: map region <x1> <y1> <x2> <y2>";
            }

            return renderer.RenderRegion(new GridPosition(x1, y1), new GridPosition(x2, y2));
        }

        if (args.StartsWith("cell ", StringComparison.OrdinalIgnoreCase))
        {
            var cellArgs = args[5..].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (cellArgs.Length != 2 || !int.TryParse(cellArgs[0], out var x) || !int.TryParse(cellArgs[1], out var y))
            {
                return "Usage: map cell <x> <y>";
            }

            return renderer.RenderCellDetail(new GridPosition(x, y));
        }

        // Default: render full map
        return renderer.RenderWithLegend();
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
