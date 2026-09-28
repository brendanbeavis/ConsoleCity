# 20. Credits and Roguelike Progression

## Purpose

Roguelike progression provides a second progression layer on top of the
normal simulation.

The player earns **Development Credits** through meaningful
accomplishments and spends them on upgrades/perks.

## Why credits exist

Normal game money answers:

> "What can I afford to build in this world?"

Development Credits answer:

> "What capabilities does my civilization carry forward or specialise
> in?"

Keeping these currencies separate prevents the roguelike system from
simply becoming another cash balance.

## Credit sources

Credits can be earned for:

-   founding a settlement;
-   reaching population milestones;
-   connecting cities;
-   achieving infrastructure milestones;
-   producing significant economic output;
-   researching technologies;
-   surviving disasters;
-   improving service coverage;
-   establishing self-sufficient food/water systems;
-   completing optional challenges;
-   discovering resources;
-   achieving scenario objectives.

## Credit philosophy

Reward outcomes and milestones, not repetitive button clicking.

Bad:

``` text
+1 credit every time road is placed
```

Better:

``` text
+20 credits for establishing a connected road network
```

## Perk selection

At milestones, present a small random selection.

Example:

``` text
DEVELOPMENT OPPORTUNITY

Choose 1:

[1] Efficient Grid
    Utility transmission losses reduced.

[2] Urban Planning
    Residential development requires less infrastructure.

[3] Logistics Culture
    Freight throughput increased.

[4] Skilled Workforce
    Education improves skill generation.
```

The player makes a strategic choice rather than buying everything.

## Run progression

Suggested:

``` text
Start
→ milestone
→ choose perk
→ milestone
→ choose perk
→ technology unlocks
→ increasingly complex city
→ major milestone
→ optional run conclusion
```

## Meta progression

Two modes can coexist:

### Run perks

Apply to the current world/run.

### Civilization legacy

Rare unlocks can remain available between worlds.

The first implementation should start with run-level perks only. Meta
progression can be added later if the roguelike layer proves useful.

## Balance

Perks should change strategy, not simply multiply every output.

Prefer:

-   lower costs for one category;
-   new capabilities;
-   alternative production methods;
-   altered risks;
-   stronger specialisation.

Avoid universally dominant bonuses.
