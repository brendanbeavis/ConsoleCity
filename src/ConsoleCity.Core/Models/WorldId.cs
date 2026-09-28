namespace ConsoleCity.Core;

public readonly record struct WorldId(Guid Value)
{
    public static WorldId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
