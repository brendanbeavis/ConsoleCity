# 26. Testing and Automation

## Testing philosophy

The simulation is ideal for automated testing because it should be
deterministic and data-driven.

The goal is not merely high line coverage. The goal is confidence that
simulation rules remain stable.

## Test layers

### Unit tests

Test individual rules:

-   utility calculations;
-   production;
-   utility capacity;
-   population needs;
-   route scoring;
-   perk effects.

### System tests

Run one subsystem over multiple ticks.

Examples:

-   electricity distribution;
-   employment;
-   transport congestion;
-   food supply.

### Integration tests

Create a small world and run multiple systems together.

Example:

``` text
Generate world
→ create town
→ add homes
→ add factory
→ add road
→ connect power
→ populate
→ simulate 30 days
→ assert population/economy remain valid
```

### Regression tests

Known scenarios become permanent fixtures.

## Deterministic tests

Every test that uses randomness should specify a seed.

Example:

``` csharp
var world = WorldFactory.Create(seed: 12345);
```

The expected outcome can then be asserted.

## Property/invariant tests

Important invariants:

-   population cannot be negative;
-   money cannot become NaN;
-   capacity cannot be negative;
-   buildings occupy valid cells;
-   network connections reference existing objects;
-   dead people cannot remain employed;
-   demolished buildings cannot continue producing;
-   electricity flow cannot exceed configured capacity unless overload
    is explicitly modelled.

## Simulation smoke tests

Run a standard world for:

-   1 day;
-   1 month;
-   1 year;
-   longer stress runs.

Verify:

-   no crashes;
-   no invalid state;
-   stable memory behaviour;
-   deterministic result.

## Golden simulation tests

Store a known seed and selected aggregate metrics:

``` text
Population = ...
Businesses = ...
Power demand = ...
Food production = ...
```

A code change that alters these values should require an intentional
test update.

## Mock data

Provide factories/builders:

``` text
PersonFactory
HouseholdFactory
BuildingFactory
BusinessFactory
CityFactory
WorldFactory
```

And presets:

-   tiny town;
-   industrial city;
-   farming region;
-   transport-heavy city;
-   utility-constrained city.

## Test automation

CI should run:

1.  restore;
2.  build;
3.  format check;
4.  static analysis;
5.  unit tests;
6.  integration tests;
7.  simulation smoke test;
8.  package/build artifact.

## Performance tests

Later add benchmarks for:

-   1,000 agents;
-   10,000 agents;
-   100,000 agents;
-   large transport networks.

Do not optimise for 100,000 agents before the smaller simulation is
correct.
