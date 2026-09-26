# ConsoleCity — Phase 5: City Services

## Copilot Prompt

Read the existing ConsoleCity implementation and all relevant documentation in `/docs`, especially services, agents, buildings, economy, infrastructure and gameplay documentation.

Implement **Phase 5: Richer City Services**.

The objective is to make city services functional simulation systems that respond to population demand and available capacity.

## Services

Implement the services documented by ConsoleCity, including where applicable:

- Police
- Fire
- Healthcare
- Education
- Emergency services
- Government
- Retail
- Recreation
- Other documented service types

Do not invent large new service categories without checking `/docs`.

## Common service model

Where appropriate, establish reusable concepts such as:

- ServiceProvider
- ServiceFacility
- ServiceType
- ServiceCapacity
- ServiceDemand
- ServiceRequest
- ServiceCoverage
- ServiceQuality
- ServiceResponse

Do not force all services into an abstraction if their domain behaviour genuinely differs.

## Demand

Service demand should arise from the simulation.

Examples:

- Population creates healthcare demand.
- Students create education demand.
- Fires create emergency/fire demand.
- Crime/events create police demand.
- Households create retail/recreation demand.

Avoid hard-coded arbitrary service usage where demand can be derived from existing simulation state.

## Capacity and outcomes

Services should have finite capacity.

Where demand exceeds capacity, model appropriate consequences rather than simply fulfilling every request.

Examples:

- Longer emergency response.
- Unserved healthcare demand.
- School capacity shortages.
- Reduced service coverage.
- Increased pressure on facilities.

Use the documented game/simulation rules where they already exist.

## Service facilities

Service facilities should exist in the world and have relationships with:

- Location
- Capacity
- Staff
- Utilities
- Transport access
- Operating state
- Service area

Do not bypass the existing world model.

## Events

Use simulation events for important occurrences such as:

- Fire started
- Fire responded to
- Fire extinguished
- Emergency request
- Service capacity exceeded
- Facility opened/closed

Use existing event architecture rather than inventing a parallel event mechanism.

## Observability

Add statistics for:

- Service facilities
- Capacity
- Demand
- Utilisation
- Unserved demand
- Response times where appropriate
- Service coverage
- Major service incidents

## Testing

Add unit and integration tests for:

- Service demand.
- Capacity.
- Facility availability.
- Service requests.
- Response.
- Capacity shortages.
- Interactions with utilities and transport.
- Long-running simulation behaviour.

## Completion criteria

A city should now have services that respond to actual population and world conditions rather than merely existing as building types.

Do not implement graphical rendering or player construction in this phase.
