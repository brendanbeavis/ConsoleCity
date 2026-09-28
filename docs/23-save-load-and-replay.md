# 23. Save, Load and Replay

## Save format

Use a versioned, structured format.

Recommended initial approach:

-   JSON for development/debugging;
-   compressed JSON or binary snapshot later if required.

## Save contents

A save should contain:

-   schema version;
-   world seed;
-   simulation tick;
-   world state;
-   agents;
-   organisations;
-   buildings;
-   networks;
-   economy;
-   technology;
-   progression;
-   active events;
-   RNG state where reproducibility requires it.

## Save slots

Support:

-   manual saves;
-   autosaves;
-   milestone saves;
-   quicksave.

## Versioning

Every save must include a schema version.

Migration strategy:

``` text
Save v1
→ migration
→ Save v2 model
```

Never silently assume an old save is current.

## Deterministic replay

Long term, the engine should be capable of replaying a world from:

-   initial seed;
-   player action log;
-   deterministic simulation.

This is valuable for debugging and testing.

## Action log

Record player interventions:

``` text
tick 1842
BUILD
type=Road
location=...
```

This can later support:

-   replay;
-   debugging;
-   undo-like inspection;
-   experiment comparison.

## Autosave safety

Use atomic save replacement:

``` text
write temp
→ flush
→ validate
→ replace current
```

Keep at least one recovery save.
