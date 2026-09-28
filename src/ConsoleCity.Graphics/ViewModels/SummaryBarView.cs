namespace ConsoleCity.Graphics.ViewModels;

public sealed record class SummaryBarView
{
    public IReadOnlyList<string> Items { get; }

    public SummaryBarView(IReadOnlyList<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Items = items;
    }
}
