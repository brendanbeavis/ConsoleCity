# ConsoleCity — Phase 6: Player Construction and Development

## Copilot Prompt

Read the existing ConsoleCity implementation and all relevant documentation in `/docs`, especially gameplay, world, construction, buildings, economy, infrastructure and transport documentation.

Implement **Phase 6: Player Construction and Development**.

The objective is to allow the player to deliberately alter the simulation through construction and development while preserving the autonomous simulation model.

## Player role

The player represents the civilization/governing force defined by the game design.

The player should be able to make decisions that alter the world, but the world must continue operating autonomously.

Do not turn the simulation into a scripted city builder.

## Construction system

Implement a reusable construction/development mechanism supporting the documented buildable objects.

Potential categories include:

- Roads
- Residential buildings
- Commercial buildings
- Industrial buildings
- Farms
- Parks
- Power infrastructure
- Water infrastructure
- Service facilities
- Other documented structures

Use the actual list in `/docs` as the authority.

## Construction lifecycle

A build operation should support an appropriate lifecycle such as:

`Requested -> Validated -> Funded -> Under Construction -> Completed`

and where applicable:

`Cancelled / Failed / Demolished`

Construction should have:

- Location
- Cost
- Requirements
- Construction duration
- Required resources/labour where appropriate
- Resulting world object

Do not instantly create complex structures unless the design explicitly calls for instant construction.

## Validation

Validate:

- Plot availability
- Zoning/rules where applicable
- Required infrastructure
- Player funds
- Construction prerequisites
- Technology/unlocks where applicable
- Spatial conflicts
- Other documented constraints

Validation should be performed by domain/game systems, not by the Console UI.

## Player commands

Add Console commands sufficient to test the feature, such as:

`build`
`demolish`
`zone`
`construct`
`construction`
`inspect`

Use the existing command conventions.

## Economy interaction

Construction should consume resources/money and create economic activity.

Where appropriate:

- Construction companies receive work.
- Labour is consumed.
- Materials are consumed.
- Infrastructure demand changes.
- Completed buildings create new economic/service capacity.

Do not implement a fake transaction that bypasses the economy unless the design explicitly calls for it.

## Architecture

Keep player/game decisions in `ConsoleCity.Game`.

Keep physical world objects in `ConsoleCity.World`.

Keep construction economic effects in the relevant economy/infrastructure systems.

Do not put game rules into the Console presentation project.

## Testing

Test:

- Valid construction.
- Invalid construction.
- Insufficient funds.
- Occupied plots.
- Prerequisites.
- Construction duration.
- Completion.
- Demolition.
- Economic effects.
- Infrastructure effects.

## Completion criteria

A player should be able to create meaningful changes to a small city using Console commands, and those changes should feed into the existing autonomous simulation.

Do not implement graphical placement yet.
