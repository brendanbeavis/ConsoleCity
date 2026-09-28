using ConsoleCity.Core;
using ConsoleCity.World;

namespace ConsoleCity.Graphics.ViewModels;

public sealed record class SelectionView
{
    public string Title { get; }

    public ObjectType? ObjectType { get; }

    public string? ObjectId { get; }

    public GridPosition? Position { get; }

    public IReadOnlyList<string> Lines { get; }

    public SelectionView(string title, ObjectType? objectType, string? objectId, GridPosition? position, IReadOnlyList<string> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(lines);

        Title = title;
        ObjectType = objectType;
        ObjectId = objectId;
        Position = position;
        Lines = lines;
    }
}
