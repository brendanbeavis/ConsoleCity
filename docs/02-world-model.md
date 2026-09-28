# 2. World Model

## Hierarchy

``` text
World
├── Regions
│   ├── Cities
│   │   ├── Districts
│   │   │   ├── Plots
│   │   │   │   ├── Buildings
│   │   │   │   ├── Businesses
│   │   │   │   └── Services
│   │   │   └── Local infrastructure
│   │   └── Transport network
│   └── Rural / natural areas
├── Global infrastructure
├── Technology state
├── Economy
├── Government / civic state
├── Population
├── Events
└── Simulation clock
```

## World

The top-level simulation container. It owns:

-   seed;
-   clock;
-   geography;
-   regions;
-   global resources;
-   global technology;
-   population registry;
-   organisations;
-   transport network;
-   infrastructure networks;
-   event queue;
-   progression state.

## Region

A broad geographic area containing one or more cities, rural land,
resources and connections.

Regions are useful for:

-   inter-city transport;
-   resource distribution;
-   regional economies;
-   large-scale infrastructure;
-   migration;
-   regional government;
-   regional technology effects.

## City

A population centre with:

-   districts;
-   residential areas;
-   workplaces;
-   services;
-   roads;
-   utilities;
-   local government;
-   economic activity.

Cities can begin as small settlements and grow organically.

## District

A grouping of nearby plots with a local identity or planning purpose.

Possible district types:

-   residential;
-   commercial;
-   industrial;
-   office;
-   mixed;
-   civic;
-   agricultural;
-   entertainment;
-   logistics.

District classification should influence desirability and allowed
development but should not necessarily hard-lock every plot.

## Plot

A variable-sized collection of grid cells representing developable land.

A plot may contain:

-   a house;
-   apartment;
-   shop;
-   office;
-   factory;
-   farm;
-   school;
-   police station;
-   fire station;
-   park;
-   utility facility;
-   transport facility;
-   other building.

Plots are deliberately more meaningful than individual map cells. The
grid is a spatial implementation detail; the plot is a simulation
object.

## Connections

Important objects are connected through typed networks:

-   road;
-   rail;
-   pedestrian;
-   electricity;
-   water;
-   sewage;
-   waste;
-   telecom;
-   fuel;
-   supply/logistics.

## Object lifecycle

Most world objects follow:

``` text
Planned → Constructing → Operational → Degraded → Abandoned → Demolished
```

Not every object needs every state.

## Identity

Every persistent object should have:

-   unique ID;
-   object type;
-   creation tick;
-   location;
-   owner/controller;
-   state;
-   optional parent/region/city references.

## Spatial model

Start with a discrete 2D grid.

Each cell should support:

-   terrain type;
-   elevation;
-   water state;
-   resource deposits;
-   occupancy;
-   road/rail/path flags;
-   development suitability.

Higher-level objects reference collections of cells.
