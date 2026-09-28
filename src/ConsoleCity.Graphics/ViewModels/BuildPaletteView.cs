using ConsoleCity.World;

namespace ConsoleCity.Graphics.ViewModels;

public sealed record class BuildPaletteView
{
    public bool IsActive { get; }

    public BuildingType? SelectedBuildingType { get; }

    public IReadOnlyList<BuildOptionView> Options { get; }

    public BuildPaletteView(bool isActive, BuildingType? selectedBuildingType, IReadOnlyList<BuildOptionView> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        IsActive = isActive;
        SelectedBuildingType = selectedBuildingType;
        Options = options;
    }
}
