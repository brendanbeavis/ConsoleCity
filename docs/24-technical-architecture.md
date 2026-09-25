# 24. Technical Architecture

## Target stack

### Runtime

-   .NET 10
-   C#
-   Visual Studio 2026

### Application type

A desktop application with a simulation core independent of
presentation.

No game engine.

## Recommended libraries

Keep dependencies intentionally small.

### UI

**Terminal.Gui** is a strong candidate for a console-style interactive
UI.

Alternative/fallback:

-   plain `System.Console` for the first prototype;
-   a lightweight custom renderer later if necessary.

### Serialization

-   `System.Text.Json`

### Logging

-   `Microsoft.Extensions.Logging`
-   optionally Serilog if structured file logging becomes valuable.

### Dependency injection / hosting

-   `Microsoft.Extensions.Hosting`
-   built-in Microsoft dependency injection.

### Testing

-   xUnit or NUnit;
-   FluentAssertions if desired;
-   Moq/NSubstitute only where mocking genuinely improves tests.

### Static analysis

-   .NET analyzers;
-   StyleCop only if the team wants stricter style enforcement;
-   SonarQube/SonarCloud if project scale justifies it.

### Persistence

Start with JSON save files.

Move to SQLite only if querying/history/state volume makes file
snapshots insufficient.

Potential library:

-   `Microsoft.Data.Sqlite`

## Architecture

Recommended separation:

``` text
ConsoleCity
├── ConsoleCity.App
├── ConsoleCity.Core
├── ConsoleCity.Simulation
├── ConsoleCity.Economy
├── ConsoleCity.Technology
├── ConsoleCity.Progression
├── ConsoleCity.WorldGeneration
├── ConsoleCity.Infrastructure
├── ConsoleCity.UI
├── ConsoleCity.Tests
└── ConsoleCity.Testing
```

## Core principle

The simulation must not depend on the UI.

``` text
UI
 ↓
Application services
 ↓
Simulation/domain
 ↓
Persistence / infrastructure
```

The simulation should be runnable headlessly for automated tests.

## Deterministic RNG

Define:

``` csharp
public interface IRandomSource
{
    int Next(int minInclusive, int maxExclusive);
    double NextDouble();
}
```

Inject it into systems requiring randomness.

## Clock

Define:

``` csharp
public interface ISimulationClock
{
    SimulationTime Now { get; }
}
```

The engine should never depend directly on `DateTime.Now`.

## Events

Use domain events or an internal event bus for loose coupling.

Example:

``` text
PopulationMoved
BuildingCompleted
PowerShortageStarted
BusinessClosed
TechnologyUnlocked
```

## Performance

Start simple.

Optimise only after profiling.

Likely future optimisation areas:

-   spatial indexing;
-   pathfinding;
-   agent scheduling;
-   network flow;
-   large collection updates.

## Parallelism

Do not introduce parallel simulation updates until deterministic
ordering and thread safety are understood.

If parallelism is later used, partition independent systems/agents and
preserve deterministic results.
