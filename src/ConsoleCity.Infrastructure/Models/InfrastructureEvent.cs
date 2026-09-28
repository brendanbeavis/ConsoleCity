using ConsoleCity.Core;

namespace ConsoleCity.Infrastructure;

public sealed record class InfrastructureEvent
{
    public InfrastructureEventType Type { get; }

    public UtilityType UtilityType { get; }

    public string TargetId { get; }

    public SimulationTime CapturedAt { get; }

    public decimal Magnitude { get; }

    public string Message { get; }

    public InfrastructureEvent(
        InfrastructureEventType type,
        UtilityType utilityType,
        string targetId,
        SimulationTime capturedAt,
        decimal magnitude,
        string message)
    {
        ArgumentNullException.ThrowIfNull(targetId);
        ArgumentNullException.ThrowIfNull(message);
        Type = type;
        UtilityType = utilityType;
        TargetId = targetId;
        CapturedAt = capturedAt;
        Magnitude = magnitude;
        Message = message;
    }
}
