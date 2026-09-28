# 19. World Creation and Editing

## World generation

World generation should be deterministic from a seed.

Pipeline:

``` text
Seed
→ terrain
→ water
→ resources
→ regions
→ initial roads
→ settlements
→ starting population
→ initial economy
→ utilities
→ organisations
```

## Generation presets

Potential presets:

-   Balanced;
-   Fertile;
-   Resource Rich;
-   Coastal;
-   Mountainous;
-   Archipelago;
-   Dense;
-   Sparse.

Presets modify generation parameters, not core simulation code.

## Manual editing

The player can modify the world through construction tools.

### Land

-   create/remove development plots;
-   zone land;
-   change district designation.

### Roads

-   draw road;
-   upgrade road;
-   remove road;
-   connect destinations.

### Buildings

-   place building;
-   upgrade;
-   demolish.

### Services

-   place service facility;
-   upgrade capacity;
-   assign funding.

### Utilities

-   place generation;
-   connect networks;
-   upgrade capacity;
-   repair.

### Parks

-   create park;
-   place playground;
-   place sports facility;
-   upgrade attraction.

## Construction constraints

The game should prevent invalid actions such as:

-   building on occupied cells;
-   exceeding available funds;
-   building without required technology;
-   connecting incompatible infrastructure;
-   exceeding utility capacity without an upgrade.

## Preview

Before committing construction, show:

-   cost;
-   footprint;
-   expected capacity;
-   utility demand;
-   prerequisites;
-   affected networks;
-   expected benefit.

## Demolition

Demolition can:

-   cost money;
-   create construction waste;
-   disrupt services;
-   displace residents/businesses;
-   remove infrastructure links.

## Editing philosophy

Construction is an intervention in the simulation, not an abstract menu
action. The consequences should propagate through the world.
