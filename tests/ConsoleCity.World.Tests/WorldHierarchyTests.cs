using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.World.Tests;

public class WorldHierarchyTests
{
    [Fact]
    public void WorldModel_ContainsNestedHierarchyAndCells()
    {
        var worldId = WorldId.New();
        var regionId = RegionId.New();
        var cityId = CityId.New();
        var districtId = DistrictId.New();
        var plotId = PlotId.New();
        var buildingId = BuildingId.New();

        var building = new BuildingModel(
            buildingId,
            plotId,
            "House 1",
            BuildingType.House,
            new SimulationTime(0),
            new GridPosition(0, 0),
            null,
            [new GridPosition(0, 0)],
            BuildingCapacities.Empty,
            Money.Zero,
            new SimulationTick(10),
            Money.Zero,
            ObjectLifecycleState.Planned,
            1d);

        var plot = new PlotModel(
            plotId,
            districtId,
            "Residential Plot",
            LandUseType.Residential,
            new SimulationTime(0),
            new GridPosition(0, 0),
            null,
            [new GridPosition(0, 0)],
            [building],
            ObjectLifecycleState.Operational);

        var district = new DistrictModel(
            districtId,
            cityId,
            "Central",
            DistrictType.Residential,
            new SimulationTime(0),
            new GridPosition(0, 0),
            null,
            [new GridPosition(0, 0), new GridPosition(1, 0)],
            [plot]);

        var city = new CityModel(
            cityId,
            regionId,
            "Sample City",
            new SimulationTime(0),
            new GridPosition(0, 0),
            null,
            [new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(1, 1)],
            [district]);

        var region = new RegionModel(
            regionId,
            worldId,
            "Sample Region",
            new SimulationTime(0),
            new GridPosition(0, 0),
            null,
            [new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(1, 1), new GridPosition(2, 1)],
            [city]);

        var world = new WorldModel(
            worldId,
            42,
            new SimulationTime(0),
            [region],
            [new TerrainCellModel(
                new GridPosition(0, 0),
                TerrainType.Plains,
                0,
                false,
                false,
                false,
                false,
                1d,
                [],
                null,
                null,
                new SimulationTime(0))],
            EnvironmentalStateModel.Neutral);

        Assert.Equal(worldId, world.Id);
        Assert.Single(world.Regions);
        Assert.Single(world.Cells);
        Assert.Equal(regionId, world.Regions[0].Id);
        Assert.Equal(cityId, world.Regions[0].Cities[0].Id);
        Assert.Equal(districtId, world.Regions[0].Cities[0].Districts[0].Id);
        Assert.Equal(plotId, world.Regions[0].Cities[0].Districts[0].Plots[0].Id);
        Assert.Equal(buildingId, world.Regions[0].Cities[0].Districts[0].Plots[0].Buildings[0].Id);
    }

    [Fact]
    public void PlotModel_RejectsBuildingsFromDifferentPlot()
    {
        var building = new BuildingModel(
            BuildingId.New(),
            PlotId.New(),
            "Mismatch",
            BuildingType.Other,
            new SimulationTime(0),
            new GridPosition(0, 0),
            null,
            [new GridPosition(0, 0)],
            BuildingCapacities.Empty,
            Money.Zero,
            new SimulationTick(1),
            Money.Zero,
            ObjectLifecycleState.Planned,
            1d);

        Assert.Throws<ArgumentException>(() => new PlotModel(
            PlotId.New(),
            DistrictId.New(),
            "Plot",
            LandUseType.Residential,
            new SimulationTime(0),
            new GridPosition(0, 0),
            null,
            [new GridPosition(0, 0)],
            [building],
            ObjectLifecycleState.Planned));
    }

    [Fact]
    public void ResourceDepositModel_RejectsInvalidQuality()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ResourceDepositModel(
            ResourceType.Coal,
            new GridPosition(0, 0),
            new Quantity(10m),
            1.5d,
            new Quantity(1m),
            new SimulationTime(0)));
    }
}
