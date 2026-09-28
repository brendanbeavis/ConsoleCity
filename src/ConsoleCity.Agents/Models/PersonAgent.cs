using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public sealed record class PersonAgent
{
    public PersonId Id { get; }

    public HouseholdId HouseholdId { get; }

    public string DisplayName { get; }

    public int Age { get; }

    public LifeStage LifeStage { get; }

    public EmploymentRecord Employment { get; }

    public Money Income { get; }

    public Money Expenses { get; }

    public Money Balance { get; }

    public PlotId? ResidencePlotId { get; }

    public GridPosition CurrentLocation { get; }

    public AgentActivity CurrentActivity { get; }

    public TransportPreference TransportPreference { get; }

    public double Satisfaction { get; }

    public AgentAttributeProfile Attributes { get; }

    public IReadOnlyList<NeedStatus> Needs { get; }

    public IReadOnlyList<SkillRating> Skills { get; }

    public IReadOnlyList<RelationshipLink> Relationships { get; }

    public IReadOnlyList<AgentGoal> Goals { get; }

    public RoutinePlan Routine { get; }

    public EmploymentState EmploymentState => Employment.State;

    public OrganizationId? WorkplaceId => Employment.WorkplaceId;

    public GridPosition? WorkplaceLocation => Employment.WorkplaceLocation;

    public NeedStatus GetNeed(NeedType needType)
        => Needs.FirstOrDefault(need => need.Need == needType) ?? new NeedStatus(needType, 1d);

    public double GetNeedPressure(NeedType needType) => GetNeed(needType).Pressure;

    public PersonAgent(
        PersonId id,
        HouseholdId householdId,
        string displayName,
        int age,
        LifeStage lifeStage,
        EmploymentRecord employment,
        Money income,
        Money expenses,
        Money balance,
        PlotId? residencePlotId,
        GridPosition currentLocation,
        AgentActivity currentActivity,
        TransportPreference transportPreference,
        double satisfaction,
        AgentAttributeProfile attributes,
        IReadOnlyList<NeedStatus> needs,
        IReadOnlyList<SkillRating> skills,
        IReadOnlyList<RelationshipLink> relationships,
        IReadOnlyList<AgentGoal> goals,
        RoutinePlan routine)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));
        }

        if (age < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(age));
        }

        if (!double.IsFinite(satisfaction) || satisfaction is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(satisfaction), "Satisfaction must be a finite fraction between 0 and 1.");
        }

        ArgumentNullException.ThrowIfNull(employment);
        ArgumentNullException.ThrowIfNull(needs);
        ArgumentNullException.ThrowIfNull(skills);
        ArgumentNullException.ThrowIfNull(relationships);
        ArgumentNullException.ThrowIfNull(goals);
        ArgumentNullException.ThrowIfNull(routine);

        Id = id;
        HouseholdId = householdId;
        DisplayName = displayName.Trim();
        Age = age;
        LifeStage = lifeStage;
        Employment = employment;
        Income = income;
        Expenses = expenses;
        Balance = balance;
        ResidencePlotId = residencePlotId;
        CurrentLocation = currentLocation;
        CurrentActivity = currentActivity;
        TransportPreference = transportPreference;
        Satisfaction = satisfaction;
        Attributes = attributes;
        Needs = needs;
        Skills = skills;
        Relationships = relationships;
        Goals = goals;
        Routine = routine;
    }
}
