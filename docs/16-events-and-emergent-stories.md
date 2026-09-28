# 16. Events and Emergent Stories

## Event philosophy

Events should be consequences of simulation state whenever possible.

## Event categories

### Infrastructure

-   power outage;
-   water main failure;
-   bridge failure;
-   road closure.

### Environment

-   flood;
-   bushfire;
-   drought;
-   storm;
-   pollution incident.

### Economy

-   business closure;
-   shortage;
-   price spike;
-   industrial boom;
-   recession-like contraction.

### Society

-   migration wave;
-   housing shortage;
-   population boom;
-   service overload.

### Transport

-   congestion;
-   accident;
-   rail disruption;
-   freight bottleneck.

## Event model

``` text
Event
- id
- type
- location
- startTime
- duration
- severity
- causes
- affectedObjects
- consequences
- resolutionState
```

## Event causality

Events should expose cause chains.

Example:

``` text
Drought
→ farm output -35%
→ food supply tightens
→ food prices rise
→ household budgets tighten
→ consumer spending falls
→ retail revenue falls
```

This is more valuable than a generic notification saying "Drought
occurred."

## Random events

Random events can exist but should be bounded by plausible world
conditions.

A flood should be more likely in flood-prone geography.

## Player response

Events create opportunities to:

-   build;
-   repair;
-   allocate funding;
-   reroute transport;
-   expand services;
-   accept temporary consequences.

## Story log

Maintain a chronological event log that players can inspect later.
