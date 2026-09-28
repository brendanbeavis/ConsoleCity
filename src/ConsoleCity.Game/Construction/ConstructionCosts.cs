using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Game.Construction;

/// <summary>
/// Static configuration for construction costs per building type.
/// </summary>
public static class ConstructionCosts
{
    /// <summary>
    /// Get the construction cost for a specific building type.
    /// </summary>
    public static Money GetCost(BuildingType buildingType)
    {
        return buildingType switch
        {
            // Residential
            BuildingType.House => new Money(5000),
            BuildingType.Apartment => new Money(15000),

            // Commercial
            BuildingType.Shop => new Money(8000),
            BuildingType.Office => new Money(20000),

            // Industrial
            BuildingType.Factory => new Money(50000),

            // Agricultural
            BuildingType.Farm => new Money(3000),

            // Services
            BuildingType.School => new Money(30000),
            BuildingType.PoliceStation => new Money(25000),
            BuildingType.FireStation => new Money(25000),

            // Recreation and parks
            BuildingType.Park => new Money(5000),

            // Infrastructure
            BuildingType.UtilityFacility => new Money(40000),
            BuildingType.TransportFacility => new Money(20000),

            // Default/Other
            _ => new Money(10000)
        };
    }

    /// <summary>
    /// Get the construction duration (in ticks) for a specific building type.
    /// </summary>
    public static int GetDurationTicks(BuildingType buildingType)
    {
        return buildingType switch
        {
            // Fast construction (10 ticks)
            BuildingType.House or BuildingType.Shop or BuildingType.Park => 10,

            // Medium construction (20 ticks)
            BuildingType.Apartment or BuildingType.Farm or BuildingType.UtilityFacility => 20,

            // Long construction (40 ticks)
            BuildingType.Office or BuildingType.School or BuildingType.PoliceStation or BuildingType.FireStation => 40,

            // Very long construction (60 ticks)
            BuildingType.Factory => 60,

            // Transport facility (30 ticks)
            BuildingType.TransportFacility => 30,

            // Default: 20 ticks
            _ => 20
        };
    }
}
