using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportNetwork
{
    public IReadOnlyList<TransportNode> Nodes { get; }

    public IReadOnlyList<TransportIntersection> Intersections { get; }

    public IReadOnlyList<TransportLink> Links { get; }

    public TransportNetwork(IReadOnlyList<TransportNode> nodes, IReadOnlyList<TransportIntersection> intersections, IReadOnlyList<TransportLink> links)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(intersections);
        ArgumentNullException.ThrowIfNull(links);
        Nodes = nodes;
        Intersections = intersections;
        Links = links;
    }

    public static TransportNetwork Empty { get; } = new(Array.Empty<TransportNode>(), Array.Empty<TransportIntersection>(), Array.Empty<TransportLink>());

    public TransportNode? GetNode(EntityId nodeId) => Nodes.FirstOrDefault(node => node.Id == nodeId);

    public IEnumerable<TransportLink> GetOutgoingLinks(EntityId nodeId) => Links.Where(link => link.FromNodeId == nodeId);

    public TransportLink? GetLink(EntityId linkId) => Links.FirstOrDefault(link => link.Id == linkId);
}
