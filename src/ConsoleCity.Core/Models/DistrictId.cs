namespace ConsoleCity.Core;

public readonly record struct DistrictId(Guid Value)
{
    public static DistrictId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
