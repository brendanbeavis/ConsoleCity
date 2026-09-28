using ConsoleCity.Core;

namespace ConsoleCity.Services;

public sealed record class ServiceCoverage
{
    public ServiceType ServiceType { get; }

    public GridPosition Location { get; }

    public double Radius { get; }

    public bool IsCovered { get; }

    public double ResponseTimeHours { get; }

    public ServiceCoverage(ServiceType serviceType, GridPosition location, double radius, bool isCovered, double responseTimeHours)
    {
        if (!double.IsFinite(radius) || radius < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(radius));
        }

        if (!double.IsFinite(responseTimeHours) || responseTimeHours < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(responseTimeHours));
        }

        ServiceType = serviceType;
        Location = location;
        Radius = radius;
        IsCovered = isCovered;
        ResponseTimeHours = responseTimeHours;
    }
}
