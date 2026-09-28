# ConsoleCity --- Solution Design

**Status:** Current design baseline\
**Date:** 2026-09-22\
**Technology target:** .NET 10 / C# / Visual Studio 2026\
**Presentation:** Retro console/CLI-first, with a lightweight 2D/basic
sprite UI where mouse interaction is useful\
**Game-engine constraint:** No Unity, Godot, Unreal, or other game
engine.

## Purpose

This repository is the design source of truth for ConsoleCity: a
living-world city simulation in which the player acts as a civilization
steward rather than directly controlling individual people.

The world should be able to operate autonomously. The player's role is
to establish settlements, provide infrastructure and services, shape
economies and transport, unlock technology, respond to problems, and
guide the long-term development of cities and regions.

## Core design pillars

1.  **Living simulation** --- people, businesses, infrastructure,
    transport, resources and institutions interact continuously.
2.  **Player as civilization** --- the player influences systems rather
    than micromanaging every person.
3.  **Build and expand** --- creating cities and connecting them is a
    major source of play.
4.  **Observe and react** --- the simulation should produce stories
    without requiring scripted campaigns.
5.  **Economic and resource management** --- production, jobs,
    logistics, money, utilities and land use matter.
6.  **Technology progression** --- research and milestones unlock new
    capabilities.
7.  **Roguelike progression** --- development credits allow persistent
    or run-level upgrades/perks and create meaningful strategic
    variation.
8.  **Retro presentation** --- simulation depth is prioritised over
    visual complexity.
9.  **Deterministic simulation** --- seeded randomness enables
    reproducible worlds and reliable automated testing.
10. **Data-driven content** --- object definitions, technologies,
    upgrades and balancing values should live outside core simulation
    logic where practical.

## Repository document map

-   `01-vision-and-design-principles.md` --- overall game concept and
    design principles.
-   `02-world-model.md` --- world hierarchy and simulation entities.
-   `03-simulation-cycle.md` --- time, ticks, system ordering and
    autonomous evolution.
-   `04-people-agents.md` --- people, households, needs, decisions and
    life stages.
-   `05-rico-land-use.md` --- Residential, Industrial, Commercial,
    Office and Farming land use.
-   `06-buildings-and-organisations.md` --- buildings, businesses and
    organisations.
-   `07-transport.md` --- roads, rail, pedestrian and public/freight
    transport.
-   `08-services.md` --- emergency, health, education, civic and leisure
    services.
-   `09-infrastructure.md` --- electricity, water, sewage, waste,
    telecoms and fuel.
-   `10-parks-and-recreation.md` --- parks, playgrounds, sports and
    attractions.
-   `11-farms-and-food.md` --- farms, agriculture, food production and
    supply chains.
-   `12-terrain-and-resources.md` --- terrain, natural resources and
    extraction.
-   `13-economy.md` --- households, businesses, money, production,
    logistics and trade.
-   `14-government-and-civic-systems.md` --- civic administration and
    public decisions.
-   `15-technology-and-research.md` --- research, technology tree and
    unlocks.
-   `16-events-and-emergent-stories.md` --- simulation events and
    consequences.
-   `17-core-game-loop.md` --- player loop and progression.
-   `18-ui-and-menus.md` --- new/load game, simulation controls and
    editing interfaces.
-   `19-world-creation-and-editing.md` --- world generation and
    construction tools.
-   `20-credits-and-roguelike-progression.md` --- development credits
    and run progression.
-   `21-roguelike-upgrades.md` --- proposed upgrade/perk catalogue.
-   `22-additional-gamification.md` --- optional gamification systems.
-   `23-save-load-and-replay.md` --- persistence and reproducibility.
-   `24-technical-architecture.md` --- recommended technical stack and
    architecture.
-   `25-code-structure-and-standards.md` --- solution structure,
    conventions and engineering standards.
-   `26-testing-and-automation.md` --- automated testing, simulation
    tests and mock data.
-   `27-data-driven-content.md` --- schemas and configuration strategy.
-   `28-mvp-and-implementation-roadmap.md` --- staged implementation
    plan.
-   `29-graphical-ui.md` --- graphical presentation architecture,
    rendering loop, camera model and Raylib boundary.

## Design vocabulary

**World** contains regions.\
**Region** contains cities and rural areas.\
**City** contains districts.\
**District** contains plots.\
**Plot** is a variable-sized collection of grid cells and is the
principal land-use/building location.

Objects can own, occupy, connect to, consume from, supply, employ,
transport, service or depend upon other objects.

## Current scope boundary

The initial implementation should favour a convincing systemic
simulation over exhaustive realism. Many real-world concepts can be
represented by abstractions rather than detailed physics or accounting.

The design is intentionally extensible: a small initial world should run
with a few thousand simulated agents, while the architecture should not
fundamentally prevent much larger worlds later.
