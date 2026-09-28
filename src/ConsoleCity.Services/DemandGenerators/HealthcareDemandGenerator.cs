using ConsoleCity.Core;

namespace ConsoleCity.Services;

/// <summary>
/// Generates healthcare service demand based on population health status.
/// </summary>
public sealed class HealthcareDemandGenerator : IServiceDemandGenerator
{
    private readonly IRandomSource random;

    public HealthcareDemandGenerator(IRandomSource random)
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

        // Base healthcare demand: ~5% of population per tick needs some healthcare
        double baseHealthcareDemandRate = 0.05d;
        double healthStatusMultiplier = context.GetHealthStatusAt(location);

        // Higher health status means more demand (more sick people)
        int baseDemand = (int)Math.Ceiling(population * baseHealthcareDemandRate * (0.5d + healthStatusMultiplier));

        // Add some randomness
        int variance = random.Next(-baseDemand / 4, baseDemand / 4);
        int totalDemand = Math.Max(0, baseDemand + variance);

        if (totalDemand > 0)
        {
            // Split between clinic and hospital based on severity
            int clinicDemand = (int)(totalDemand * 0.7d); // 70% clinic/GP
            int hospitalDemand = totalDemand - clinicDemand; // 30% hospital

            if (clinicDemand > 0)
            {
                demands.Add(new ServiceDemand(
                    EntityId.New(),
                    ServiceType.Hospital, // GP clinics are tracked under Hospital type with different category
                    location,
                    clinicDemand,
                    0.3d)); // Moderate urgency for routine care
            }

            if (hospitalDemand > 0)
            {
                demands.Add(new ServiceDemand(
                    EntityId.New(),
                    ServiceType.Hospital,
                    location,
                    hospitalDemand,
                    0.7d)); // High urgency for hospital admission
            }
        }

        return demands;
    }
}

/// <summary>
/// Generates ambulance/emergency medical demand for critical health incidents.
/// </summary>
public sealed class AmbulanceDemandGenerator : IServiceDemandGenerator
{
    private readonly IRandomSource random;

    public AmbulanceDemandGenerator(IRandomSource random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public IReadOnlyList<ServiceDemand> GenerateDemands(
        GridPosition location,
        SimulationTime currentTime,
        IServiceDemandContext context)
    {
        var demands = new List<ServiceDemand>();

        // Emergency ambulance demand occurs when health status is critical
        double healthStatus = context.GetHealthStatusAt(location);
        if (healthStatus > 0.8d) // Only for severe health conditions
        {
            int population = context.GetPopulationAt(location);
            if (population == 0)
            {
                return demands;
            }

            // ~2% of population in critical health status need ambulance
            int demand = (int)Math.Ceiling(population * 0.02d * healthStatus);

            if (demand > 0)
            {
                demands.Add(new ServiceDemand(
                    EntityId.New(),
                    ServiceType.Ambulance,
                    location,
                    demand,
                    1.0d)); // Highest urgency
            }
        }

        return demands;
    }
}
