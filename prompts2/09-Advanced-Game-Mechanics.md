# ConsoleCity — Phase 9: Advanced Game Mechanics

## Copilot Prompt

Read the entire existing ConsoleCity implementation and all design documentation in `/docs`.

Implement **Phase 9: Advanced Game Mechanics**.

This phase should turn the functioning simulation into the game layer described by the ConsoleCity design.

Do not begin by adding arbitrary game features. First identify the game mechanics already defined in `/docs`, then implement them consistently.

## Core principle

The simulation should remain capable of operating autonomously.

The player does not micromanage every person.

The player acts at the civilization/city/governance level.

The challenge comes from making decisions while the underlying world continues to evolve.

## Progression

Implement the documented progression system.

Potential concepts include:

- Technology
- Research
- Milestones
- Unlocks
- Development levels
- Civilization progression
- New building types
- New infrastructure
- New services
- New economic capabilities

Progression should emerge from simulation achievements and/or documented player actions rather than arbitrary button presses.

## Roguelike / modifier system

Implement the documented shop/upgrade/modifier mechanics.

Modifiers should be grouped according to the existing design categories, including where applicable:

- Construction
- Agents
- Production/trade
- Migration
- Research
- Policy/services
- Events/environment
- Specialisation

A modifier should have:

- Identity
- Description
- Eligibility
- Effect
- Duration where appropriate
- Stacking rules
- Acquisition method
- Removal/expiry rules where appropriate

Avoid hard-coding modifier effects throughout unrelated systems.

Use a centralised but extensible modifier/effect mechanism.

## Cycles and milestones

Implement the documented cycle/milestone mechanics.

A cycle should provide a meaningful gameplay transition without simply deleting the simulation.

Where the design specifies persistent progression, preserve it.

Where the design specifies temporary modifiers or world changes, resolve them correctly.

## Policies

Implement player-level policies where documented.

Examples may include:

- Tax policy
- Immigration policy
- Infrastructure priorities
- Education investment
- Healthcare investment
- Development priorities
- Environmental policy
- Economic policy

Policies should alter simulation parameters rather than directly forcing outcomes.

For example:

`Policy -> changed incentives/capacity/cost -> agent/economic behaviour -> emergent outcome`

rather than:

`Policy -> set population to X`

## Specialisation

Implement meaningful city/district specialisation if supported by the documentation.

Potential specialisations include:

- Industrial
- Commercial
- Agricultural
- Residential
- Technology
- Logistics
- Services
- Recreation

Specialisation should influence production, demand, employment, migration, infrastructure and/or progression where documented.

## Events

Implement the documented world-event system.

Events should include appropriate categories such as:

- Economic
- Environmental
- Infrastructure
- Social
- Political/governance
- Natural
- Technological

Events should modify simulation conditions and allow systems to react.

Avoid creating purely scripted narrative events unless the game design explicitly calls for them.

## Player decision layer

The player should make decisions at meaningful intervals rather than being required to constantly micromanage individual entities.

The game should remain interesting while the simulation is running autonomously.

## Difficulty and challenge

Implement challenge through interacting constraints such as:

- Limited resources
- Infrastructure capacity
- Population needs
- Economic instability
- Construction costs
- Service capacity
- Environmental constraints
- Technology requirements
- Competing priorities

Do not create difficulty by simply multiplying random penalties.

## Long-term gameplay

The game should support a progression such as:

`Found -> Develop -> Expand -> Specialise -> Research -> Adapt -> Overcome events -> Reach new capabilities`

The exact progression must follow the existing design documents.

## Save/load

Ensure advanced game state integrates with the Phase 2 save/load system.

Persist:

- Player progression
- Technologies
- Unlocks
- Policies
- Modifiers
- Cycle state
- Relevant event state
- Any other documented persistent game state

## Observability

Expose enough information for the player to understand why the simulation is changing.

Avoid hidden modifiers that make outcomes impossible to explain.

Provide inspection/history where appropriate.

## Testing

Add tests for:

- Unlocks.
- Technology progression.
- Modifiers.
- Stacking/expiry.
- Policies.
- Events.
- Cycles.
- Player progression.
- Save/load persistence.
- Long-running game progression.
- Deterministic outcomes where applicable.

## Completion criteria

ConsoleCity should now have two clearly separated layers:

### Simulation

An autonomous evolving world.

### Game

A player-directed progression and decision system operating over that world.

The player should have meaningful strategic decisions without manually controlling every agent.

Do not compromise the simulation architecture to implement game mechanics.
