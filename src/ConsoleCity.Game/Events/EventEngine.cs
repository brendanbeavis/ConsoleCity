namespace ConsoleCity.Game;

public static class EventEngine
{
    public static IReadOnlyList<GameEventRecord> GenerateEvents(SimulationSliceSnapshot snapshot)
    {
        var events = new List<GameEventRecord>();

        var foodInventory = snapshot.Economy.Businesses.Sum(business => business.Inventory.Where(line => line.ResourceId.Value == "packaged-food").Sum(line => line.Quantity.Value));
        var population = snapshot.Population.People.Count;
        if (population > 0 && foodInventory < population * 2)
        {
            events.Add(new GameEventRecord(
                $"food-shortage-{snapshot.CurrentTime.Tick}",
                GameEventCategory.Economic,
                snapshot.CurrentTime,
                "Food supply is tightening.",
                0.6d,
                ["food inventory is low", "household demand remains steady"]));
        }

        if (snapshot.CurrentTime.Tick % 720 == 0)
        {
            events.Add(new GameEventRecord(
                $"cycle-marker-{snapshot.CurrentTime.Tick}",
                GameEventCategory.Cycle,
                snapshot.CurrentTime,
                $"Cycle milestone reached at tick {snapshot.CurrentTime.Tick}.",
                0.2d,
                ["cycle transition"]));
        }

        return events;
    }
}