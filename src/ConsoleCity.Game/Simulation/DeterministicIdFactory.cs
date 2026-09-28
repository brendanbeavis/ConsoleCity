using System.Security.Cryptography;
using System.Text;
using ConsoleCity.Core;

namespace ConsoleCity.Game;

internal static class DeterministicIdFactory
{
    public static WorldId World(int seed, int index = 0) => new(CreateGuid(seed, nameof(WorldId), index));

    public static RegionId Region(int seed, int index) => new(CreateGuid(seed, nameof(RegionId), index));

    public static CityId City(int seed, int index) => new(CreateGuid(seed, nameof(CityId), index));

    public static DistrictId District(int seed, int index) => new(CreateGuid(seed, nameof(DistrictId), index));

    public static PlotId Plot(int seed, int index) => new(CreateGuid(seed, nameof(PlotId), index));

    public static BuildingId Building(int seed, int index) => new(CreateGuid(seed, nameof(BuildingId), index));

    public static HouseholdId Household(int seed, int index) => new(CreateGuid(seed, nameof(HouseholdId), index));

    public static PersonId Person(int seed, int index) => new(CreateGuid(seed, nameof(PersonId), index));

    public static OrganizationId Organization(int seed, int index) => new(CreateGuid(seed, nameof(OrganizationId), index));

    private static Guid CreateGuid(int seed, string scope, int index)
    {
        var bytes = Encoding.UTF8.GetBytes($"{seed}:{scope}:{index}");
        var hash = SHA256.HashData(bytes);
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(guidBytes);
        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x40);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);
        return new Guid(guidBytes);
    }
}
