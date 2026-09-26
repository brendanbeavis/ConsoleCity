namespace ConsoleCity.Agents;

public sealed record AgentTransitionResult(
    PersonAgent Person,
    HouseholdAgent Household,
    string Effect);
