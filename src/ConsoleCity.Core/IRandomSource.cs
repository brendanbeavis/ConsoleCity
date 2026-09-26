namespace ConsoleCity.Core;

public interface IRandomSource
{
    int Next(int minInclusive, int maxExclusive);
    double NextDouble();
}
