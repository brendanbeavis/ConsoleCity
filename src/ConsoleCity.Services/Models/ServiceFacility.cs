using ConsoleCity.Core;

namespace ConsoleCity.Services;

public sealed record class ServiceFacility
{
    public BuildingId BuildingId { get; }

    public ServiceType ServiceType { get; }

    public int Capacity { get; }

    public int StaffAssigned { get; }

    public double Quality { get; }

    public double CoverageRadius { get; }

    public ServiceFacility(BuildingId buildingId, ServiceType serviceType, int capacity, int staffAssigned, double quality, double coverageRadius)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        if (staffAssigned < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(staffAssigned));
        }

        if (!double.IsFinite(quality) || quality is < 0d or > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(quality));
        }

        if (!double.IsFinite(coverageRadius) || coverageRadius < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(coverageRadius));
        }

        BuildingId = buildingId;
        ServiceType = serviceType;
        Capacity = capacity;
        StaffAssigned = staffAssigned;
        Quality = quality;
        CoverageRadius = coverageRadius;
    }
}
