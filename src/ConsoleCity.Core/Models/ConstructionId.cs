namespace ConsoleCity.Core;

public readonly record struct ConstructionId(Guid Value)
{
    public static ConstructionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
