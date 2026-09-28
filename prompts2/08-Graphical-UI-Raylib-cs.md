# ConsoleCity — Phase 8: Graphical UI

## Objective

Read the existing ConsoleCity implementation and all relevant documentation in `/docs`, especially architecture, gameplay, map, UI and simulation documentation.

Implement **Phase 8: Graphical UI** using:

- **Raylib-cs** for rendering and input
- A **retro terminal / pixel-art / vector-art hybrid** visual style
- The existing simulation and game architecture as the source of truth

The graphical layer is a client of the simulation. It must never introduce graphical concerns into the simulation domain.

Dependency direction:

```text
ConsoleCity.Core
    ↓
World / Agents / Economy / Infrastructure / Transport / Services
    ↓
ConsoleCity.Simulation
    ↓
ConsoleCity.Game
    ↓
ConsoleCity.Graphics
    ↓
Raylib-cs
```

The simulation must never reference `ConsoleCity.Graphics` or Raylib-cs.

---

## 1. Technology

Use **Raylib-cs** as the graphical rendering library.

Do not introduce Godot, Unity, or another full game engine.

Create an appropriate presentation project such as:

```text
src/
└── ConsoleCity.Graphics/
```

Follow the existing solution structure if an equivalent project already exists.

Raylib-specific types and calls should remain inside the graphics boundary wherever practical.

The graphical application must allow the simulation to remain testable and runnable without a graphical environment.

---

## 2. Visual Identity

ConsoleCity should deliberately avoid looking like a conventional modern city-builder.

The visual language should combine:

- old-school computer terminals
- console interfaces
- pixel-art city maps
- simple vector/geometric graphics
- monospace typography
- information-dense panels
- restrained colours
- visible simulation data
- retro diagnostic/debug aesthetics

Avoid:

- glossy modern dashboards
- realistic 3D graphics
- photorealistic assets
- excessive animation
- unnecessary visual effects
- generic modern strategy-game presentation

Keep fonts, colours, sprites and UI assets easy to replace.

---

## 3. Hybrid Presentation

### Terminal layer

Use terminal-like presentation for:

- simulation date/time
- population
- economy
- resources
- utilities
- services
- selected-object information
- events/history
- diagnostics
- notifications
- commands

Use monospace fonts, rectangular panels, compact information displays and simple borders.

### Map layer

Use graphical 2D representation for:

- plots
- buildings
- roads
- infrastructure
- districts
- parks
- farms
- transport
- people/agents where appropriate

Graphics may use:

- pixel-art sprites
- simple vector shapes
- lines
- rectangles
- icons
- coloured map cells
- small animated indicators

Detailed art is not required for the initial implementation.

---

## 4. Low-Resolution Rendering

Prefer a configurable low-resolution internal render surface, for example:

```text
320 × 180
640 × 360
```

Scale it to the application window using crisp/nearest-neighbour-style scaling where appropriate.

Keep logical rendering resolution separate from physical window resolution:

```text
Logical Resolution
    ↓
Camera / Layout
    ↓
Raylib Rendering
    ↓
Window Scaling
```

Do not hard-code the UI around one physical display size.

---

## 5. Application Loop

Separate:

1. Input
2. Game/simulation interaction
3. View-model generation
4. Rendering

Conceptually:

```text
while running:

    input = ReadInput()

    game.HandleInput(input)

    simulation.Update(...)

    view = BuildViewModel(...)

    renderer.Render(view)
```

Do not put domain logic inside drawing code.

Do not update simulation entities from rendering code.

Simulation progression must not depend on render FPS.

---

## 6. Simulation Time

Rendering and simulation operate independently.

Support:

- pause
- resume
- single-step/advance
- normal speed
- faster speeds
- optionally slower speeds

For example:

```text
paused
1x
2x
5x
10x
50x
```

Rendering should continue while simulation time is paused.

Use the existing simulation timing architecture rather than creating a second timing system.

---

## 7. Camera and Map

Reuse the Phase 7 map representation.

Do not create an independent world/map model inside the graphics project.

Implement:

- pan
- zoom
- viewport bounds
- world-to-screen conversion
- screen-to-world conversion
- map positioning

Conceptually:

```text
World Coordinates
    ↓
Map/View Representation
    ↓
Camera Transform
    ↓
Screen Coordinates
    ↓
Raylib
```

Support mouse selection and hover.

The renderer should not decide what simulation entities mean.

---

## 8. Presentation View Models

Introduce view models where useful, for example:

```text
CityMapView
PlotView
BuildingView
RoadView
InfrastructureView
AgentView
DistrictView
SelectionView
EconomyView
UtilityView
ServiceView
SimulationStatusView
EventLogView
```

Use the boundary:

```text
Simulation Domain
    ↓
View Model Builder
    ↓
Graphics View Models
    ↓
Raylib Renderer
```

Do not expose unnecessary simulation internals directly to rendering code.

Only include fields actually supported by the current implementation.

---

## 9. Main Screen

Implement a first usable graphical screen.

A conceptual layout:

```text
┌──────────────────────────────────────────────────────────────┐
│ CONSOLECITY     YEAR 2026   DAY 142   14:32   ▶ 2x          │
├───────────────────────────────────────────────┬──────────────┤
│                                               │ SELECTED     │
│                 CITY MAP                      │              │
│                                               │ Building     │
│                                               │ Population   │
│                                               │ Jobs         │
│                                               │ Power        │
│                                               │ Water        │
├───────────────────────────────────────────────┴──────────────┤
│ POP 12,482  MONEY $84,120  POWER 94%  WATER 98%  EVENTS 3  │
├──────────────────────────────────────────────────────────────┤
│ > BUILD   > INSPECT   > MAP   > ECONOMY   > SERVICES        │
└──────────────────────────────────────────────────────────────┘
```

Treat this as a visual direction, not a requirement to fabricate unavailable data.

Adapt the screen to what the current implementation actually supports.

---

## 10. Selection and Inspection

Allow the player to select map objects.

Support whatever entity types already exist, potentially including:

- people
- households
- buildings
- businesses
- roads
- transport
- utilities
- services
- districts
- plots
- cities

The inspector should display real simulation state.

Example:

```text
BUILDING #1842
────────────────────
Type       Residential
District   Northside
Residents  4
Jobs       0
Power      OK
Water      OK
Condition  94%
```

Only display information supported by the current implementation.

Make the inspector extensible.

---

## 11. Map Rendering

Render the Phase 7 spatial model.

Possible visual conventions:

```text
Residential     small building shapes
Commercial      storefront/block shapes
Industrial      larger blocks
Roads           lines/paths
Parks           simple natural symbols
Farms           repeating field patterns
Utilities       small infrastructure symbols
Transport       small moving indicators
People          tiny sprites/markers
Districts       subtle boundaries
```

Simple geometric placeholders are preferred for the initial implementation.

The architecture should allow them to be replaced by pixel-art or vector assets later.

---

## 12. Rendering Abstractions

Use only useful abstractions. Potential boundaries include:

```text
IGraphicsRenderer
IMapRenderer
IUiRenderer
IInputHandler
ICamera
```

Do not create abstractions solely for abstraction's sake.

The purpose is to prevent Raylib-specific dependencies spreading into Game, Simulation or Domain projects.

---

## 13. Asset Management

Support external assets such as:

```text
assets/
├── fonts/
├── sprites/
├── icons/
├── ui/
└── maps/
```

Do not create a complex asset pipeline yet.

Initial graphics may use:

- procedural geometry
- simple shapes
- one or two fonts
- placeholder sprites

Replacing placeholders with proper artwork later must not require simulation changes.

---

## 14. Input

Translate graphical input into game-level commands.

Potential bindings:

```text
Escape       Pause/menu
Space        Pause/resume
+ / -        Simulation speed
Mouse wheel  Zoom
Middle drag  Pan
Left click   Select
Right click  Context/action
B            Build mode
I            Inspect mode
M            Map mode
E            Economy
U            Utilities
S            Services
```

These are examples only.

Inspect the existing Game/Console command model and reuse existing commands rather than duplicating domain/game logic.

---

## 15. Construction

If Phase 6 construction functionality exists, expose it graphically.

Support existing capabilities for:

- selecting a build type
- selecting a location
- placement preview
- validation
- confirmation
- demolition where supported

Use the same game/construction functionality as other interfaces:

```text
Graphical Input
    ↓
Game Command
    ↓
Construction System
    ↓
Simulation
    ↓
Updated World
    ↓
Graphical View
```

Do not create separate graphical construction rules.

---

## 16. Information Panels

Expose information already available from the simulation.

Potential panels:

### Simulation
- date
- time
- speed
- pause state
- tick/step

### Population
- population
- households
- employment
- migration where implemented

### Economy
- money
- production
- consumption
- employment
- prices
- trade

### Utilities
- power
- water
- sewerage
- waste
- communications

### Services
- police
- fire
- healthcare
- education
- emergency services

Only display implemented systems.

The UI should expand as the simulation expands.

---

## 17. Event / History Display

If events/history already exist, provide a terminal-style event log.

Example:

```text
14:32:08  POWER     North District demand increased
14:32:12  ECONOMY   Factory #82 production reduced
14:32:18  TRAFFIC   Congestion detected on Main St
14:32:24  PEOPLE    6 households migrated into Northside
```

Consume the existing simulation event model.

Do not create a separate UI-only event system.

---

## 18. Console and Graphical Modes

Keep the existing console presentation available.

Both interfaces should consume the same underlying Game/Simulation state:

```text
ConsoleCity.Console ──┐
                      ├──> Game / Simulation
ConsoleCity.Graphics ─┘
```

The graphical UI must not replace the console architecture unless existing project documentation explicitly requires it.

---

## 19. Performance

Do not prematurely optimize.

Nevertheless:

- avoid rebuilding expensive UI data every render frame
- avoid full-world queries every frame
- avoid repeatedly loading assets
- load and reuse fonts/textures
- avoid blocking the simulation thread
- keep simulation update frequency independent from rendering FPS

Optimize based on actual evidence rather than speculation.

---

## 20. Testing

Prioritize automated tests for non-rendering presentation logic.

Test:

- view-model generation
- coordinate conversion
- camera movement
- zoom calculations
- selection mapping
- input-to-game-command translation
- construction command routing
- map updates after simulation changes
- simulation/game/graphics dependency boundaries

View-model and input logic should be testable without starting Raylib.

Full graphical rendering tests are not required for the initial phase.

---

## 21. Initial Vertical Slice

Do not implement the entire graphical UI at once.

The first working slice should provide:

1. Raylib-cs application starts.
2. Existing simulation can run.
3. A city/map is rendered.
4. Simulation time is visible.
5. Pause/resume works.
6. Simulation speed can be changed.
7. Camera can pan.
8. Camera can zoom.
9. Map objects can be selected.
10. A selected object can be inspected.
11. Basic available population/economy/utility information is displayed.
12. Existing build functionality can be accessed where implemented.
13. Simulation progression is independent of render FPS.
14. No simulation project references Raylib.

Only after this is stable should detailed artwork, animation, effects and richer interaction be added.

---

## 22. Documentation

Update relevant `/docs` documentation to describe:

- graphical architecture
- Raylib-cs choice
- project boundaries
- rendering loop
- camera model
- map rendering
- view models
- input model
- asset strategy
- visual design principles
- relationship between Console and Graphical interfaces

Do not create contradictory documentation.

If existing documentation conflicts with this prompt, follow the established project documentation and make the minimum necessary adjustment rather than silently changing architectural decisions.

---

## 23. Acceptance Criteria

Phase 8 is complete when:

- [ ] ConsoleCity has a working Raylib-cs graphical application.
- [ ] Raylib dependencies are isolated to the graphics/presentation layer.
- [ ] The simulation can run without graphics.
- [ ] The existing Console interface remains functional.
- [ ] The graphical UI renders the existing world/map.
- [ ] The UI has a deliberate retro terminal/pixel/vector identity.
- [ ] The map supports camera movement and zoom.
- [ ] Map objects can be selected.
- [ ] Selected objects can be inspected.
- [ ] Simulation time and speed are visible.
- [ ] Pause/resume works.
- [ ] Graphical input uses existing game/simulation commands.
- [ ] Existing construction functionality is exposed where available.
- [ ] The UI does not duplicate domain/simulation logic.
- [ ] The simulation does not depend on the graphics project.
- [ ] Presentation logic has automated tests where practical.
- [ ] Documentation has been updated.
- [ ] The implementation remains consistent with `/docs`.

---

## 24. Implementation Rules

Before changing code:

1. Read the existing implementation.
2. Read relevant `/docs` files.
3. Identify implemented functionality versus planned functionality.
4. Reuse existing domain and game functionality.
5. Do not invent unavailable simulation systems.
6. Do not move simulation logic into the UI.
7. Do not introduce a game engine.
8. Keep Raylib-specific code isolated.
9. Prefer a small working vertical slice over broad incomplete functionality.
10. Add/update tests for presentation logic.
11. Update documentation.
12. Keep the code idiomatic for the project's current .NET/C# version.

The objective is not merely to display a map. Establish the long-term graphical presentation architecture for a retro, information-rich, simulation-driven world simulation while preserving the underlying simulation as the authoritative system.
