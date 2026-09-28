namespace ConsoleCity.Transport;

public readonly record struct TransportCapacity
{
    public double Limit { get; }

    public double Used { get; }

    public double Available => Math.Max(0d, Limit - Used);

    public TransportCapacity(double limit, double used)
    {
        if (!double.IsFinite(limit) || limit < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(limit));
        }

        if (!double.IsFinite(used) || used < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(used));
        }

        Limit = limit;
        Used = Math.Min(used, limit);
    }

    public TransportCapacity WithUsage(double used) => new(Limit, used);
}
