using ConsoleCity.Core;
using ConsoleCity.Services;

namespace ConsoleCity.Simulation;

public sealed class SimulationEngine : ISimulationEngine
{
    private readonly MutableSimulationClock clock;
    private readonly IReadOnlyList<ISimulationSystem> systems;
    private readonly SimulationContext context;
    private readonly SimulationEventProcessor eventProcessor = new();
    private readonly SimulationEngineOptions options;
    private readonly HashSet<string> processedInfrastructureEvents = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> processedServiceEvents = new(StringComparer.OrdinalIgnoreCase);

    public SimulationMode Mode { get; private set; }

    public SimulationState State { get; private set; }

    public SimulationTime CurrentTime => clock.Now;

    public SimulationEngine(
        MutableSimulationClock clock,
        IReadOnlyList<ISimulationSystem> systems,
        SimulationContext context,
        SimulationMode mode = SimulationMode.Normal,
        SimulationEngineOptions? options = null)
    {
        this.clock = clock;
        this.systems = systems.OrderBy(system => system.Order).ThenBy(system => system.Name, StringComparer.Ordinal).ToList();
        this.options = options ?? new SimulationEngineOptions();
        this.context = this.options.Seed == 0
            ? context
            : context with { Random = new DeterministicRandomSource(this.options.Seed) };
        Mode = mode;
        State = SimulationState.Create(mode).WithTime(clock.Now);
    }

    public void Step(int ticks = 1)
    {
        if (Mode == SimulationMode.Paused)
        {
            return;
        }

        var totalTicks = Math.Max(1, ticks * options.Acceleration);
        for (var i = 0; i < totalTicks; i++)
        {
            clock.Advance();
            ExecuteSystems();
            State = State.WithTime(clock.Now).WithTicksProcessed(State.TicksProcessed + 1).WithMode(Mode);
        }
    }

    public void Pause() => SetMode(SimulationMode.Paused);

    public void Resume() => SetMode(SimulationMode.Normal);

    public void SetAcceleration(int acceleration)
    {
        if (acceleration <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(acceleration));
        }

        State = new SimulationState(Mode, State.CurrentTime, State.TicksProcessed, State.Events);
    }

    public void SetMode(SimulationMode mode)
    {
        Mode = mode;
        State = State.WithMode(mode);
    }

    public void EnqueueEvent(SimulationEvent simulationEvent)
    {
        eventProcessor.Enqueue(simulationEvent);
    }

    public IReadOnlyList<SimulationEventRecord> GetEventHistory() => eventProcessor.History;

    private void ExecuteSystems()
    {
        foreach (var system in systems)
        {
            if (system.TickInterval <= 1 || clock.Now.Tick % system.TickInterval == 0)
            {
                system.Execute(context);

                // After each system runs, inspect infrastructure snapshot for new events and publish them
                try
                {
                    var infra = context.Infrastructure;
                    if (infra is not null)
                    {
                        foreach (var ie in infra.Snapshot.Events)
                        {
                            var key = $"{ie.UtilityType}:{ie.Type}:{ie.TargetId}:{ie.CapturedAt.Tick}";
                            if (!processedInfrastructureEvents.Add(key))
                            {
                                continue;
                            }

                            // Find a location for the target if possible
                            GridPosition? location = null;
                            var network = infra.Snapshot.Networks.FirstOrDefault(n => n.UtilityType == ie.UtilityType);
                            if (network is not null)
                            {
                                var node = network.GetNode(ie.TargetId);
                                if (node is not null)
                                {
                                    location = node.Position;
                                }
                                else
                                {
                                    var edge = network.GetEdge(ie.TargetId);
                                    if (edge is not null)
                                    {
                                        var from = network.GetNode(edge.FromNodeId);
                                        if (from is not null)
                                        {
                                            location = from.Position;
                                        }
                                    }
                                }
                            }

                            var severity = ie.Magnitude > 0m ? (double?)Math.Min(1.0, (double)ie.Magnitude) : 0.0;
                            var ev = new SimulationEvent(EntityId.New(), $"infrastructure.{ie.UtilityType}.{ie.Type}", new SimulationTick(context.Clock.Now.Tick), null, location, ie.Message, severity);
                            EnqueueEvent(ev);
                        }
                    }
                }
                catch
                {
                    // don't let infrastructure event publishing disrupt system execution
                }

                // After systems run, inspect services snapshot for new events and publish them
                try
                {
                    var services = context.Services;
                    if (services is EnhancedServiceModel enhancedServices)
                    {
                        foreach (var se in enhancedServices.Events)
                        {
                            var key = $"{se.ServiceType}:{se.EventType}:{se.FacilityId}:{context.Clock.Now.Tick}";
                            if (!processedServiceEvents.Add(key))
                            {
                                continue;
                            }

                            var ev = new SimulationEvent(
                                EntityId.New(),
                                $"service.{se.ServiceType}.{se.EventType}",
                                new SimulationTick(context.Clock.Now.Tick),
                                null,
                                se.Location,
                                se.Description,
                                se.Severity
                            );
                            EnqueueEvent(ev);
                        }
                    }
                }
                catch
                {
                    // don't let service event publishing disrupt system execution
                }
            }
        }

        var processed = eventProcessor.Drain(clock.Now);
        if (processed.Count > 0)
        {
            State = State.WithEvents(State.Events.Concat(processed).ToList());
        }
    }
}
