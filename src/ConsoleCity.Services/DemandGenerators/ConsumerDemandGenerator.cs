using ConsoleCity.Core;

namespace ConsoleCity.Services;

/// <summary>
/// Generates retail demand based on household income and population.
/// </summary>
public sealed class RetailDemandGenerator : IServiceDemandGenerator
{
    private readonly IRandomSource random;

    public RetailDemandGenerator(IRandomSource random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public IReadOnlyList<ServiceDemand> GenerateDemands(
        GridPosition location,
        SimulationTime currentTime,
        IServiceDemandContext context)
    {
        var demands = new List<ServiceDemand>();

        int population = context.GetPopulationAt(location);
        if (population == 0)
        {
            return demands;
        }

        double averageIncome = context.GetAverageIncomeAt(location);

        // Base retail demand: ~10% of population shop per tick
        double baseRetailRate = 0.10d;

        // Income level affects retail demand (higher income = more spending)
        int baseDemand = (int)Math.Ceiling(population * baseRetailRate * (0.3d + (averageIncome * 0.7d)));

        // Add variance
        int variance = random.Next(-baseDemand / 4, baseDemand / 4);
        int totalDemand = Math.Max(0, baseDemand + variance);

        if (totalDemand > 0)
        {
            demands.Add(new ServiceDemand(
                EntityId.New(),
                ServiceType.Retail,
                location,
                totalDemand,
                0.3d)); // Low urgency - shopping can wait
        }

        return demands;
    }
}

/// <summary>
/// Generates recreation demand based on population and income.
/// </summary>
public sealed class RecreationDemandGenerator : IServiceDemandGenerator
{
    private readonly IRandomSource random;

    public RecreationDemandGenerator(IRandomSource random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public IReadOnlyList<ServiceDemand> GenerateDemands(
        GridPosition location,
        SimulationTime currentTime,
        IServiceDemandContext context)
    {
        var demands = new List<ServiceDemand>();

        int population = context.GetPopulationAt(location);
        if (population == 0)
        {
            return demands;
        }

        double averageIncome = context.GetAverageIncomeAt(location);

        // Base recreation demand: ~5% of population per tick
        double baseRecreationRate = 0.05d;

        // Income enables more recreation
        int baseDemand = (int)Math.Ceiling(population * baseRecreationRate * (0.2d + (averageIncome * 0.8d)));

        // Add variance
        int variance = random.Next(-baseDemand / 4, baseDemand / 4);
        int totalDemand = Math.Max(0, baseDemand + variance);

        if (totalDemand > 0)
        {
            demands.Add(new ServiceDemand(
                EntityId.New(),
                ServiceType.Recreation,
                location,
                totalDemand,
                0.2d)); // Very low urgency - recreation is optional
        }

        return demands;
    }
}

/// <summary>
/// Generates civic/government service demand based on population size.
/// </summary>
public sealed class CivicServiceDemandGenerator : IServiceDemandGenerator
{
    private readonly IRandomSource random;

    public CivicServiceDemandGenerator(IRandomSource random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public IReadOnlyList<ServiceDemand> GenerateDemands(
        GridPosition location,
        SimulationTime currentTime,
        IServiceDemandContext context)
    {
        var demands = new List<ServiceDemand>();

        int population = context.GetPopulationAt(location);
        if (population == 0)
        {
            return demands;
        }

        // Government services: ~2% of population per tick
        double govServiceRate = 0.02d;
        int baseDemand = (int)Math.Ceiling(population * govServiceRate);

        // Add variance
        int variance = random.Next(-baseDemand / 5, baseDemand / 5);
        int totalDemand = Math.Max(0, baseDemand + variance);

        if (totalDemand > 0)
        {
            demands.Add(new ServiceDemand(
                EntityId.New(),
                ServiceType.Government,
                location,
                totalDemand,
                0.4d)); // Moderate urgency
        }

        return demands;
    }
}
