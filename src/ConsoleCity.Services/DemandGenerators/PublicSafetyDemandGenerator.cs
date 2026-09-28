using ConsoleCity.Core;

namespace ConsoleCity.Services;

/// <summary>
/// Generates police demand based on crime levels and incidents.
/// </summary>
public sealed class PoliceDemandGenerator : IServiceDemandGenerator
{
    private readonly IRandomSource random;

    public PoliceDemandGenerator(IRandomSource random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public IReadOnlyList<ServiceDemand> GenerateDemands(
        GridPosition location,
        SimulationTime currentTime,
        IServiceDemandContext context)
    {
        var demands = new List<ServiceDemand>();

        // Routine police demand based on crime rate
        double crimeRate = context.GetCrimeRateAt(location);
        int population = context.GetPopulationAt(location);

        if (population > 0 && crimeRate > 0)
        {
            // Base demand: ~1% of population involved in crime incidents per tick
            int baseDemand = (int)Math.Ceiling(population * 0.01d * crimeRate);

            // Add variance
            int variance = random.Next(-baseDemand / 3, baseDemand / 3);
            int totalDemand = Math.Max(0, baseDemand + variance);

            if (totalDemand > 0)
            {
                demands.Add(new ServiceDemand(
                    EntityId.New(),
                    ServiceType.Police,
                    location,
                    totalDemand,
                    0.5d + (crimeRate * 0.3d))); // Higher urgency for higher crime
            }
        }

        // Immediate police response for active crime incidents
        if (context.HasCrimeAt(location))
        {
            demands.Add(new ServiceDemand(
                EntityId.New(),
                ServiceType.Police,
                location,
                1, // At least one unit needed
                0.95d)); // Very high urgency
        }

        return demands;
    }
}

/// <summary>
/// Generates fire department demand based on fire risk and active incidents.
/// </summary>
public sealed class FireDemandGenerator : IServiceDemandGenerator
{
    private readonly IRandomSource random;

    public FireDemandGenerator(IRandomSource random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public IReadOnlyList<ServiceDemand> GenerateDemands(
        GridPosition location,
        SimulationTime currentTime,
        IServiceDemandContext context)
    {
        var demands = new List<ServiceDemand>();

        // Immediate demand if there are active fires
        if (context.HasActiveFireAt(location))
        {
            demands.Add(new ServiceDemand(
                EntityId.New(),
                ServiceType.Fire,
                location,
                1, // Minimum one fire truck needed
                1.0d)); // Absolute highest urgency
        }

        return demands;
    }
}
