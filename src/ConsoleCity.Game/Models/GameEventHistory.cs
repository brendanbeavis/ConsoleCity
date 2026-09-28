namespace ConsoleCity.Game;

public sealed record class GameEventHistory(IReadOnlyList<GameEventRecord> Events)
{
    public static GameEventHistory Empty { get; } = new(Array.Empty<GameEventRecord>());

    public GameEventHistory AddRange(IEnumerable<GameEventRecord> events)
    {
        var updated = Events.Concat(events).OrderBy(entry => entry.OccurredAt.Tick).ToList();
        const int maxEntries = 200;
        return updated.Count <= maxEntries ? new GameEventHistory(updated) : new GameEventHistory(updated.Skip(updated.Count - maxEntries).ToList());
    }
}