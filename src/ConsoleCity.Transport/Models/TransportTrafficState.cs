using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportTrafficState
{
    public IReadOnlyList<TransportLinkTrafficState> Links { get; }

    public IReadOnlyList<TransportJourney> Journeys { get; }

    public TransportTrafficState(IReadOnlyList<TransportLinkTrafficState> links, IReadOnlyList<TransportJourney> journeys)
    {
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(journeys);
        Links = links;
        Journeys = journeys;
    }

    public static TransportTrafficState Empty { get; } = new(Array.Empty<TransportLinkTrafficState>(), Array.Empty<TransportJourney>());

    public TransportLinkTrafficState GetLinkState(EntityId linkId)
        => Links.FirstOrDefault(link => link.LinkId == linkId) ?? new TransportLinkTrafficState(linkId, 0d, 0d);
}
