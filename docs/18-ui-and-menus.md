# 18. UI and Menus

## Interface philosophy

The UI should feel like an old-school simulation console while still
allowing mouse-friendly inspection.

Text and structured panels are preferred over high-fidelity graphics.

The graphical client should use a hybrid presentation:

-   terminal-style panels for status, diagnostics and inspection;
-   low-resolution 2D map rendering for spatial information;
-   restrained colours, simple geometry and replaceable assets.

The same core actions should remain available in both console and graphical
modes.

## Main menu

``` text
CONSOLECITY

[1] New Game
[2] Load Game
[3] Continue
[4] Scenarios
[5] Settings
[6] Exit
```

## New Game

Flow:

``` text
New Game
→ World seed
→ World size
→ Geography preset
→ Starting era/technology preset
→ Difficulty
→ Starting resources
→ Player/civilization name
→ Generate
→ World summary
→ Enter simulation
```

## Load Game

Show:

-   save name;
-   date;
-   world seed;
-   population;
-   cities;
-   play time;
-   last saved time.

Actions:

-   load;
-   delete;
-   rename;
-   duplicate.

## Main simulation screen

Suggested layout:

``` text
┌─────────────────────────────────────────────────────────────┐
│ CONSOLECITY  Year 12 / Day 184 / 14:00   $1.2M   RP 542    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│                    MAP / TEXT WORLD                         │
│                                                             │
├──────────────────┬──────────────────────────────────────────┤
│ STATUS           │ INSPECTOR                                │
│ Population       │ Selected object                          │
│ Jobs             │ Type / condition / capacity              │
│ Power            │ Inputs / outputs                         │
│ Water            │ Connections                              │
│ Traffic          │ Problems                                 │
└──────────────────┴──────────────────────────────────────────┘
```

## Simulation controls

-   pause;
-   step;
-   speed;
-   save;
-   load;
-   camera/map mode;
-   event log;
-   pan and zoom in graphical mode;
-   mouse selection/inspection in graphical mode.

## Build menu

``` text
BUILD
  Residential
  Industrial
  Commercial
  Office
  Farming
  Transport
  Utilities
  Services
  Parks
  Civic
  Special
```

## Inspector

Every object should support an inspect command.

Example:

``` text
> inspect building B-1042

B-1042
TYPE: SUPERMARKET
STATUS: OPERATIONAL

Workers: 41 / 50
Customers today: 812
Inventory: 67%
Power: 100%
Water: 100%
Road access: GOOD

Problems:
- Freight deliveries delayed 12%
```

## Command interface

Potential commands:

``` text
help
world
map
inspect <id>
build <type>
zone <type>
connect <a> <b>
demolish <id>
budget
economy
population
transport
utilities
research
perks
events
save
load
pause
step
speed <n>
```

The same actions should eventually be available through mouse-driven
menus.
