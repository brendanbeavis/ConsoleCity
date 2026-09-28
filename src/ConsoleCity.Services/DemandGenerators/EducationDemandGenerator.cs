using ConsoleCity.Core;

namespace ConsoleCity.Services;

/// <summary>
/// Generates education demand based on school-age population.
/// </summary>
public sealed class EducationDemandGenerator : IServiceDemandGenerator
{
    private readonly IRandomSource random;

    public EducationDemandGenerator(IRandomSource random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public IReadOnlyList<ServiceDemand> GenerateDemands(
        GridPosition location,
        SimulationTime currentTime,
        IServiceDemandContext context)
    {
        var demands = new List<ServiceDemand>();

        // Primary education demand
        int primaryStudents = (int)(context.GetStudentsAt(location) * 0.4d); // ~40% are primary age
        if (primaryStudents > 0)
        {
            // Add some daily variance (attendance)
            int attendance = Math.Max(0, primaryStudents + random.Next(-primaryStudents / 10, primaryStudents / 10));

            if (attendance > 0)
            {
                demands.Add(new ServiceDemand(
                    EntityId.New(),
                    ServiceType.PrimaryEducation,
                    location,
                    attendance,
                    0.5d)); // Moderate urgency - school enrollment
            }
        }

        // Secondary education demand
        int secondaryStudents = (int)(context.GetStudentsAt(location) * 0.4d); // ~40% are secondary age
        if (secondaryStudents > 0)
        {
            int attendance = Math.Max(0, secondaryStudents + random.Next(-secondaryStudents / 10, secondaryStudents / 10));

            if (attendance > 0)
            {
                demands.Add(new ServiceDemand(
                    EntityId.New(),
                    ServiceType.SecondaryEducation,
                    location,
                    attendance,
                    0.5d));
            }
        }

        // University demand
        int universityStudents = (int)(context.GetStudentsAt(location) * 0.2d); // ~20% are university age
        if (universityStudents > 0)
        {
            int attendance = Math.Max(0, universityStudents + random.Next(-universityStudents / 15, universityStudents / 15));

            if (attendance > 0)
            {
                demands.Add(new ServiceDemand(
                    EntityId.New(),
                    ServiceType.University,
                    location,
                    attendance,
                    0.4d)); // Slightly lower urgency than K-12
            }
        }

        return demands;
    }
}
