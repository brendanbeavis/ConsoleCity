# 13. Economy

## Economic actors

-   households;
-   businesses;
-   farms;
-   industrial organisations;
-   service providers;
-   government.

## Money

Use a simplified monetary system.

Every actor can have:

-   cash;
-   income;
-   expenses;
-   assets;
-   debt;
-   credit/financial capacity.

Avoid simulating banking mechanics initially.

## Household economy

Income comes primarily from:

-   wages;
-   business ownership;
-   transfers.

Expenses include:

-   housing;
-   food;
-   utilities;
-   transport;
-   healthcare;
-   education;
-   recreation.

Households save or spend surplus money.

## Business economy

Businesses:

-   hire workers;
-   buy inputs;
-   produce goods/services;
-   sell output;
-   pay wages;
-   pay operating costs;
-   invest;
-   pay taxes where applicable.

## Production

A production recipe defines:

``` text
Inputs → process → outputs
```

Example:

``` text
100 grain + energy + labour
→ food processing
→ 80 packaged food
```

The exact ratios should be data-driven.

## Prices

Prices can be modelled using a simplified supply/demand adjustment.

Avoid perfect market equilibrium.

Example:

``` text
priceAdjustment =
  basePrice × demandPressure × scarcityModifier
```

## Logistics

Economic production is limited by transport and inventory.

A factory with no steel should stop producing even if it has money.

## Trade

Cities can trade with:

-   other cities;
-   regions;
-   ports;
-   external markets.

External trade can initially be abstracted as an infinite/limited market
with configurable prices.

## Economic health

Useful aggregate indicators:

-   employment;
-   unemployment;
-   wages;
-   household income;
-   consumer demand;
-   production;
-   inventory;
-   business closures;
-   construction;
-   trade balance;
-   government revenue.

These are indicators, not direct player objectives unless a scenario
introduces them.
