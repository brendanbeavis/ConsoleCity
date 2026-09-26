using ConsoleCity.Core;

namespace ConsoleCity.Infrastructure;

public sealed record class UtilityNode
{
    public string Id { get; }

    public UtilityType UtilityType { get; }

    public GridPosition Position { get; }

    public UtilityNodeRole Role { get; }

    public decimal Capacity { get; }

    public decimal Demand { get; }

    public decimal Supplied { get; }

    public bool IsOperational { get; }

    public UtilityNode(
        string id,
        UtilityType utilityType,
        GridPosition position,
        UtilityNodeRole role,
        decimal capacity,
        decimal demand,
        decimal supplied,
        bool isOperational)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (capacity < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        if (demand < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(demand));
        }

        if (supplied < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(supplied));
        }

        Id = id;
        UtilityType = utilityType;
        Position = position;
        Role = role;
        Capacity = capacity;
        Demand = demand;
        Supplied = supplied;
        IsOperational = isOperational;
    }
}
