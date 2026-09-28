# 29. Graphical UI

## Purpose

ConsoleCity supports both console and graphical presentation layers.

The graphical UI is a client of the existing game and simulation stack. It
must never introduce graphical concerns into the simulation domain.

## Technology choice

The graphical vertical slice uses `Raylib-cs`.

Reasons:

-   small dependency surface;
-   explicit control of the application loop;
-   suitable for low-resolution 2D rendering;
-   easy isolation of rendering/input code inside a presentation project;
-   no requirement for a full game engine.

## Project boundary

``` text
ConsoleCity.Core
  ↓
ConsoleCity.World / Agents / Economy / Infrastructure / Transport / Services
  ↓
ConsoleCity.Simulation
  ↓
ConsoleCity.Game
  ↓
ConsoleCity.Graphics
  ↓
Raylib-cs
```

Rules:

-   `ConsoleCity.Graphics` may depend on `ConsoleCity.Game`, `ConsoleCity.World`
	and `ConsoleCity.Core`.
-   `ConsoleCity.Simulation`, `ConsoleCity.Game` and domain projects must not
	reference Raylib-cs.
-   the simulation remains runnable without a graphical environment.
-   the console UI and graphical UI are parallel clients of the same game state.

## Presentation model

The graphical UI uses a hybrid style:

-   terminal-like text panels for simulation status, selection, diagnostics and
	commands;
-   low-resolution 2D map rendering for plots, buildings, roads and terrain;
-   restrained colours, simple geometry and replaceable assets.

Detailed artwork is optional. The first slice should prefer readable geometry
and explicit simulation data over decorative effects.

## Rendering loop

The rendering loop keeps input, simulation advancement, view-model generation
and drawing separate.

``` text
while running:
	input = read graphical input
	controller.handle(input)
	controller.update(simulationDelta)
	view = build view model
	renderer.render(view)
```

Simulation advancement is based on elapsed real time and the selected
simulation speed. Rendering continues even when the simulation is paused.

## Low-resolution rendering

The graphical client renders to a logical surface first, then scales that
surface to the window.

``` text
Logical surface (for example 640×360)
  ↓
UI layout + camera transform
  ↓
Raylib drawing
  ↓
scaled window output
```

Nearest-neighbour style scaling is preferred so the presentation keeps a crisp
retro appearance.

## Camera model

The map camera is responsible for:

-   pan;
-   zoom;
-   viewport bounds;
-   world-to-screen conversion;
-   screen-to-world conversion.

The camera works in world-cell space and stays independent of simulation rules.
Selection logic asks the existing world/map inspection layer what object is at a
world position.

## View models

Rendering does not read arbitrary domain objects directly.

The graphics project builds explicit view models such as:

-   simulation status;
-   summary bar items;
-   map cells;
-   selection/inspection panel data;
-   build palette state;
-   construction summaries.

This keeps rendering testable and allows panel content to evolve without
changing the simulation architecture.

## Input model

Graphical input is translated into game/session actions such as:

-   pause/resume;
-   step;
-   speed change;
-   camera movement;
-   hover/selection;
-   construction request.

Input handling belongs in the graphics layer. Domain rules, validation and world
changes remain in `ConsoleCity.Game` and below.

## Asset strategy

The first slice may use built-in fonts, simple shapes and placeholder symbols.

External assets should remain easy to introduce later using a lightweight
layout such as:

``` text
assets/
├── fonts/
├── sprites/
├── icons/
└── ui/
```

Replacing graphical assets must not require simulation changes.

## Testing strategy

Automated tests should focus on non-rendering presentation logic:

-   camera coordinate conversion;
-   pan and zoom behaviour;
-   view-model generation;
-   input/controller behaviour;
-   dependency boundary protection.

The goal is confidence in the presentation logic without requiring graphical
snapshot testing.

## Relationship to console mode

`ConsoleCity.Console` remains available.

Both presentation layers consume the same underlying game/simulation model.
The console mode stays valuable for testing, debugging and headless-oriented
workflows, while the graphical client provides map navigation and mouse-based
inspection.