namespace ConsoleCity.Core;

public readonly record struct RoadId(Guid Value)
{
    public static RoadId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
