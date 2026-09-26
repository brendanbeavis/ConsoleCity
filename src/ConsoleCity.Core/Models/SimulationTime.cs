namespace ConsoleCity.Core;

public readonly record struct SimulationTime
{
    public const int TicksPerDay = 24;

    public long Tick { get; }

    public SimulationTime(long tick)
    {
        if (tick < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tick), "Simulation time cannot be negative.");
        }

        Tick = tick;
    }

    public int Hour => (int)(Tick % TicksPerDay);
    public int Day => (int)(Tick / TicksPerDay);

    public SimulationTime Advance(long ticks = 1) => new(checked(Tick + ticks));

    public GameDateTime ToGameDateTime() => GameDateTime.FromSimulationTime(this);

    public static SimulationTime FromGameDateTime(GameDateTime dateTime) => dateTime.ToSimulationTime();

    public override string ToString() => Tick.ToString();
}
