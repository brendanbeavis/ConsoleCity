namespace ConsoleCity.Infrastructure;

public sealed record class UtilityEdge
{
    public string Id { get; }

    public string FromNodeId { get; }

    public string ToNodeId { get; }

    public decimal Capacity { get; }

    public decimal Flow { get; }

    public bool IsOperational { get; }

    public UtilityEdge(string id, string fromNodeId, string toNodeId, decimal capacity, decimal flow, bool isOperational)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(fromNodeId);
        ArgumentNullException.ThrowIfNull(toNodeId);
        if (capacity < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        if (flow < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(flow));
        }

        Id = id;
        FromNodeId = fromNodeId;
        ToNodeId = toNodeId;
        Capacity = capacity;
        Flow = flow;
        IsOperational = isOperational;
    }
}
