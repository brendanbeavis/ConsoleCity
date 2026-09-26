# ConsoleCity — Phase 7: Map Representation

## Copilot Prompt

Read the existing ConsoleCity implementation and all relevant documentation in `/docs`.

Implement **Phase 7: Map Representation**.

The objective is to create a visual/spatial representation of the existing simulation state without creating a second world model.

## Architectural rule

The simulation remains the source of truth.

The map is a **view of simulation state**.

Do not create a parallel set of authoritative map entities.

The map should derive its representation from:

- World
- Cities
- Districts
- Plots
- Buildings
- Roads
- Infrastructure
- Transport
- Other documented spatial objects

## Initial implementation

Implement the simplest useful map representation first.

It should support:

- World boundaries
- City boundaries
- Districts
- Plots
- Buildings
- Roads
- Utility networks where useful
- Basic object identification

A simple grid/tile representation is acceptable if consistent with the existing design.

Do not implement advanced graphics yet.

## Interaction

Provide basic ways to:

- Inspect an object.
- Identify its type.
- Select a plot/building.
- Display basic state.
- Highlight relevant networks or relationships where practical.

The map must use existing IDs so that selecting a visual object can resolve back to the actual simulation entity.

## Rendering architecture

Separate:

1. Simulation state.
2. Spatial projection.
3. Rendering/presentation.

For example:

`Simulation World -> Map Model/View Data -> Renderer`

Do not add rendering dependencies to the core simulation projects.

## Console compatibility

Where practical, retain a text/grid representation so the map can initially be tested without a heavyweight graphical framework.

The existing Console UI must continue to work.

## Testing

Test:

- Correct spatial projection.
- Entity-to-map identity.
- Updates after construction.
- Updates after demolition.
- Road/network representation.
- Changes as the simulation advances.

## Completion criteria

The existing city can be represented spatially and inspected without changing the underlying simulation model.

Do not implement a complete graphical UI in this phase.
