# Hope income and performance validation — October 27, 2026

The permanent base Hope multiplier is 5. It applies through GlobalMultiplier to
passive production (including offline production), taps, and newly started starter
Gather rewards. Costs, chapter gifts, navigation, and timer durations are unchanged.
Code-based simulated-time boosts remain separate. Existing pending starter rewards
retain their saved amount; legacy pending gathers without a reward field retain 5.

## Implemented optimizations

- Calculate the global multiplier once per total-production query, rather than once
  per generator. Reuse static synergy ID arrays.
- Tick cooldowns without allocating a dictionary-key snapshot.
- Cache up to 2,048 derived localization captions; invalidate on language changes
  and added translations, and bound dynamic-caption memory growth.
- Defer story translation JSON loading/parsing until a non-English language needs it.
- Reuse the story reader's text-component list and portrait cache. Apply font
  configuration only when language changes. Skip unchanged safe-area layout and
  texture/aspect updates.

## Reproduce

```powershell
dotnet run --project 'E:\ZendaGame\Genesis\Tools\PerformanceValidation\PerformanceValidation.csproj' --configuration Release --verbosity quiet -tl:off
dotnet run --project 'E:\ZendaGame\Genesis\Tools\Validation\IdleRegression\IdleRegression.csproj' --verbosity quiet -tl:off
dotnet run --project 'E:\ZendaGame\Genesis\Tools\StoryLocalizationValidation\StoryLocalizationValidation.csproj' --verbosity quiet -tl:off
dotnet build 'E:\ZendaGame\Genesis\Genesis.Sanctum.csproj' --no-restore --verbosity quiet -tl:off
```

## Observed results

Same Release benchmark, 2,000 warmups and 20,000 measured calls per operation:

| Operation | Before | After | Allocated bytes before / after |
| --- | ---: | ---: | ---: |
| Total production query | 900.51 ms | 203.81 ms | 1,492,112 / 51,984 |
| Tick without production | 5.97 ms | 4.38 ms | 800,040 / 576 |
| Repeated numeric localization | 42.63 ms | 2.01 ms | 15,840,808 / 576 |

These are isolated .NET measurements, not Unity player FPS, GPU, or cold-start
measurements. Timing and allocation totals include measurement/runtime overhead.
The production result changed from 0.005 to 0.025 as intended.

- 58 idle regression checks passed, including exact fivefold income, save/offline
  behavior, and localization cache invalidation/eviction.
- 428 story pagination/metadata checks passed; 261 UI translations validated.
- Build passed with zero errors and five warnings in unchanged catalog/audio code.
- Unity runtime text-fit, navbar, and ending scripts were attempted but could not
  find an initialized game instance ("Play mode required"). They are not counted
  as passing. Full device visual checks, Unity CPU/GPU frame-time profiling, and
  cold-start/load-time measurements remain outstanding.

No artwork or rendering quality was reduced. Full translation coverage is outside
this change and remains incomplete.