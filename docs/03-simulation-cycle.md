# 3. Simulation Cycle

## Time model

ConsoleCity uses a discrete simulation clock.

Recommended initial scale:

-   **1 tick = 1 hour**
-   24 ticks = 1 simulated day
-   7 days = week
-   12 months = year

Not every subsystem needs to execute every tick.

## Multi-rate simulation

  System                  Suggested frequency
  ----------------------- ---------------------
  Movement                every tick
  Utility flow            every tick
  Emergency response      every tick
  Agent needs             every 1--6 hours
  Jobs / production       hourly
  Traffic                 hourly
  Shops / demand          hourly/daily
  Population statistics   daily
  Construction            daily
  Business financials     daily/weekly
  Demographics            daily/monthly
  Technology              daily
  Long-term land value    weekly
  Regional economy        weekly/monthly

## Tick order

A deterministic tick should use a fixed pipeline:

``` text
1. Advance clock
2. Apply scheduled player actions
3. Generate environmental conditions
4. Update infrastructure networks
5. Update transport network and congestion
6. Update production and logistics
7. Update workplaces/businesses
8. Update household needs and finances
9. Run agent decisions
10. Move agents
11. Update services
12. Process construction
13. Resolve events/incidents
14. Apply deaths/births/migration where due
15. Update economy/statistics
16. Generate research/progression
17. Publish observable state changes
18. Persist checkpoint if configured
```

The exact order can evolve, but it must remain deterministic and
documented.

## Agent decision cadence

Agents should not all recalculate every tick.

Use staggered decision windows:

``` text
Person 1 → tick 1
Person 2 → tick 2
...
Person N → tick N mod decisionInterval
```

This reduces CPU spikes.

## Simulation modes

-   **Paused** --- no autonomous time progression.
-   **Step** --- advance one tick.
-   **Slow** --- e.g. 1 simulated hour per several real seconds.
-   **Normal** --- balanced observation speed.
-   **Fast** --- useful for long-term development.
-   **Very fast** --- statistical/management mode.

## Determinism

The simulation RNG should be seeded.

Randomness must come from an injected deterministic RNG service rather
than `System.Random` calls scattered through the domain.

## Checkpoints

The engine should support periodic checkpoints so long-running
simulations can be saved without interrupting the player.

## Explainability

Important state changes should generate structured events:

``` text
Factory F102 production stopped:
- electricity supply: 42% of required
- workers present: 71%
- input steel: 0
```

The UI can turn these events into human-readable explanations.
