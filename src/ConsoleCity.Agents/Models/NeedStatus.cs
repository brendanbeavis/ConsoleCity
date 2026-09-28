namespace ConsoleCity.Agents;

public sealed record NeedStatus
{
    public NeedType Need { get; }

    public double Fulfillment { get; }

    public double Pressure => 1d - Fulfillment;

    public NeedStatus(NeedType need, double fulfillment)
    {
        if (!double.IsFinite(fulfillment) || fulfillment is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(fulfillment), "Need fulfillment must be a finite fraction between 0 and 1.");
        }

        Need = need;
        Fulfillment = fulfillment;
    }
}
