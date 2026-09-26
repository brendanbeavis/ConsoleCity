namespace ConsoleCity.Core;

public readonly record struct RegionId(Guid Value)
{
    public static RegionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
