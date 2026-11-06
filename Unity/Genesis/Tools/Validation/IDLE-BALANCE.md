# Idle / AFK progression and validation

The release scene uses `GenesisSanctum`, not `GenesisSession`. Sanctum now saves its own game,
gathering cycles, VIP speed, presentation choices, companion and journey state locally.
Autosave runs every five seconds and on purchases, story completion/rewards, VIP changes,
reset, journey actions, loss of focus, suspension and normal exit. Offline time is calculated
locally in UTC; no account, server connection or network clock is required.

## Calendar balance

`EconomyMath.CampaignProductionRate` scales all fairy production to 0.01 of the original
catalog rate. Stable generator IDs, costs, chapter objectives, rewards and save counts are
unchanged. It is deliberately shared by rate labels, gathering, world income and offline
calculations. VIP codes replace the current simulation speed (10x, 30x, 50x); they do not
multiply together or accelerate UI animation / Unity's global clock. The existing GONZALO
100x code remains supported.

The conservative reference simulation starts with five taps and no other tapping. At each
visit it does two passes through ready stories and recruits; buys each resident once; and
reinvests in the most recently unlocked progression fairy. It uses no journeys, prestige,
relics, VIP codes or purchases. Story reading time is not simulated, and inefficient spending
or skipped visits can extend completion. This is a tested balance target, not a guarantee
for arbitrary player decisions.

Measured completion of all 107 chapters and ownership of all 255 fairies:

| Check-in interval | Days at 1x |
| --- | ---: |
| 6 hours | 11 |
| 12 hours | 17 |
| 24 hours | 30 |

Run the pure C# model regression from PowerShell (requires .NET 10 SDK and Unity's restored
Newtonsoft package):

```powershell
dotnet run --project 'E:\ZendaGame\Genesis\Tools\Validation\IdleRegression\IdleRegression.csproj'
```

In Unity, outside Play mode, run **Genesis > Validate > Idle persistence, VIP and completion**.
This creates and destroys a temporary component without saving scenes or modifying player
save files. The report is `E:\ZendaGame\Genesis\Builds\idle-sanctum-validation.txt`.

## Storage and completion

The release save is `genesis_sanctum_v2.json` in `Application.persistentDataPath`, with a backup
and temporary-write recovery. Editor previews use `genesis_sanctum_editor_v2.json` instead.
Unreadable saves are preserved rather than overwritten with a fresh game. Older
`genesis_unity_v1.json` saves in the same directory are imported without deleting them.
There is no historical Sanctum save to recover from builds that never saved anything.

WebGL explicitly flushes its virtual filesystem to IndexedDB. Saves remain specific to the
browser/site storage; private browsing, clearing site data, removing the app, changing its
storage identity or denying storage can remove or prevent persistence. Browser flush failures
are logged in the browser console. A local save is not cloud backup or cross-device sync.

Offline income has no 12-hour cap or 50% penalty. It advances saved partial gathering cycles
at the saved VIP rate and completes any existing journey, whose reward is still claimed once
by the player. It never auto-purchases fairies or invents additional journeys. Backward clock
changes award zero elapsed time; a fully local game cannot securely reject forward clock
manipulation. Temporary inspiration/emotion animations are intentionally transient.

Completion is all stories read (optional gifts are not required), or all chapter gifts already
claimed in an older save. Credits appear once automatically, and can be reopened from a fairy
hexagon on the main page. Close continues indefinitely; reset uses explicit confirmation and
clears the completion marker, progress, VIP speed and journey state.

Still required for platform acceptance: rebuild each player, verify real process close/reopen,
phone suspend/resume and browser reload with persistent storage. Pure model and editor
round-trip tests do not replace those device/browser tests.