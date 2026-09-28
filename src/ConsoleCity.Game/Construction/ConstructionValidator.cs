using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Game.Construction;

/// <summary>
/// Validates construction requests against world state and constraints.
/// </summary>
public static class ConstructionValidator
{
    /// <summary>
    /// Validate a construction request against world constraints.
    /// Returns the request with validation errors added if validation fails.
    /// </summary>
    public static ConstructionRequest Validate(
        ConstructionRequest request,
        WorldModel world,
        Money playerFunds)
    {
        var result = request.ClearValidationErrors();

        // Check if location is within world bounds
        if (!IsLocationValid(request.Location, world))
        {
            result = result.WithValidationError($"Location {request.Location} is outside world bounds.");
        }

        // Check if plot at location is available and has appropriate zoning
        var plotAtLocation = FindPlotAtLocation(request.Location, world);
        if (plotAtLocation == null)
        {
            result = result.WithValidationError($"No plot found at location {request.Location}.");
        }
        else
        {
            // Validate zoning compatibility
            var zoneError = ValidateZoning(request.BuildingType, plotAtLocation);
            if (zoneError != null)
            {
                result = result.WithValidationError(zoneError);
            }

            // Check for existing buildings at location
            if (plotAtLocation.Buildings.Any(b => b.State == ObjectLifecycleState.Operational))
            {
                result = result.WithValidationError($"Plot at {request.Location} is already occupied by a building.");
            }
        }

        // Check player funds
        var constructionCost = ConstructionCosts.GetCost(request.BuildingType);
        if (playerFunds.Amount < constructionCost.Amount)
        {
            result = result.WithValidationError(
                $"Insufficient funds: construction costs {constructionCost}, but player only has {playerFunds}.");
        }

        // Check building-specific prerequisites
        var prerequisiteError = CheckBuildingPrerequisites(request.BuildingType);
        if (prerequisiteError != null)
        {
            result = result.WithValidationError(prerequisiteError);
        }

        return result;
    }

    /// <summary>
    /// Check if a location is within the world bounds.
    /// </summary>
    private static bool IsLocationValid(GridPosition location, WorldModel world)
    {
        return world.Cells.Any(c => c.Position == location);
    }

    /// <summary>
    /// Find the plot containing a specific location.
    /// </summary>
    private static PlotModel? FindPlotAtLocation(GridPosition location, WorldModel world)
    {
        return world.Regions
            .SelectMany(r => r.Cities)
            .SelectMany(c => c.Districts)
            .SelectMany(d => d.Plots)
            .FirstOrDefault(p => p.Cells.Contains(location));
    }

    /// <summary>
    /// Validate that the building type is compatible with the plot's zoning.
    /// </summary>
    private static string? ValidateZoning(BuildingType buildingType, PlotModel plot)
    {
        // Map building types to required land use types
        var requiredLandUse = GetRequiredLandUse(buildingType);

        // Allow Unassigned plots (can be developed freely) or exact match
        if (plot.LandUse != LandUseType.Unassigned && plot.LandUse != requiredLandUse)
        {
            return $"Building type {buildingType} is not compatible with zone type {plot.LandUse}. Requires {requiredLandUse}.";
        }

        return null;
    }

    /// <summary>
    /// Map building types to required land use types.
    /// </summary>
    private static LandUseType GetRequiredLandUse(BuildingType buildingType)
    {
        return buildingType switch
        {
            BuildingType.House or BuildingType.Apartment => LandUseType.Residential,
            BuildingType.Shop or BuildingType.Office => LandUseType.Commercial,
            BuildingType.Factory => LandUseType.Industrial,
            BuildingType.Farm => LandUseType.Farming,
            BuildingType.School => LandUseType.Service,
            BuildingType.PoliceStation or BuildingType.FireStation => LandUseType.Service,
            BuildingType.Park => LandUseType.Recreation,
            BuildingType.UtilityFacility => LandUseType.Infrastructure,
            BuildingType.TransportFacility => LandUseType.Transport,
            _ => LandUseType.Unassigned
        };
    }

    /// <summary>
    /// Check building-specific prerequisites (e.g., minimum city size, technology unlocks).
    /// </summary>
    private static string? CheckBuildingPrerequisites(BuildingType buildingType)
    {
        // For now, all building types are available.
        // This can be extended later to check technology unlocks, city milestone requirements, etc.
        return null;
    }
}

