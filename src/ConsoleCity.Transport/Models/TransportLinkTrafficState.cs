using ConsoleCity.Core;

namespace ConsoleCity.Transport;

public sealed record class TransportLinkTrafficState
{
    public EntityId LinkId { get; }

    public double Demand { get; }

    public double Congestion { get; }

    public TransportLinkTrafficState(EntityId linkId, double demand, double congestion)
    {
        if (!double.IsFinite(demand) || demand < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(demand));
        }

        if (!double.IsFinite(congestion) || congestion < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(congestion));
        }

        LinkId = linkId;
        Demand = demand;
        Congestion = congestion;
    }
}
