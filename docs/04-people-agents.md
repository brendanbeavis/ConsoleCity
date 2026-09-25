# 4. People and Agents

## Purpose

People are the primary autonomous agents. They create the emergent
behaviour that makes the world feel alive.

## Person model

A person should contain approximately:

-   identity;
-   age;
-   life stage;
-   household;
-   residence;
-   workplace;
-   education;
-   skills;
-   income;
-   money;
-   expenses;
-   needs;
-   health state;
-   relationships;
-   preferences;
-   current activity;
-   current location;
-   transport preference;
-   satisfaction;
-   employment state.

## Life stages

Initial model:

1.  Child
2.  Teenager
3.  Young adult
4.  Adult
5.  Senior

Life stages affect:

-   education;
-   employment;
-   housing;
-   travel;
-   consumption;
-   family formation;
-   service usage.

## Household

People are grouped into households.

A household has:

-   members;
-   income;
-   expenses;
-   home;
-   savings;
-   debt;
-   food demand;
-   utility demand;
-   transport assets;
-   satisfaction.

Households are the primary economic consumption unit.

## Needs

Initial needs:

-   shelter;
-   food;
-   water;
-   safety;
-   healthcare;
-   social interaction;
-   education;
-   employment/income;
-   recreation;
-   mobility.

Needs should be represented numerically but exposed through
understandable categories.

## Decision model

A utility-based decision model is recommended initially.

Example:

``` text
utility(action) =
    needPressure
  + expectedBenefit
  + preference
  + affordability
  - travelCost
  - risk
  - inconvenience
```

Possible actions:

-   go to work;
-   buy food;
-   visit a park;
-   visit a doctor;
-   attend school;
-   travel home;
-   seek employment;
-   move house;
-   buy transport;
-   socialise.

Behaviour trees can be used for complex workflows, but a simple utility
model is preferable for the first implementation.

## Employment

People seek workplaces based on:

-   available jobs;
-   wage;
-   skill fit;
-   travel time;
-   preference;
-   workplace desirability.

Businesses publish job opportunities.

## Housing

Housing choice depends on:

-   affordability;
-   household size;
-   accessibility;
-   district desirability;
-   safety;
-   services;
-   travel time;
-   preferences.

## Relationships

Start with household/family relationships. Expand later to friendship
and social networks.

## Migration

People may move between cities when:

-   employment is unavailable;
-   housing is unaffordable;
-   household circumstances change;
-   another city offers a sufficiently better utility;
-   disasters or shortages make the current location undesirable.

Migration should be probabilistic rather than instantaneous
teleportation.

## Agent simulation principle

Agents do not need perfect intelligence. They need consistent,
explainable behaviour that produces plausible aggregate outcomes.
