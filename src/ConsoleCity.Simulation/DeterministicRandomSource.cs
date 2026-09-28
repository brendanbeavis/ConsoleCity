using ConsoleCity.Core;

namespace ConsoleCity.Simulation;

public sealed class DeterministicRandomSource : IRandomSource
{
    private readonly Random random;

    public DeterministicRandomSource(long seed)
    {
        random = new Random(checked((int)(seed & int.MaxValue)));
    }

    public int Next(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);

    public double NextDouble() => random.NextDouble();
}
