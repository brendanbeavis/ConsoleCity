# 28. MVP and Implementation Roadmap

## Phase 0 --- Repository foundation

Create:

-   solution;
-   projects;
-   CI;
-   coding standards;
-   basic test infrastructure;
-   logging;
-   configuration;
-   deterministic RNG;
-   simulation clock.

## Phase 1 --- World skeleton

Implement:

-   world;
-   regions;
-   cities;
-   districts;
-   plots;
-   grid;
-   terrain;
-   seeded generation;
-   save/load.

Goal: generate and inspect a world.

## Phase 2 --- Basic population

Implement:

-   people;
-   households;
-   housing;
-   life stages;
-   needs;
-   simple movement;
-   basic employment.

Goal: people exist and behave over time.

## Phase 3 --- RICO/F economy

Implement:

-   residential;
-   commercial;
-   industrial;
-   office;
-   farming;
-   production;
-   consumption;
-   money;
-   jobs;
-   basic supply chain.

Goal: the world develops an economy.

## Phase 4 --- Transport

Implement:

-   roads;
-   pedestrians;
-   cars;
-   freight;
-   basic route finding;
-   congestion.

Goal: movement matters.

## Phase 5 --- Utilities

Implement:

-   electricity;
-   water;
-   sewage;
-   waste.

Goal: cities have infrastructure constraints.

## Phase 6 --- Services

Implement:

-   police;
-   fire;
-   healthcare;
-   schools;
-   parks.

Goal: quality of life and service capacity matter.

## Phase 7 --- Technology and progression

Implement:

-   research;
-   technology tree;
-   Development Credits;
-   milestones;
-   initial perk pool.

Goal: strategic progression.

## Phase 8 --- Events

Implement:

-   outages;
-   shortages;
-   fires;
-   weather;
-   economic events;
-   event log.

Goal: create emergent problems.

## Phase 9 --- UI refinement

Implement:

-   console UI;
-   map;
-   inspector;
-   build menus;
-   dashboards;
-   event feed;
-   simulation controls.

## Phase 10 --- Balance and scale

Run:

-   deterministic long simulations;
-   performance tests;
-   balancing experiments;
-   save compatibility tests.

## First playable milestone

The first genuinely playable version should support:

1.  new game;
2.  generated map;
3.  build homes;
4.  build roads;
5.  build workplaces;
6.  populate people;
7.  simulate time;
8.  people travel to work;
9.  businesses produce;
10. households consume;
11. utilities constrain growth;
12. inspect world state;
13. save/load;
14. earn and spend Development Credits.

Everything beyond this can be layered on top of the same core.
