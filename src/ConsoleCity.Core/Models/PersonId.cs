namespace ConsoleCity.Core;

public readonly record struct PersonId(Guid Value)
{
    public static PersonId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
