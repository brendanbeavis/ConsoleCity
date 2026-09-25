# 27. Data-Driven Content

## Principle

Content definitions should be separate from simulation algorithms.

This allows new buildings, technologies, production recipes and perks to
be added without changing core code.

## Recommended initial format

JSON is sufficient.

Example building definition:

``` json
{
  "id": "residential.house.basic",
  "name": "Basic House",
  "category": "Residential",
  "capacity": 4,
  "constructionCost": 50000,
  "electricityDemand": 1,
  "waterDemand": 1
}
```

## Definition categories

Suggested data folders:

``` text
data/
  buildings/
  businesses/
  transport/
  services/
  infrastructure/
  resources/
  technologies/
  perks/
  production/
  scenarios/
  world-generation/
```

## Data validation

Load all definitions at startup and validate:

-   unique IDs;
-   required properties;
-   valid references;
-   non-negative capacities/costs;
-   prerequisite technologies exist;
-   production recipes reference valid resources;
-   perks reference valid effects.

Fail fast on invalid game content.

## Runtime state vs definitions

Definitions are static:

``` text
House definition
```

Runtime state is mutable:

``` text
House H-123
condition = 0.83
occupants = [...]
```

Do not mutate definitions during simulation.

## Modding potential

A data-driven design creates a future path to modding without requiring
the first release to support a formal mod API.
