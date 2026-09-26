Read the ConsoleCity architecture and simulation documentation in /docs and inspect the existing implementation.

Implement the first production-quality save/load system for ConsoleCity.

Requirements:

- Save the complete deterministic simulation state.
- Load a saved simulation and continue from exactly that state.
- Preserve simulation time.
- Preserve entity IDs.
- Preserve relationships between entities.
- Preserve random/simulation seed state where required for deterministic continuation.
- Preserve world, agents, economy, infrastructure, transport and game state that currently exists.
- Do not serialize transient services, dependency injection containers, loggers or presentation objects.
- Clearly distinguish persistent simulation state from runtime infrastructure.
- Use System.Text.Json initially unless the existing architecture provides a strong reason otherwise.
- Add version information to the save format so it can evolve later.
- Add tests for save → load → continue simulation.
- Add tests proving that loading a saved simulation produces equivalent observable state.

Add Console commands for:
    save <name>
    load <name>
    saves

Do not implement graphical rendering in this task.