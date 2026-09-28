using ConsoleCity.Core;
using ConsoleCity.Services;
using ConsoleCity.Simulation;

namespace ConsoleCity.Services.Tests;

public class DemandGeneratorTests
{
    private readonly DeterministicRandomSource random = new(42);

    [Fact]
    public void HealthcareDemandGenerator_GeneratesDemandForPopulation()
    {
        var generator = new HealthcareDemandGenerator(random);
        var context = CreateMockContext(population: 1000, healthStatus: 0.5d);
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.NotEmpty(demands);
        Assert.All(demands, d => Assert.Equal(ServiceType.Hospital, d.ServiceType));
    }

    [Fact]
    public void HealthcareDemandGenerator_NoPopulation_EmptyDemands()
    {
        var generator = new HealthcareDemandGenerator(random);
        var context = CreateMockContext(population: 0);
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.Empty(demands);
    }

    [Fact]
    public void HealthcareDemandGenerator_HighHealthStatus_IncreasesDemand()
    {
        var generator = new HealthcareDemandGenerator(random);
        var context1 = CreateMockContext(population: 1000, healthStatus: 0.2d);
        var context2 = CreateMockContext(population: 1000, healthStatus: 0.8d);
        var location = new GridPosition(0, 0);

        var demands1 = generator.GenerateDemands(location, new SimulationTime(0), context1);
        var demands2 = generator.GenerateDemands(location, new SimulationTime(0), context2);

        int totalDemand1 = demands1.Sum(d => d.Amount);
        int totalDemand2 = demands2.Sum(d => d.Amount);

        // Higher health status should generate more demand
        Assert.True(totalDemand2 > totalDemand1);
    }

    [Fact]
    public void AmbulanceDemandGenerator_CriticalHealthStatus_GeneratesEmergencyDemand()
    {
        var generator = new AmbulanceDemandGenerator(random);
        var context = CreateMockContext(population: 1000, healthStatus: 0.9d); // Critical
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.NotEmpty(demands);
        Assert.Single(demands);
        Assert.Equal(1.0d, demands[0].Urgency); // Highest urgency
    }

    [Fact]
    public void AmbulanceDemandGenerator_MildHealthStatus_NoDemand()
    {
        var generator = new AmbulanceDemandGenerator(random);
        var context = CreateMockContext(population: 1000, healthStatus: 0.3d); // Not critical
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.Empty(demands);
    }

    [Fact]
    public void EducationDemandGenerator_GeneratesDemandForStudents()
    {
        var generator = new EducationDemandGenerator(random);
        var context = CreateMockContext(students: 500);
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.NotEmpty(demands);
        Assert.Contains(demands, d => d.ServiceType == ServiceType.PrimaryEducation);
        Assert.Contains(demands, d => d.ServiceType == ServiceType.SecondaryEducation);
    }

    [Fact]
    public void EducationDemandGenerator_NoStudents_EmptyDemands()
    {
        var generator = new EducationDemandGenerator(random);
        var context = CreateMockContext(students: 0);
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.Empty(demands);
    }

    [Fact]
    public void PoliceDemandGenerator_HighCrimeRate_IncreasesDemand()
    {
        var generator = new PoliceDemandGenerator(random);
        var context1 = CreateMockContext(population: 1000, crimeRate: 0.1d);
        var context2 = CreateMockContext(population: 1000, crimeRate: 0.8d);
        var location = new GridPosition(0, 0);

        var demands1 = generator.GenerateDemands(location, new SimulationTime(0), context1);
        var demands2 = generator.GenerateDemands(location, new SimulationTime(0), context2);

        int totalDemand1 = demands1.Sum(d => d.Amount);
        int totalDemand2 = demands2.Sum(d => d.Amount);

        Assert.True(totalDemand2 > totalDemand1);
    }

    [Fact]
    public void PoliceDemandGenerator_ActiveCrime_GeneratesUrgentDemand()
    {
        var generator = new PoliceDemandGenerator(random);
        var context = CreateMockContext(population: 1000, crimeRate: 0.5d, hasActiveCrime: true);
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.NotEmpty(demands);
        var urgentDemand = demands.FirstOrDefault(d => d.Urgency > 0.9d);
        Assert.NotNull(urgentDemand);
    }

    [Fact]
    public void FireDemandGenerator_ActiveFire_GeneratesUrgentDemand()
    {
        var generator = new FireDemandGenerator(random);
        var context = CreateMockContext(hasActiveFire: true);
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.Single(demands);
        Assert.Equal(1.0d, demands[0].Urgency); // Absolute highest urgency
    }

    [Fact]
    public void FireDemandGenerator_NoActiveFire_NoDemand()
    {
        var generator = new FireDemandGenerator(random);
        var context = CreateMockContext(hasActiveFire: false);
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.Empty(demands);
    }

    [Fact]
    public void RetailDemandGenerator_GeneratesDemandForPopulation()
    {
        var generator = new RetailDemandGenerator(random);
        var context = CreateMockContext(population: 1000, averageIncome: 0.6d);
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.Single(demands);
        Assert.Equal(ServiceType.Retail, demands[0].ServiceType);
    }

    [Fact]
    public void RetailDemandGenerator_HigherIncome_IncreasesDemand()
    {
        var generator = new RetailDemandGenerator(random);
        var context1 = CreateMockContext(population: 1000, averageIncome: 0.2d);
        var context2 = CreateMockContext(population: 1000, averageIncome: 0.9d);
        var location = new GridPosition(0, 0);

        var demands1 = generator.GenerateDemands(location, new SimulationTime(0), context1);
        var demands2 = generator.GenerateDemands(location, new SimulationTime(0), context2);

        int totalDemand1 = demands1.Sum(d => d.Amount);
        int totalDemand2 = demands2.Sum(d => d.Amount);

        Assert.True(totalDemand2 > totalDemand1);
    }

    [Fact]
    public void RecreationDemandGenerator_GeneratesDemandForPopulation()
    {
        var generator = new RecreationDemandGenerator(random);
        var context = CreateMockContext(population: 1000, averageIncome: 0.5d);
        var location = new GridPosition(0, 0);

        var demands = generator.GenerateDemands(location, new SimulationTime(0), context);

        Assert.Single(demands);
        Assert.Equal(ServiceType.Recreation, demands[0].ServiceType);
        Assert.True(demands[0].Urgency < 0.3d); // Low urgency
    }

    // Helper methods
    private MockServiceDemandContext CreateMockContext(
        int population = 0,
        int students = 0,
        double crimeRate = 0d,
        double fireRisk = 0d,
        double healthStatus = 0d,
        double averageIncome = 0.5d,
        bool hasActiveFire = false,
        bool hasActiveCrime = false)
    {
        return new MockServiceDemandContext
        {
            Population = population,
            Students = students,
            CrimeRate = crimeRate,
            FireRisk = fireRisk,
            HealthStatus = healthStatus,
            AverageIncome = averageIncome,
            HasActiveFire = hasActiveFire,
            HasActiveCrime = hasActiveCrime
        };
    }

    private sealed class MockServiceDemandContext : IServiceDemandContext
    {
        public int Population { get; set; }
        public int Students { get; set; }
        public double CrimeRate { get; set; }
        public double FireRisk { get; set; }
        public double HealthStatus { get; set; }
        public double AverageIncome { get; set; }
        public bool HasActiveFire { get; set; }
        public bool HasActiveCrime { get; set; }

        public int GetPopulationAt(GridPosition location) => Population;
        public int GetStudentsAt(GridPosition location) => Students;
        public double GetCrimeRateAt(GridPosition location) => CrimeRate;
        public double GetFireRiskAt(GridPosition location) => FireRisk;
        public double GetHealthStatusAt(GridPosition location) => HealthStatus;
        public double GetAverageIncomeAt(GridPosition location) => AverageIncome;
        public bool HasActiveFireAt(GridPosition location) => HasActiveFire;
        public bool HasCrimeAt(GridPosition location) => HasActiveCrime;
    }
}
