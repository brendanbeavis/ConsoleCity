# ConsoleCity — Phase 8: Graphical User Interface

## Copilot Prompt

Read the existing ConsoleCity implementation and all relevant documentation in `/docs`, especially architecture, gameplay, map, UI and simulation documentation.

Implement **Phase 8: Graphical UI**.

The goal is to provide a lightweight graphical presentation layer for ConsoleCity while preserving the simulation-first architecture.

## Critical architecture rule

The graphical UI is a client of the simulation.

It must NOT become the source of truth.

The dependency direction should remain conceptually:

`UI -> Game/Simulation -> World/Systems`

Never:

`Simulation -> UI`

## UI goals

Provide a functional interface for:

- Viewing the city map.
- Inspecting buildings.
- Inspecting people/households.
- Viewing city statistics.
- Viewing economy statistics.
- Viewing utility status.
- Viewing service status.
- Controlling simulation time.
- Pausing/resuming.
- Changing simulation speed.
- Selecting build/construction commands.
- Viewing events/history.

Use the project's documented visual style. Preserve the ConsoleCity identity rather than turning the project into a generic modern city-builder UI.

## Rendering

Reuse the Phase 7 map representation.

Do not duplicate world/map logic inside the UI.

The renderer should consume view data derived from the simulation.

## Interaction

Support basic interactions such as:

- Select object.
- Inspect object.
- Pan/zoom where appropriate.
- Build.
- Demolish.
- Pause.
- Advance time.
- Change simulation speed.

Do not implement every possible UI feature.

Prioritise a stable vertical slice.

## Technology

Use the simplest appropriate .NET-compatible UI/rendering technology consistent with the existing repository and project documentation.

Do not introduce a full game engine unless the project documentation explicitly changes direction.

Keep UI dependencies isolated from simulation projects.

## Performance

Do not prematurely optimise.

However:

- Do not recreate the entire UI model unnecessarily every simulation tick.
- Avoid blocking the simulation thread with rendering.
- Keep simulation time independent from rendering frame rate.

## Testing

Add appropriate tests for:

- View-model generation.
- Selection/inspection.
- Simulation controls.
- Build commands.
- Map updates.
- UI-to-simulation interaction boundaries.

## Completion criteria

The player can run ConsoleCity visually, inspect the world, observe the simulation evolving, control simulation time and perform basic construction without the graphical layer becoming coupled to the simulation internals.
