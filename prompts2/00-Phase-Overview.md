# ConsoleCity — Copilot Implementation Phases 3–9

These prompts define the recommended implementation sequence after the initial architecture, playable simulation slice, save/load and basic Console tooling have been implemented.

## Sequence

1. Phase 3 — Utilities Constraints
2. Phase 4 — Economy and Logistics
3. Phase 5 — City Services
4. Phase 6 — Player Construction and Development
5. Phase 7 — Map Representation
6. Phase 8 — Graphical UI
7. Phase 9 — Advanced Game Mechanics

## Guiding principle

Do not treat these phases as independent feature drops.

Each phase should deepen the existing simulation and create interactions with previously implemented systems.

The desired progression is:

World
→ autonomous agents
→ economy
→ utilities
→ logistics
→ services
→ player construction
→ spatial representation
→ graphical presentation
→ strategic game mechanics

## Before each phase

Copilot should:

1. Read the relevant `/docs` files.
2. Inspect the existing implementation.
3. Identify already-existing types and systems.
4. Reuse existing concepts rather than creating duplicates.
5. Identify architecture conflicts before implementing.
6. Preserve deterministic simulation where practical.
7. Add tests with the implementation.

## Important architectural principles

### Simulation first

The simulation is the source of truth.

### UI independence

Neither the Console nor graphical UI should contain domain simulation rules.

### Explicit interactions

Prefer events, interfaces and explicit domain relationships over direct coupling between unrelated systems.

### Determinism

A seeded simulation should produce reproducible behaviour where randomness is involved.

### No premature complexity

Do not introduce a game engine, ECS, database or large third-party framework without an explicit architectural reason.

### Emergent behaviour

Prefer systems interacting through rules and constraints over scripted outcomes.

## Desired end state

The eventual ConsoleCity architecture should allow:

- A world to run autonomously.
- People and organisations to make decisions.
- Cities to grow and change.
- Utilities to constrain development.
- Goods and people to move.
- Businesses to produce and consume.
- Services to respond to demand.
- Players to construct and govern.
- The world to be visualised.
- Long-term progression and game mechanics to operate over the simulation.

The objective is not maximum realism.

The objective is a coherent, understandable simulation capable of producing interesting emergent behaviour.
