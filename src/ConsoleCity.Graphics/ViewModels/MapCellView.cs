using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Graphics.ViewModels;

public sealed record class MapCellView
{
    public GridPosition Position { get; }

    public TerrainType Terrain { get; }

    public bool IsWater { get; }

    public bool HasRoad { get; }

    public bool HasRail { get; }

    public BuildingType? BuildingType { get; }

    public LandUseType? LandUse { get; }

    public bool IsSelected { get; }

    public bool IsHovered { get; }

    public MapCellView(
        GridPosition position,
        TerrainType terrain,
        bool isWater,
        bool hasRoad,
        bool hasRail,
        BuildingType? buildingType,
        LandUseType? landUse,
        bool isSelected,
        bool isHovered)
    {
        Position = position;
        Terrain = terrain;
        IsWater = isWater;
        HasRoad = hasRoad;
        HasRail = hasRail;
        BuildingType = buildingType;
        LandUse = landUse;
        IsSelected = isSelected;
        IsHovered = isHovered;
    }
}
