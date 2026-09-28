using ConsoleCity.Core;

namespace ConsoleCity.Services;

public sealed record class ServiceDemand
{
    public EntityId RequesterId { get; }

    public ServiceType ServiceType { get; }

    public GridPosition Location { get; }

    public int Amount { get; }

    public double Urgency { get; }

    public ServiceDemand(EntityId requesterId, ServiceType serviceType, GridPosition location, int amount, double urgency)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        if (!double.IsFinite(urgency) || urgency is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(urgency));
        }

        RequesterId = requesterId;
        ServiceType = serviceType;
        Location = location;
        Amount = amount;
        Urgency = urgency;
    }
}
