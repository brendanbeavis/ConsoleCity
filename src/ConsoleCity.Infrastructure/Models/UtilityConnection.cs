namespace ConsoleCity.Infrastructure;

public sealed record class UtilityConnection
{
    public string ConsumerId { get; }
    public string NodeId { get; }

    public UtilityConnection(string consumerId, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(consumerId);
        ArgumentNullException.ThrowIfNull(nodeId);
        ConsumerId = consumerId;
        NodeId = nodeId;
    }
}
