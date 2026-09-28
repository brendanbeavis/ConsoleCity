using ConsoleCity.World;

namespace ConsoleCity.Graphics.ViewModels;

public sealed record class BuildOptionView
{
    public BuildingType BuildingType { get; }

    public bool IsSelected { get; }

    public BuildOptionView(BuildingType buildingType, bool isSelected)
    {
        BuildingType = buildingType;
        IsSelected = isSelected;
    }
}
