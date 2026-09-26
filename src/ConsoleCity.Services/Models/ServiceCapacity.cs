namespace ConsoleCity.Services;

public sealed record class ServiceCapacity
{
    public int Available { get; }

    public int Demand { get; }

    public int Shortage => Math.Max(0, Demand - Available);

    public double Utilization => Available <= 0 ? 0d : Math.Min(1d, (double)Demand / Available);

    public ServiceCapacity(int available, int demand)
    {
        if (available < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(available));
        }

        if (demand < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(demand));
        }

        Available = available;
        Demand = demand;
    }
}
