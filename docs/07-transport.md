# 7. Transport

## Transport model

Transport is a network connecting people, businesses, services and
resources.

## Road

Roads support:

-   private cars;
-   buses;
-   trucks;
-   emergency vehicles;
-   service vehicles.

Road attributes:

-   capacity;
-   speed;
-   lanes;
-   condition;
-   congestion.

## Pedestrian network

Paths and walkable streets provide low-cost local movement.

Pedestrian accessibility affects:

-   housing desirability;
-   commercial activity;
-   park usage;
-   school accessibility.

## Rail

Rail supports:

-   passenger trains;
-   freight trains.

Rail is expensive to build but efficient for high-volume movement.

## Public transport

Initial modes:

-   bus;
-   train;
-   tram;
-   metro.

Possible future modes:

-   ferry;
-   cable transit;
-   autonomous transit.

## Air transport

Airports support long-distance passenger and freight connections.

They should be introduced after the basic regional transport simulation
works.

## Ports

Ports support:

-   imports;
-   exports;
-   bulk resources;
-   container freight.

## Freight

Freight is a first-class transport demand.

Typical flow:

``` text
farm → processor → warehouse → shop → household
mine → factory → warehouse → construction site
```

## Route selection

Agents should select routes based on:

-   travel time;
-   cost;
-   congestion;
-   availability;
-   preference.

Emergency vehicles should use priority routing.

## Congestion

Congestion is produced from:

``` text
traffic demand / effective road capacity
```

Congestion should increase travel time rather than merely being a
cosmetic map value.

## Transport feedback

Transport quality influences:

-   employment accessibility;
-   business productivity;
-   land value;
-   emergency response;
-   migration;
-   city growth.

Poor transport can therefore create a self-reinforcing decline.
