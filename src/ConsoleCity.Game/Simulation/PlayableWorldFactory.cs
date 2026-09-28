using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.Simulation;
using ConsoleCity.Transport;
using ConsoleCity.World;

namespace ConsoleCity.Game;

internal static class PlayableWorldFactory
{
    private static readonly string[] FirstNames = ["Ava", "Noah", "Mila", "Leo", "Ivy", "Owen", "Zoe", "Eli", "Ruby", "Finn"];
    private static readonly string[] LastNames = ["Carter", "Bennett", "Harper", "Turner", "Fletcher", "Reed", "Brooks", "Cohen"];
    private static readonly GridPosition[] ResidentialPositions =
    [
        new GridPosition(0, 0),
        new GridPosition(0, 2),
        new GridPosition(3, 2),
        new GridPosition(3, 0)
    ];

    private static readonly GridPosition WorkplacePosition = new(1, 1);

    public static SimulationSliceState Create(int seed)
    {
        var random = new DeterministicRandomSource(seed);
        var currentTime = new SimulationTime(0);

        var worldId = DeterministicIdFactory.World(seed);
        var regionId = DeterministicIdFactory.Region(seed, 0);
        var cityId = DeterministicIdFactory.City(seed, 0);
        var districtId = DeterministicIdFactory.District(seed, 0);
        var workplacePlotId = DeterministicIdFactory.Plot(seed, ResidentialPositions.Length);
        var workplaceBuildingId = DeterministicIdFactory.Building(seed, ResidentialPositions.Length);
        var businessId = DeterministicIdFactory.Organization(seed, 0);

        var residentialPlots = new List<PlotModel>();
        var residentialBuildings = new List<BuildingModel>();

        for (var index = 0; index < ResidentialPositions.Length; index++)
        {
            var plotId = DeterministicIdFactory.Plot(seed, index);
            var buildingId = DeterministicIdFactory.Building(seed, index);
            var position = ResidentialPositions[index];
            var plotName = $"Residential Plot {index + 1}";
            var buildingName = index == ResidentialPositions.Length - 1 ? "Vacant Cottage" : $"Cottage {index + 1}";

            var building = new BuildingModel(
                buildingId,
                plotId,
                buildingName,
                BuildingType.House,
                currentTime,
                position,
                null,
                [position],
                new BuildingCapacities(2, 0, 0, 0, 0, 0),
                new Money(80m),
                new SimulationTick(24),
                new Money(1m),
                ObjectLifecycleState.Operational,
                1d);

            residentialBuildings.Add(building);
            residentialPlots.Add(new PlotModel(
                plotId,
                districtId,
                plotName,
                LandUseType.Residential,
                currentTime,
                position,
                null,
                [position],
                [building],
                ObjectLifecycleState.Operational));
        }

        var workplaceBuilding = new BuildingModel(
            workplaceBuildingId,
            workplacePlotId,
            "Corner Market",
            BuildingType.Shop,
            currentTime,
            WorkplacePosition,
            null,
            [WorkplacePosition],
            new BuildingCapacities(0, 6, 24, 0, 240, 24),
            new Money(160m),
            new SimulationTick(36),
            new Money(3m),
            ObjectLifecycleState.Operational,
            1d);

        var workplacePlot = new PlotModel(
            workplacePlotId,
            districtId,
            "Market Square",
            LandUseType.Commercial,
            currentTime,
            WorkplacePosition,
            null,
            [WorkplacePosition],
            [workplaceBuilding],
            ObjectLifecycleState.Operational);

        var plots = residentialPlots.Concat([workplacePlot]).ToList();
        var districtCells = BuildDistrictCells();
        var district = new DistrictModel(
            districtId,
            cityId,
            "Founders District",
            DistrictType.Mixed,
            currentTime,
            WorkplacePosition,
            null,
            districtCells,
            plots);

        var city = new CityModel(
            cityId,
            regionId,
            "Seedfall",
            currentTime,
            WorkplacePosition,
            null,
            districtCells,
            [district]);

        var region = new RegionModel(
            regionId,
            worldId,
            "Starter Region",
            currentTime,
            WorkplacePosition,
            null,
            districtCells,
            [city]);

        var terrain = BuildTerrainCells(currentTime, plots);
        var world = new WorldModel(
            worldId,
            seed,
            currentTime,
            [region],
            terrain,
            EnvironmentalStateModel.Neutral);

        var householdAssignments = new[]
        {
            new[] { 0, 1 },
            new[] { 2, 3 },
            new[] { 4, 5 }
        };

        var personIds = Enumerable.Range(0, 6).Select(index => DeterministicIdFactory.Person(seed, index)).ToArray();
        var householdIds = Enumerable.Range(0, householdAssignments.Length).Select(index => DeterministicIdFactory.Household(seed, index)).ToArray();
        var dailyWage = new Money(12m);
        var people = new List<PersonAgent>();
        var households = new List<HouseholdAgent>();
        var homeBuildingByPerson = new Dictionary<PersonId, BuildingId>();
        var homeBuildingByHousehold = new Dictionary<HouseholdId, BuildingId>();
        var workplaceBuildingByPerson = new Dictionary<PersonId, BuildingId>();

        for (var householdIndex = 0; householdIndex < householdAssignments.Length; householdIndex++)
        {
            var householdId = householdIds[householdIndex];
            var plot = residentialPlots[householdIndex];
            var homeBuilding = residentialBuildings[householdIndex];
            var memberIndices = householdAssignments[householdIndex];
            var memberIds = memberIndices.Select(index => personIds[index]).ToArray();
            var savings = new Money(90m + random.Next(0, 41));

            homeBuildingByHousehold[householdId] = homeBuilding.Id;

            households.Add(new HouseholdAgent(
                householdId,
                plot.Id,
                homeBuilding.Location,
                memberIds,
                Money.Zero,
                Money.Zero,
                savings,
                Money.Zero,
                new Quantity(memberIds.Length * 2m),
                new Quantity(memberIds.Length),
                0,
                0.7d));

            foreach (var memberIndex in memberIndices)
            {
                var personId = personIds[memberIndex];
                var age = 22 + random.Next(0, 31);
                var lifeStage = LifeStageExtensions.FromAge(age);
                var name = CreatePersonName(seed, memberIndex, random);
                var balance = new Money(15m + random.Next(0, 16));
                var needs = CreateNeeds(random);
                var relationships = memberIds
                    .Where(otherId => otherId != personId)
                    .Select(otherId => new RelationshipLink(otherId, RelationshipType.HouseholdMember, 0.8d))
                    .ToList();
                var goals = new List<AgentGoal>
                {
                    new(GoalType.Work, "Keep the household stable through work.", 0.8d),
                    new(GoalType.Food, "Keep the pantry stocked.", 0.6d)
                };

                people.Add(new PersonAgent(
                    personId,
                    householdId,
                    name,
                    age,
                    lifeStage,
                    new EmploymentRecord(EmploymentState.Employed, businessId, WorkplacePosition, dailyWage),
                    Money.Zero,
                    Money.Zero,
                    balance,
                    plot.Id,
                    homeBuilding.Location,
                    AgentActivity.AtHome,
                    TransportPreference.Walk,
                    0.75d,
                    new AgentAttributeProfile(
                        0.8d + (random.NextDouble() * 0.2d),
                        0.7d + (random.NextDouble() * 0.3d),
                        0.4d + (random.NextDouble() * 0.4d),
                        0.6d + (random.NextDouble() * 0.4d)),
                    needs,
                    [new SkillRating("Retail", 0.5d + (random.NextDouble() * 0.4d))],
                    relationships,
                    goals,
                    CreateRoutinePlan()));

                homeBuildingByPerson[personId] = homeBuilding.Id;
                workplaceBuildingByPerson[personId] = workplaceBuildingId;
            }
        }

        var business = new BusinessModel(
            businessId,
            "Corner Market",
            EconomicActorType.Business,
            BusinessLifecycleState.Operating,
            currentTime,
            workplaceBuildingId,
            WorkplacePosition,
            new Money(900m),
            Money.Zero,
            Money.Zero,
            dailyWage,
            personIds,
            [new InventoryLine("grain", 80m), new InventoryLine("packaged-food", 36m)],
            ["bakery-daily"]);

        var economy = new EconomySnapshot(
            currentTime,
            [new PriceQuote("packaged-food", 4m, 4m), new PriceQuote("grain", 1m, 1m)],
            [new ProductionRecipe("bakery-daily", [new InventoryLine("grain", 4m)], [new InventoryLine("packaged-food", 12m)], new Money(6m))],
            BuildIndicators(people, households, [business]),
            [business],
            GovernmentFinance.Zero,
            []);

        var progression = ProgressionEngine.CreateInitialProgression(currentTime);

        return new SimulationSliceState(
            seed,
            currentTime,
            world,
            people,
            households,
            economy,
            progression,
            homeBuildingByPerson,
            homeBuildingByHousehold,
            workplaceBuildingByPerson,
            [],
            0,
            0);
    }

    private static string CreatePersonName(int seed, int personIndex, DeterministicRandomSource random)
    {
        var first = FirstNames[(seed + personIndex + random.Next(0, FirstNames.Length)) % FirstNames.Length];
        var last = LastNames[(seed + (personIndex * 3) + random.Next(0, LastNames.Length)) % LastNames.Length];
        return $"{first} {last}";
    }

    private static IReadOnlyList<NeedStatus> CreateNeeds(DeterministicRandomSource random)
        =>
        [
            new NeedStatus(NeedType.Shelter, 0.95d),
            new NeedStatus(NeedType.Food, 0.7d + (random.NextDouble() * 0.2d)),
            new NeedStatus(NeedType.Water, 0.85d),
            new NeedStatus(NeedType.Income, 0.75d),
            new NeedStatus(NeedType.Recreation, 0.65d + (random.NextDouble() * 0.2d)),
            new NeedStatus(NeedType.Mobility, 0.8d)
        ];

    private static RoutinePlan CreateRoutinePlan()
        => new(
        [
            new RoutineBlock(0, 6, AgentActionType.Rest),
            new RoutineBlock(6, 8, AgentActionType.ReturnHome),
            new RoutineBlock(8, 17, AgentActionType.GoToWork),
            new RoutineBlock(17, 20, AgentActionType.ReturnHome),
            new RoutineBlock(20, 22, AgentActionType.BuyFood),
            new RoutineBlock(22, 24, AgentActionType.Rest)
        ]);

    private static EconomicIndicators BuildIndicators(
        IReadOnlyList<PersonAgent> people,
        IReadOnlyList<HouseholdAgent> households,
        IReadOnlyList<BusinessModel> businesses)
    {
        var employed = people.Count(person => person.EmploymentState == EmploymentState.Employed);
        var employmentRate = people.Count == 0 ? 0m : employed / (decimal)people.Count;
        var householdIncome = households.Aggregate(Money.Zero, static (current, household) => current + household.Income);
        var householdExpenses = households.Aggregate(Money.Zero, static (current, household) => current + household.Expenses);
        var businessRevenue = businesses.Aggregate(Money.Zero, static (current, business) => current + business.Revenue);
        var businessCosts = businesses.Aggregate(Money.Zero, static (current, business) => current + business.Expenses);
        var consumerDemand = households.Aggregate(Quantity.Zero, static (current, household) => current + household.FoodDemand);
        var inventory = businesses
            .SelectMany(business => business.Inventory)
            .Aggregate(Quantity.Zero, static (current, line) => current + line.Quantity);

        return new EconomicIndicators(
            employmentRate,
            householdIncome,
            householdExpenses,
            businessRevenue,
            businessCosts,
            0,
            consumerDemand: consumerDemand,
            production: Quantity.Zero,
            inventory: inventory);
    }

    private static List<GridPosition> BuildDistrictCells()
    {
        var cells = new List<GridPosition>();
        for (var y = 0; y < 3; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                cells.Add(new GridPosition(x, y));
            }
        }

        return cells;
    }

    private static IReadOnlyList<TerrainCellModel> BuildTerrainCells(SimulationTime currentTime, IReadOnlyList<PlotModel> plots)
    {
        var plotByPosition = plots.ToDictionary(plot => plot.Location);
        var buildingByPosition = plots
            .SelectMany(plot => plot.Buildings)
            .ToDictionary(building => building.Location);
        var cells = new List<TerrainCellModel>();

        for (var y = 0; y < 3; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                var position = new GridPosition(x, y);
                plotByPosition.TryGetValue(position, out var plot);
                buildingByPosition.TryGetValue(position, out var building);

                cells.Add(new TerrainCellModel(
                    position,
                    TerrainType.Plains,
                    0,
                    false,
                    hasRoad: x == 1 || y == 1,
                    hasRail: false,
                    hasPath: true,
                    developmentSuitability: 0.9d,
                    [],
                    plot?.Id,
                    building?.Id,
                    currentTime));
            }
        }

        return cells;
    }
}
