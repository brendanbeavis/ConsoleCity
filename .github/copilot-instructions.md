# ConsoleCity Development Instructions

ConsoleCity is a ground-up city/world simulation written in C# using .NET 10.

The /docs directory contains the authoritative design documentation.

## General Guidelines
- Before implementing a significant feature:
  - Read the relevant documentation in /docs.
  - Understand the existing domain model.
  - Reuse existing concepts rather than creating duplicates.
  - Do not invent major domain concepts without justification.
  - Preserve the separation between simulation and presentation.

## Code Structure
- Use practical file names (no generic Class1/UnitTest1).
- Place each class/type in its own .cs file.
- Put model types in a Models subfolder with one model per file.

## Architecture
Core
→ World  
→ Agents  
→ Economy  
→ Infrastructure  
→ Transport  
→ Services  
→ Simulation  
→ Game  
→ Presentation  

## Design Principles
- The simulation must never depend on the Console/UI project.
- Domain models should not contain UI concerns.
- Prefer explicit domain models over generic dictionaries or untyped data.
- Prefer composition and clear interfaces where multiple subsystems interact.
- Simulation behaviour must be deterministic when provided with a deterministic random seed.
- Simulation time must be independent of real-world wall-clock time.
- Avoid premature optimisation.
- Do not introduce a game engine.
- Do not introduce Entity Framework or a database unless explicitly required.
- Do not introduce ECS unless profiling demonstrates that it is necessary.

## Testing
- Write unit tests for domain behaviour and simulation rules.
- When uncertain about a design decision, inspect the relevant /docs files before making assumptions.