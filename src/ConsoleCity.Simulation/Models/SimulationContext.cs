using ConsoleCity.Agents;
using ConsoleCity.Core;
using ConsoleCity.Economy;
using ConsoleCity.Infrastructure;
using ConsoleCity.Services;
using ConsoleCity.Transport;
using ConsoleCity.World;

namespace ConsoleCity.Simulation;

public sealed record SimulationContext(
    ISimulationClock Clock,
    IWorldRepository World,
    IAgentModel Agents,
    IEconomyModel Economy,
    IInfrastructureModel Infrastructure,
    ITransportModel Transport,
    IServiceModel Services,
    IRandomSource Random) : ISimulationContext;
