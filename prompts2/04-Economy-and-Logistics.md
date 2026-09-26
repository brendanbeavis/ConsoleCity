# ConsoleCity — Phase 4: Economy and Logistics

## Copilot Prompt

Read the existing ConsoleCity implementation and all relevant documentation in `/docs`, especially the economy, agents, buildings, businesses, transport and simulation documents.

Implement **Phase 4: Richer Economy and Logistics**.

The objective is to make ConsoleCity's economy a functioning interconnected system rather than a collection of independent money/employment values.

## Economic concepts

Implement the documented concepts for:

- Businesses
- Employment
- Wages
- Household income
- Production
- Consumption
- Inventory
- Goods
- Supply
- Demand
- Markets
- Prices
- Revenue
- Costs
- Profit/loss
- Business opening/closure
- Trade where supported
- Government revenue/expenditure where already documented

Do not introduce unnecessary economic complexity. Start with a coherent, functioning model.

## Production chain

Support a basic chain such as:

`Producer -> Inventory/Warehouse -> Transport -> Retail/Business -> Household -> Consumption`

Use the actual domain terminology already established in `/docs`.

The system should allow shortages and surpluses to emerge from production, inventory, transport and demand.

## Pricing

Implement a simple deterministic pricing mechanism based on documented supply/demand concepts.

Avoid attempting to reproduce real-world financial markets.

Prices should be observable and should influence behaviour where appropriate.

## Employment

Businesses should have labour requirements.

People should be able to:

- Seek employment.
- Become employed.
- Earn wages.
- Lose employment when appropriate.
- Change consumption behaviour when income changes.

Business viability should be influenced by revenue, costs, production and demand.

## Logistics

Implement movement of goods through the existing transport model.

Separate:

- Economic demand for goods
- Physical inventory
- Transport requests
- Goods in transit
- Delivered goods

Do not make a business magically receive inventory without a logistics mechanism.

## Emergent interactions

The implementation should support chains such as:

`Production shortage -> lower supply -> price change -> household behaviour -> business revenue -> employment changes`

and:

`Transport disruption -> delayed goods -> inventory shortage -> reduced sales`

These interactions should emerge from subsystem rules rather than being scripted scenarios.

## Architecture

- Keep economy logic in `ConsoleCity.Economy`.
- Keep physical movement in `ConsoleCity.Transport`.
- Keep agent behaviour in `ConsoleCity.Agents`.
- Use interfaces/events between systems where appropriate.
- Do not create a god-object economy manager.
- Do not put economy logic into the Console/UI.

## Observability

Add useful metrics including, where supported:

- Total economic output
- Total household income
- Employment/unemployment
- Business count
- Business openings/closures
- Production
- Consumption
- Inventory
- Prices
- Supply/demand
- Goods in transit

## Testing

Add tests for:

- Production.
- Consumption.
- Inventory.
- Pricing.
- Employment.
- Business viability.
- Supply shortages.
- Logistics delays.
- Economic feedback loops.
- Deterministic simulation.

## Completion criteria

A small city should have an economy where businesses produce goods/services, people earn income and consume, goods can move through the city, shortages can occur, prices can respond, and economic changes can affect employment and household behaviour.

Do not implement graphical UI or advanced player mechanics in this phase.
