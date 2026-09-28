using System.Collections.Generic;

namespace ConsoleCity.Game.Save.Dto;

public sealed record SaveEnvelopeDto(int Version, System.DateTime CreatedAt, SimulationSliceStateDto Payload);

public sealed record SimulationSliceStateDto(
    int Seed,
    long CurrentTick,
    IList<PersonDto> People,
    IList<HouseholdDto> Households,
    IList<CommuteTripDto> ActiveTrips,
    int CompletedTrips,
    int HouseholdPurchases,
    IDictionary<string, string> HomeBuildingByPerson,
    IDictionary<string, string> HomeBuildingByHousehold,
    IDictionary<string, string> WorkplaceBuildingByPerson);

public sealed record PersonDto(
    string Id,
    string HouseholdId,
    string DisplayName,
    int Age,
    string LifeStage,
    EmploymentDto Employment,
    decimal Income,
    decimal Expenses,
    decimal Balance,
    string? ResidencePlotId,
    GridPositionDto CurrentLocation,
    string CurrentActivity,
    string TransportPreference,
    double Satisfaction,
    AgentAttributeProfileDto Attributes,
    IList<NeedStatusDto> Needs,
    IList<SkillRatingDto> Skills,
    IList<RelationshipLinkDto> Relationships,
    IList<AgentGoalDto> Goals,
    RoutinePlanDto Routine);

public sealed record HouseholdDto(
    string Id,
    string? HomePlotId,
    GridPositionDto? HomeLocation,
    IList<string> Members,
    decimal Income,
    decimal Expenses,
    decimal Savings,
    decimal Debt,
    decimal FoodDemand,
    decimal UtilityDemand,
    int TransportAssets,
    double Satisfaction);

public sealed record EmploymentDto(string State, string? WorkplaceId, GridPositionDto? WorkplaceLocation, decimal Wage);

public sealed record NeedStatusDto(string Need, double Fulfillment);

public sealed record SkillRatingDto(string Name, double Level);

public sealed record RelationshipLinkDto(string PersonId, string RelationshipType, double Strength);

public sealed record AgentGoalDto(string GoalType, string Description, double Priority, long? DueByTick);

public sealed record AgentAttributeProfileDto(double Health, double Energy, double Sociability, double Diligence);

public sealed record RoutinePlanDto(IList<RoutineBlockDto> Blocks);

public sealed record RoutineBlockDto(int StartHour, int EndHour, string? PreferredAction);

public sealed record GridPositionDto(int X, int Y);

public sealed record CommuteTripDto(string PersonId, GridPositionDto Origin, GridPositionDto Destination, long DepartureTick, long ArrivalTick, string Mode, string Purpose);
