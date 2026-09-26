# ConsoleCity — Phase 3: Utilities Constraints

## Copilot Prompt

Read the existing ConsoleCity implementation and all relevant documentation in `/docs`, especially the architecture, world, infrastructure, building, agent and simulation documents.

Implement **Phase 3: Utilities Constraints**.

The goal is to turn utilities from static domain objects into functioning simulation constraints that affect buildings, services, businesses and households.

Implement the currently documented utility systems, prioritising:

- Electricity / power
- Water
- Sewerage
- Waste
- Communications and fuel where already defined by the design

## Core model

Model utilities as systems with a clear distinction between:

- Supply / generation
- Network capacity
- Distribution
- Connections
- Demand
- Allocation
- Shortage
- Outage
- Recovery

Avoid simply storing flags such as `Building.HasPower`.

A building should have utility requirements/demand and a connection to the appropriate network. The simulation should determine whether demand can actually be satisfied.

Conceptually:

`Source -> Network -> Connection -> Consumer`

## Simulation behaviour

Implement behaviour for:

1. Utility production/supply.
2. Network capacity.
3. Consumer demand.
4. Demand allocation.
5. Capacity shortages.
6. Utility outages.
7. Recovery after outages or capacity restoration.
8. Effects on dependent buildings/services where defined by the documentation.

Examples:

- A power station produces electricity.
- Buildings create electricity demand.
- Network capacity limits distribution.
- Excess demand causes shortages.
- A building without required electricity may become unable to operate.
- Water demand can exceed available supply.
- Utility failures should produce simulation events where appropriate.

Do not implement detailed real-world engineering physics. This is a city simulation, not an engineering simulator.

## Architecture requirements

- Keep utility domain logic in `ConsoleCity.Infrastructure`.
- Do not put infrastructure rules into the Console/UI project.
- Use the existing simulation-system/event architecture.
- Avoid tight coupling between infrastructure and individual buildings, people or businesses.
- Prefer explicit domain concepts over generic dictionaries.
- Preserve deterministic simulation when using seeded randomness.

## Observability

Expose useful statistics such as:

- Total generation
- Total demand
- Available capacity
- Utilisation
- Connected consumers
- Unserved demand
- Active outages
- Recovery state

Make these accessible through existing inspection/summary mechanisms where appropriate.

## Testing

Add unit and integration tests for:

- Supply and demand.
- Capacity limits.
- Utility connections.
- Shortages.
- Outages.
- Recovery.
- Building/service effects.
- Multi-tick behaviour.
- Deterministic behaviour.

Do not implement graphical UI or player construction in this phase.

## Completion criteria

A small city should be able to run for an extended period where utility supply and demand interact with the rest of the simulation.

The implementation should make it possible for a city to experience a meaningful utility shortage or outage and for that condition to have observable consequences.
