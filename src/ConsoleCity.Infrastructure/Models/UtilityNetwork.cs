namespace ConsoleCity.Infrastructure;

public sealed record class UtilityNetwork
{
    public UtilityType UtilityType { get; }

    public IReadOnlyList<UtilityNode> Nodes { get; }

    public IReadOnlyList<UtilityEdge> Edges { get; }

    public decimal TotalDemand { get; }

    public decimal TotalSupply { get; }

    public decimal Shortage => Math.Max(0m, TotalDemand - TotalSupply);

    public UtilityNetwork(
        UtilityType utilityType,
        IReadOnlyList<UtilityNode> nodes,
        IReadOnlyList<UtilityEdge> edges,
        decimal totalDemand,
        decimal totalSupply)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);
        if (totalDemand < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(totalDemand));
        }

        if (totalSupply < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(totalSupply));
        }

        UtilityType = utilityType;
        Nodes = nodes;
        Edges = edges;
        TotalDemand = totalDemand;
        TotalSupply = totalSupply;
    }

    public UtilityNode? GetNode(string nodeId) => Nodes.FirstOrDefault(node => node.Id == nodeId);

    public UtilityEdge? GetEdge(string edgeId) => Edges.FirstOrDefault(edge => edge.Id == edgeId);

    public IEnumerable<UtilityEdge> GetOutgoingEdges(string nodeId) => Edges.Where(edge => edge.FromNodeId == nodeId);

    public UtilityNetwork WithNode(string nodeId, Func<UtilityNode, UtilityNode> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);
        var nodes = Nodes.Select(node => node.Id == nodeId ? updater(node) : node).ToList();
        return new UtilityNetwork(UtilityType, nodes, Edges, TotalDemand, TotalSupply);
    }

    public UtilityNetwork WithEdge(string edgeId, Func<UtilityEdge, UtilityEdge> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);
        var edges = Edges.Select(edge => edge.Id == edgeId ? updater(edge) : edge).ToList();
        return new UtilityNetwork(UtilityType, Nodes, edges, TotalDemand, TotalSupply);
    }

    public UtilityNetwork WithMetrics(decimal totalDemand, decimal totalSupply)
        => new(UtilityType, Nodes, Edges, totalDemand, totalSupply);

    public static UtilityNetwork Empty(UtilityType utilityType) => new(utilityType, Array.Empty<UtilityNode>(), Array.Empty<UtilityEdge>(), 0m, 0m);
}
