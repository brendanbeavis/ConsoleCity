# 25. Code Structure and Standards

## Solution layout

``` text
src/
  ConsoleCity.App/
  ConsoleCity.Core/
  ConsoleCity.Simulation/
  ConsoleCity.Economy/
  ConsoleCity.Technology/
  ConsoleCity.Progression/
  ConsoleCity.WorldGeneration/
  ConsoleCity.Infrastructure/
  ConsoleCity.UI/

tests/
  ConsoleCity.Core.Tests/
  ConsoleCity.Simulation.Tests/
  ConsoleCity.Economy.Tests/
  ConsoleCity.WorldGeneration.Tests/
  ConsoleCity.Integration.Tests/
  ConsoleCity.Testing/
```

## Core responsibilities

### Core

Pure domain models and contracts.

Should have minimal external dependencies.

### Simulation

Tick orchestration and system execution.

### Economy

Production, consumption, markets, employment and finance.

### Technology

Research and technology unlocks.

### Progression

Development Credits, milestones and perks.

### WorldGeneration

Seeded world generation.

### Infrastructure

Persistence, configuration, logging and external concerns.

### UI

Console/UI rendering and input.

## Coding standards

Use:

-   nullable reference types;
-   file-scoped namespaces;
-   explicit domain types where useful;
-   immutable records for value objects;
-   interfaces at meaningful boundaries;
-   dependency injection at application boundaries;
-   async I/O where appropriate;
-   cancellation tokens for long-running operations.

Avoid:

-   service locator;
-   global mutable state;
-   static simulation state;
-   UI references inside domain models;
-   random calls scattered through business logic;
-   hidden database calls inside entities.

## Naming

Prefer explicit domain names:

``` csharp
PopulationCount
ElectricityCapacity
DevelopmentCredits
SimulationTick
```

over ambiguous primitive values.

## Entity IDs

Use strongly typed IDs where practical:

``` csharp
PersonId
BuildingId
CityId
RoadId
```

## Error handling

Domain failures should be represented as meaningful results/exceptions
depending on whether they are expected conditions.

Example expected failure:

``` text
Cannot build hospital:
technology not unlocked
```

This should not be a generic exception.

## Configuration

Put balance values into data/configuration:

-   costs;
-   capacities;
-   production recipes;
-   research costs;
-   perk effects;
-   probabilities.

Avoid hard-coding balance values into simulation algorithms.

## Documentation

Public domain concepts should have XML documentation where useful.

Important algorithms should include a short explanation of the model and
assumptions.
