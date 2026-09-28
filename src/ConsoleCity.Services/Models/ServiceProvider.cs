using ConsoleCity.Core;

namespace ConsoleCity.Services;

public sealed record class ServiceProvider
{
    public string Id { get; }

    public ServiceType ServiceType { get; }

    public BuildingId FacilityId { get; }

    public GridPosition Location { get; }

    public int Capacity { get; }

    public int Staff { get; }

    public double Quality { get; }

    public double CoverageRadius { get; }

    public bool IsOperational { get; }

    public ServiceProvider(string id, ServiceType serviceType, BuildingId facilityId, GridPosition location, int capacity, int staff, double quality, double coverageRadius, bool isOperational)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        if (staff < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(staff));
        }

        if (!double.IsFinite(quality) || quality is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(quality));
        }

        if (!double.IsFinite(coverageRadius) || coverageRadius < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(coverageRadius));
        }

        Id = id;
        ServiceType = serviceType;
        FacilityId = facilityId;
        Location = location;
        Capacity = capacity;
        Staff = staff;
        Quality = quality;
        CoverageRadius = coverageRadius;
        IsOperational = isOperational;
    }
}
