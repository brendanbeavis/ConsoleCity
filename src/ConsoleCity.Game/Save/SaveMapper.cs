using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleCity.Game.Save.Dto;
using ConsoleCity.Core;
using ConsoleCity.Agents;
using ConsoleCity.World;
using ConsoleCity.Economy;
using ConsoleCity.Transport;

namespace ConsoleCity.Game.Save
{
    internal static class SaveMapper
    {
        public static SimulationSliceStateDto ToDto(SimulationSliceState state)
        {
            if (state is null) throw new ArgumentNullException(nameof(state));

            var people = state.People.Select(p => ToPersonDto(p)).ToList();
            var households = state.Households.Select(h => ToHouseholdDto(h)).ToList();
            var trips = state.ActiveTrips.Select(t => ToTripDto(t)).ToList();

            return new SimulationSliceStateDto(
                state.Seed,
                state.CurrentTime.Tick,
                people,
                households,
                trips,
                state.CompletedTrips,
                state.HouseholdPurchases,
                state.HomeBuildingByPerson.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.ToString()),
                state.HomeBuildingByHousehold.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.ToString()),
                state.WorkplaceBuildingByPerson.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.ToString()));
        }

        public static SimulationSliceState FromDto(SimulationSliceStateDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var people = dto.People.Select(MapPerson).ToList().AsReadOnly();
            var households = dto.Households.Select(MapHousehold).ToList().AsReadOnly();
            var activeTrips = dto.ActiveTrips.Select(MapTrip).ToList().AsReadOnly();

            var homeByPerson = dto.HomeBuildingByPerson.ToDictionary(kv => PersonIdFromString(kv.Key), kv => BuildingIdFromString(kv.Value));
            var homeByHousehold = dto.HomeBuildingByHousehold.ToDictionary(kv => HouseholdIdFromString(kv.Key), kv => BuildingIdFromString(kv.Value));
            var workByPerson = dto.WorkplaceBuildingByPerson.ToDictionary(kv => PersonIdFromString(kv.Key), kv => BuildingIdFromString(kv.Value));

            // Recreate minimal WorldModel from existing runtime constructors: currently we cannot fully reconstruct World so use placeholders where required
            // For now load an empty world with same seed and tick createdAt
            var world = new WorldModel(new WorldId(Guid.NewGuid()), dto.Seed, new SimulationTime(dto.CurrentTick), Array.Empty<RegionModel>(), Array.Empty<TerrainCellModel>(), default);

            return new SimulationSliceState(
                dto.Seed,
                new SimulationTime(dto.CurrentTick),
                world,
                people,
                households,
                new EconomySnapshot(new SimulationTime(dto.CurrentTick), Array.Empty<PriceQuote>(), Array.Empty<ProductionRecipe>(), new EconomicIndicators(0m, Money.Zero, Money.Zero, Money.Zero, Money.Zero, 0)),
                new Dictionary<PersonId, BuildingId>(homeByPerson),
                new Dictionary<HouseholdId, BuildingId>(homeByHousehold),
                new Dictionary<PersonId, BuildingId>(workByPerson),
                activeTrips,
                dto.CompletedTrips,
                dto.HouseholdPurchases);
        }

        private static PersonAgent MapPerson(PersonDto dto)
        {
            var id = PersonIdFromString(dto.Id);
            var householdId = HouseholdIdFromString(dto.HouseholdId);
            var employment = new EmploymentRecord(Enum.Parse<EmploymentState>(dto.Employment.State, true),
                string.IsNullOrWhiteSpace(dto.Employment.WorkplaceId) ? null : new OrganizationId(Guid.Parse(dto.Employment.WorkplaceId)),
                dto.Employment.WorkplaceLocation is null ? null : new GridPosition(dto.Employment.WorkplaceLocation.X, dto.Employment.WorkplaceLocation.Y),
                new Money(dto.Employment.Wage));

            var needs = dto.Needs.Select(n => new NeedStatus(Enum.Parse<NeedType>(n.Need, true), n.Fulfillment)).ToList().AsReadOnly();
            var skills = dto.Skills.Select(s => new SkillRating(s.Name, s.Level)).ToList().AsReadOnly();
            var relationships = dto.Relationships.Select(r => new RelationshipLink(PersonIdFromString(r.PersonId), Enum.Parse<RelationshipType>(r.RelationshipType, true), r.Strength)).ToList().AsReadOnly();
            var goals = dto.Goals.Select(g => new AgentGoal(Enum.Parse<GoalType>(g.GoalType, true), g.Description, g.Priority, g.DueByTick is null ? null : new SimulationTime(g.DueByTick.Value))).ToList().AsReadOnly();

            AgentActionType? ToAction(string? s) => string.IsNullOrWhiteSpace(s) ? null : Enum.TryParse<AgentActionType>(s, true, out var a) ? a : null;

                        var routine = new RoutinePlan(dto.Routine.Blocks.Select(b => new RoutineBlock(b.StartHour, b.EndHour, ToAction(b.PreferredAction) ?? AgentActionType.Rest)).ToList());

            return new PersonAgent(
                id,
                householdId,
                dto.DisplayName,
                dto.Age,
                Enum.Parse<LifeStage>(dto.LifeStage, true),
                employment,
                new Money(dto.Income),
                new Money(dto.Expenses),
                new Money(dto.Balance),
                string.IsNullOrWhiteSpace(dto.ResidencePlotId) ? null : new PlotId(Guid.Parse(dto.ResidencePlotId)),
                new GridPosition(dto.CurrentLocation.X, dto.CurrentLocation.Y),
                Enum.Parse<AgentActivity>(dto.CurrentActivity, true),
                Enum.Parse<TransportPreference>(dto.TransportPreference, true),
                dto.Satisfaction,
                new AgentAttributeProfile(dto.Attributes.Health, dto.Attributes.Energy, dto.Attributes.Sociability, dto.Attributes.Diligence),
                needs,
                skills,
                relationships,
                goals,
                MapRoutine(dto.Routine));
        }

        private static RoutinePlan MapRoutine(RoutinePlanDto dto)
        {
            if (dto is null) return RoutinePlan.Empty;
            AgentActionType? ToAction(string? s) => string.IsNullOrWhiteSpace(s) ? null : Enum.TryParse<AgentActionType>(s, true, out var a) ? a : null;
                        var blocks = dto.Blocks.Select(b => new RoutineBlock(b.StartHour, b.EndHour, ToAction(b.PreferredAction) ?? AgentActionType.Rest)).ToList();
            return new RoutinePlan(blocks);
        }

        private static HouseholdAgent MapHousehold(HouseholdDto dto)
        {
            var members = dto.Members.Select(s => PersonIdFromString(s)).ToList().AsReadOnly();
            PlotId? homePlotId = string.IsNullOrWhiteSpace(dto.HomePlotId) ? null : new PlotId(Guid.Parse(dto.HomePlotId));
            GridPosition? homeLocation = dto.HomeLocation is null ? null : new GridPosition(dto.HomeLocation.X, dto.HomeLocation.Y);
            return new HouseholdAgent(
                HouseholdIdFromString(dto.Id),
                homePlotId,
                homeLocation,
                members,
                new Money(dto.Income),
                new Money(dto.Expenses),
                new Money(dto.Savings),
                new Money(dto.Debt),
                new Quantity(dto.FoodDemand),
                new Quantity(dto.UtilityDemand),
                dto.TransportAssets,
                dto.Satisfaction);
        }

        private static CommuteTrip MapTrip(CommuteTripDto dto)
        {
            return new CommuteTrip(PersonIdFromString(dto.PersonId), new GridPosition(dto.Origin.X, dto.Origin.Y), new GridPosition(dto.Destination.X, dto.Destination.Y), new SimulationTime(dto.DepartureTick), new SimulationTime(dto.ArrivalTick), Enum.Parse<TransportMode>(dto.Mode, true), dto.Purpose);
        }

        private static PersonId PersonIdFromString(string s) => new(Guid.Parse(s));
        private static HouseholdId HouseholdIdFromString(string s) => new(Guid.Parse(s));
        private static BuildingId BuildingIdFromString(string s) => new(Guid.Parse(s));

        private static PersonDto ToPersonDto(PersonAgent p)
        {
            return new PersonDto(p.Id.ToString(), p.HouseholdId.ToString(), p.DisplayName, p.Age, p.LifeStage.ToString(),
                new EmploymentDto(p.Employment.State.ToString(), p.Employment.WorkplaceId?.ToString(), p.Employment.WorkplaceLocation is null ? null : new GridPositionDto(p.Employment.WorkplaceLocation.Value.X, p.Employment.WorkplaceLocation.Value.Y), p.Employment.Wage.Amount),
                p.Income.Amount, p.Expenses.Amount, p.Balance.Amount,
                p.ResidencePlotId?.ToString(), new GridPositionDto(p.CurrentLocation.X, p.CurrentLocation.Y), p.CurrentActivity.ToString(), p.TransportPreference.ToString(), p.Satisfaction,
                new AgentAttributeProfileDto(p.Attributes.Health, p.Attributes.Energy, p.Attributes.Sociability, p.Attributes.Diligence),
                p.Needs.Select(n => new NeedStatusDto(n.Need.ToString(), n.Fulfillment)).ToList(),
                p.Skills.Select(s => new SkillRatingDto(s.Name, s.Level)).ToList(),
                p.Relationships.Select(r => new RelationshipLinkDto(r.PersonId.ToString(), r.RelationshipType.ToString(), r.Strength)).ToList(),
                p.Goals.Select(g => new AgentGoalDto(g.GoalType.ToString(), g.Description, g.Priority, g.DueBy?.Tick)).ToList(),
                new RoutinePlanDto(p.Routine.Blocks.Select(b => new RoutineBlockDto(b.StartHour, b.EndHour, b.PreferredAction.ToString())).ToList()));
        }

        private static HouseholdDto ToHouseholdDto(HouseholdAgent h)
        {
            return new HouseholdDto(h.Id.ToString(), h.HomePlotId?.ToString(), h.HomeLocation is null ? null : new GridPositionDto(h.HomeLocation.Value.X, h.HomeLocation.Value.Y), h.Members.Select(m => m.ToString()).ToList(),
                h.Income.Amount, h.Expenses.Amount, h.Savings.Amount, h.Debt.Amount, h.FoodDemand.Value, h.UtilityDemand.Value, h.TransportAssets, h.Satisfaction);
        }

        private static CommuteTripDto ToTripDto(CommuteTrip t)
        {
            return new CommuteTripDto(t.PersonId.ToString(), new GridPositionDto(t.Origin.X, t.Origin.Y), new GridPositionDto(t.Destination.X, t.Destination.Y), t.DepartureTime.Tick, t.ArrivalTime.Tick, t.Mode.ToString(), t.Purpose);
        }
    }
}
