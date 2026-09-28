namespace ConsoleCity.Core;

public readonly record struct PlotId(Guid Value)
{
    public static PlotId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("N");
}
