# 17. Core Game Loop

## Player loop

``` text
PLAN
  ↓
BUILD / ALLOCATE
  ↓
SIMULATE
  ↓
OBSERVE
  ↓
RESPOND
  ↓
DEVELOP
  ↓
repeat
```

## Plan

Inspect:

-   population;
-   land;
-   transport;
-   utilities;
-   economy;
-   service coverage;
-   resource availability.

Choose a strategic objective.

## Build / Allocate

Actions include:

-   zone land;
-   place buildings;
-   build roads;
-   extend utilities;
-   establish services;
-   fund projects;
-   connect cities.

## Simulate

Let the world run.

People:

-   move;
-   work;
-   shop;
-   learn;
-   form households;
-   consume;
-   migrate.

Businesses:

-   hire;
-   produce;
-   trade;
-   expand;
-   close.

Infrastructure:

-   carries flows;
-   degrades;
-   fails;
-   gets upgraded.

## Observe

Use dashboards and drill-downs to understand:

-   where people live;
-   where jobs are;
-   traffic;
-   utility capacity;
-   economic flows;
-   service gaps.

## Respond

Fix bottlenecks and unexpected consequences.

## Develop

Spend research and development progression on:

-   technology;
-   roguelike perks;
-   strategic capabilities.

## Session structure

A game can have a long-running world with periodic milestone moments.

Optional roguelike structure:

``` text
Start run
→ establish settlement
→ expand
→ encounter conditions/events
→ earn development credits
→ select upgrades
→ reach milestone
→ continue or end run
```

The simulation itself should remain valuable even without a forced win
condition.
