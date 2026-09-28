namespace ConsoleCity.Graphics.ViewModels;

public sealed record class ConstructionPanelView
{
    public IReadOnlyList<string> ActiveProjects { get; }

    public ConstructionPanelView(IReadOnlyList<string> activeProjects)
    {
        ArgumentNullException.ThrowIfNull(activeProjects);
        ActiveProjects = activeProjects;
    }
}
