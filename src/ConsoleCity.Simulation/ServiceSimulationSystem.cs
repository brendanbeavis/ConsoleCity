using ConsoleCity.Core;
using ConsoleCity.Services;
using ConsoleCity.World;

namespace ConsoleCity.Simulation;

/// <summary>
/// Simulation system that manages city services, demand generation, and service delivery.
/// </summary>
public sealed class ServiceSimulationSystem : ISimulationSystem
{
    public string Name => "Services";

    public int TickInterval => 1; // Run every tick for responsive service updates

    public int Order => 90; // Run after infrastructure but before economy reporting

    private readonly IServiceModel serviceModel;
    private readonly IReadOnlyList<IServiceDemandGenerator> demandGenerators;
    private readonly ServiceDemandContext demandContext;

    public ServiceSimulationSystem(
        IServiceModel serviceModel,
        IReadOnlyList<IServiceDemandGenerator> demandGenerators)
    {
        this.serviceModel = serviceModel ?? throw new ArgumentNullException(nameof(serviceModel));
        this.demandGenerators = demandGenerators ?? throw new ArgumentNullException(nameof(demandGenerators));
        this.demandContext = new ServiceDemandContext();
    }

    public void Execute(ISimulationContext context)
    {
        if (context is not SimulationContext simContext)
        {
            return;
        }

        // Update demand context with current world state
        UpdateDemandContext(simContext);

        // Get all locations in the world that have population/activity
        var world = simContext.World.Current;
        var allLocations = world.Regions
            .SelectMany(r => r.Cities)
            .SelectMany(c => c.Districts)
            .SelectMany(d => d.Plots)
            .SelectMany(p => p.Buildings)
            .Select(b => b.Location)
            .Distinct()
            .ToList();

        // Generate and submit demands
        foreach (var location in allLocations)
        {
            foreach (var generator in demandGenerators)
            {
                var demands = generator.GenerateDemands(location, simContext.Clock.Now, demandContext);

                foreach (var demand in demands)
                {
                    var request = new ServiceRequest(Guid.NewGuid(), demand, simContext.Clock.Now);
                    serviceModel.SubmitRequest(request);
                }
            }
        }

        // Advance the service model
        var snapshot = serviceModel.Advance(simContext.Clock.Now);

        // Note: Service events will be processed by the simulation engine 
        // after this system completes its execution (similar to infrastructure events)
    }

    private void UpdateDemandContext(SimulationContext context)
    {
        // Populate the demand context with current world state
        // This would integrate with actual population, crime, health, etc. systems

        // For now, provide stub data that can be enhanced later
        demandContext.SetPopulationAt(new GridPosition(0, 0), 1000);
        demandContext.SetStudentsAt(new GridPosition(0, 0), 250);
        demandContext.SetHealthStatusAt(new GridPosition(0, 0), 0.3d);
        demandContext.SetCrimeRateAt(new GridPosition(0, 0), 0.2d);
    }
}

/// <summary>
/// Context that provides simulation state information to demand generators.
/// </summary>
internal sealed class ServiceDemandContext : IServiceDemandContext
{
    private readonly Dictionary<GridPosition, int> populationByLocation = new();
    private readonly Dictionary<GridPosition, int> studentsByLocation = new();
    private readonly Dictionary<GridPosition, double> crimeRateByLocation = new();
    private readonly Dictionary<GridPosition, double> fireRiskByLocation = new();
    private readonly Dictionary<GridPosition, double> healthStatusByLocation = new();
    private readonly Dictionary<GridPosition, double> averageIncomeByLocation = new();
    private readonly HashSet<GridPosition> activeFireLocations = new();
    private readonly HashSet<GridPosition> crimeIncidentLocations = new();

    public int GetPopulationAt(GridPosition location)
        => populationByLocation.GetValueOrDefault(location, 0);

    public int GetStudentsAt(GridPosition location)
        => studentsByLocation.GetValueOrDefault(location, 0);

    public double GetCrimeRateAt(GridPosition location)
        => crimeRateByLocation.GetValueOrDefault(location, 0d);

    public double GetFireRiskAt(GridPosition location)
        => fireRiskByLocation.GetValueOrDefault(location, 0d);

    public double GetHealthStatusAt(GridPosition location)
        => healthStatusByLocation.GetValueOrDefault(location, 0d);

    public double GetAverageIncomeAt(GridPosition location)
        => averageIncomeByLocation.GetValueOrDefault(location, 0.5d);

    public bool HasActiveFireAt(GridPosition location)
        => activeFireLocations.Contains(location);

    public bool HasCrimeAt(GridPosition location)
        => crimeIncidentLocations.Contains(location);

    // Setters for updating context
    internal void SetPopulationAt(GridPosition location, int population)
        => populationByLocation[location] = Math.Max(0, population);

    internal void SetStudentsAt(GridPosition location, int students)
        => studentsByLocation[location] = Math.Max(0, students);

    internal void SetCrimeRateAt(GridPosition location, double rate)
        => crimeRateByLocation[location] = Math.Clamp(rate, 0d, 1d);

    internal void SetFireRiskAt(GridPosition location, double risk)
        => fireRiskByLocation[location] = Math.Clamp(risk, 0d, 1d);

    internal void SetHealthStatusAt(GridPosition location, double status)
        => healthStatusByLocation[location] = Math.Clamp(status, 0d, 1d);

    internal void SetAverageIncomeAt(GridPosition location, double income)
        => averageIncomeByLocation[location] = Math.Clamp(income, 0d, 1d);

    internal void SetActiveFireAt(GridPosition location, bool hasfire)
    {
        if (hasfire)
            activeFireLocations.Add(location);
        else
            activeFireLocations.Remove(location);
    }

    internal void SetCrimeAt(GridPosition location, bool hasCrime)
    {
        if (hasCrime)
            crimeIncidentLocations.Add(location);
        else
            crimeIncidentLocations.Remove(location);
    }
}
