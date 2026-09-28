# 1. Vision and Design Principles

## Vision

ConsoleCity is a living city/world simulation presented through a
deliberately old-school interface. The player establishes and guides
civilisation-scale development while the underlying population and
economy continue operating autonomously.

The interesting question is not simply "what can I build?" but:

> "What happens when I build this, connect it, fund it, and let
> thousands of simulated decisions interact?"

## Player role

The player is the **civilization steward**.

The player does not directly control Bob walking to work. Instead, the
player creates the conditions under which Bob's household, workplace,
transport network and city services behave in useful ways.

The player can:

-   create and expand settlements;
-   designate land;
-   build or approve infrastructure;
-   establish services;
-   connect cities;
-   influence economic development;
-   unlock technologies;
-   respond to disasters and shortages;
-   inspect individual people and organisations;
-   pause, accelerate or step through time;
-   shape the long-term trajectory of the world.

## Primary fun priorities

1.  Building and expanding cities.
2.  Watching the world evolve autonomously.
3.  Unlocking new technologies.
4.  Economic optimisation and resource management.

## Design principles

### Emergence over scripts

Prefer systems that create outcomes from rules over scripted stories. A
recession should emerge from demand, supply, employment and finance
rather than a hard-coded "recession event".

### Legible complexity

The simulation can be deep internally, but the player must be able to
understand why a result occurred.

Every important metric should have an inspectable explanation path.

### Consequences

Construction should change the simulation. A new road affects
accessibility. A factory creates jobs and demand for freight. A new
suburb creates residents who need schools, shops, transport and
utilities.

### Imperfection

A world that never has problems is not interesting. Capacity
constraints, failures, shortages, congestion, unemployment, disasters
and competing needs should create decisions.

### Systemic abstraction

Avoid simulating every physical detail. Model only details that
influence gameplay.

### Determinism

A seed should reproduce the same world and, given the same actions, the
same simulation sequence.

### Modularity

Simulation systems should communicate through well-defined domain state
and events rather than tightly coupling every entity to every other
entity.

### Data-driven content

Adding a new building, transport type, service, technology or perk
should generally require data/configuration rather than a new hard-coded
simulation subsystem.

## Success criteria

A successful early prototype should make it possible to:

1.  generate a world;
2.  create a settlement;
3.  populate it with people;
4.  provide housing and employment;
5.  build roads;
6.  create businesses and services;
7.  supply electricity and water;
8.  run the simulation;
9.  observe population/economic changes;
10. intervene;
11. save/load;
12. repeat the experiment with a different strategy.
