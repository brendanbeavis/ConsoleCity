using ConsoleCity.Core;

namespace ConsoleCity.Agents;

public sealed record EmploymentRecord
{
    public EmploymentState State { get; }

    public OrganizationId? WorkplaceId { get; }

    public GridPosition? WorkplaceLocation { get; }

    public Money Wage { get; }

    public EmploymentRecord(EmploymentState state, OrganizationId? workplaceId = null, GridPosition? workplaceLocation = null, Money? wage = null)
    {
        State = state;
        WorkplaceId = workplaceId;
        WorkplaceLocation = workplaceLocation;
        Wage = wage ?? Money.Zero;
    }

    public static EmploymentRecord Unemployed => new(EmploymentState.Unemployed);
}
