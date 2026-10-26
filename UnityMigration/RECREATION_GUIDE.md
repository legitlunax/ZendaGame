# Unity recreation specification

## 1. Scope and non-negotiable product behavior

Genesis: Goddess of Creation is an idle creation game: hope becomes a living
universe rather than destroying one. Start with zero currency, no passive income,
no visible fairy/stars, and the first generator available for five Hope. Taps
create currency. Discoveries unlock sequentially; chapters are claimed separately.
Beyond the early cosmic sequence are Dao dimensions, Omnielle's confluence,
recognition of life's divinity, five living realm homes, Universal Emotions, and
human Earth life with possible simultaneous soul-presence in other realms.

Read every story row in the JSON; do not replace it with this summary. Spiritual
ideas are fictional worldbuilding, not claims of science or real player powers.
Earthly care and different beliefs are respected. Realm identity does not define
morality or rank. The main queen is explicitly 25, with adult rather than chibi
proportions. Historical form titles such as Child/Maiden are progression labels,
not a change to the character's age.

All story and wardrobe designs are collectible. New appearances do not replace or
automatically equip over earlier ones. Retain original designs when adding future
ones. Existing six wardrobe entries are color/wing-size/cosmetic variants, not six
authored costumes. Previous historical prototype meshes are not archived models.

## 2. Suggested Unity architecture (proposal, not existing C# code)

| Godot source | Unity responsibility |
| --- | --- |
| StageCatalog + Database | Immutable catalog DTOs, optional ScriptableObject assets |
| GameManager | Pure C# state/economy service + lifecycle MonoBehaviour adapter |
| CreationJourney | Chapter progress/reward service using stable chapter IDs |
| CosmicProgression | Discovery order and legacy-progress compatibility |
| FairyWardrobe / FairyAppearance | Catalog, selected design ID, visual configuration |
| Main | Canvas/panels/presenters, binding to state events |
| CosmicVisualizer | Tap target, background/companion effects, camera/emotion/design controls |
| FairyAvatar3D / QueenArtDetails | Character prefab, attachments, animation/presentation layer |
| CelestialSilk | Pipeline-specific shader/material recreation |
| CreationAudio | AudioSource pool, mute, signal/event hookups |
| tests/*.gd | EditMode domain tests + PlayMode layout/input/camera tests |

Keep state separate from presentation; no purchases or unlocks in animation code.
Use double for currency, production, costs, run/lifetime totals, timestamps, and
log/power calculations. Transform and color values can use float. Current Godot
counts/seeds are 64-bit integers; choose and test a compatible C# integer type.
Converting these to 32-bit silently is not parity. Source has no universal overflow
strategy for extreme long-run values; add finite/overflow guards deliberately.

The catalog contains JSON objects/dictionaries such as `fairy_names`, and old saves
use dictionaries for counts. Unity JsonUtility serialization follows engine rules
and has limitations: do not assume arbitrary dictionaries deserialize. Use a chosen
JSON library, or explicit list-based DTO conversion. The supplied converter uses
`[{"id":"...","value":0}]` for saved counts/relics. No serializer is bundled.

## 3. Exact economy and progression rules

Source authority: `SourceSnapshot/scripts/GameManager.gd`, `StageCatalog.gd`,
`CosmicProgression.gd`, `CreationJourney.gd`, `Database.gd`.

### Catalog generation

For generator index i, zero based:
- base_cost = 5 * 3.2^i
- cost_mult = 1.15 + min(i * 0.003, 0.15)
- base_prod = max(0.5, base_cost * 0.025)

Expanded values are exported; prefer them over recalculation at import. Discovery
level is highest owned generator index + 1, NOT number of unique owned generators.
A generator is discoverable if it is first, already owned, or discovery level >=
its index. This deliberately permits legacy saves with later layers to access
inserted earlier discoveries. `has_reached(id)` uses that highest-level rule too.
Unknown IDs cannot be purchased. Cards beyond the next discovery remain hidden.
Currency display name switches from Hope to Creation Potential at quark_filament.

### Production

- generator_qps = base_prod * count * 2^floor(count/25) * global_multiplier
- total_qps = sum generator_qps
- global = form.mult_bonus * (1 + current_unspent_seeds*0.25)
  * (1 + chronos_level*0.50) * harmony * living_unity * temporary_buff
- harmony = (1 + min(min(yin,yang),100)*0.01)
  * (1 + min(min(all five element counts),100)*0.02)
- living_unity = 1 + min(min(gaia_biosphere,all_seeing,omnipresence,omnipotence),50)*0.02
- temporary_buff = 3 while chrono timer > 0, otherwise 1
- tap = global_multiplier + total_qps * (0.01 + seraph_grace_level*0.02)

Accepted positive finite gains increase balance, run total and lifetime total.
Purchases subtract balance only. Evolution loops through all newly eligible forms
using current_run_quanta, not balance or lifetime. Seed spending reduces the seed
multiplier because the source uses unspent seeds, not total seeds ever earned.

### Purchasing

- discount = clamp(1 - loom_level*0.05, 0.5, 1)
- first = base_cost * discount * r^owned, r = cost_mult
- N-item cost = first * (r^N - 1)/(r - 1); one-item cost = first
- Buy modes: 1, 10, 100, MAX (-1)
- MAX: if balance < first choose amount 1 but unaffordable; otherwise
  max(1, floor(log(1 + balance*(r-1)/first)/log(r)))
- Exact affordability in source is balance >= cost. Test boundary rounding;
  fixtures use relative tolerance 1e-6 for calculated outputs, not for permission.

### Chapters and permanent unlocks

Active chapter = first chapter ID not in completed_chapters. Opening first_light
needs run total 10 and rewards 15. All discovery chapters need one owned generator
of their kind, target 1, and reward base_cost*0.25. Claim requires current active
chapter complete; it appends the stable ID, grants the reward, saves, and notifies
the UI. Duplicate claims cannot give rewards twice. Dimension chapter titles use
Build. Completed chapters survive Bloom; ownership of a generator alone does NOT
unlock a wardrobe design or emotion.

Historical chapter IDs are preserved in the exported catalog. Never generate IDs
from translated titles. Never reassign IDs when inserting/reordering chapters.

### Miracles, relics, Bloom, tick

- Miracles require has_reached(hadron_forge), and their own cooldown at zero.
- Genesis Chorus: max(50, total_qps*30) gain, cooldown 25s.
- Chrono-Weave: 3x global for 20s, cooldown 60s.
- Relic cost = base_seed_cost*(level+1); spend seeds. Loom max level 10;
  other relics have no purchase cap in source, though loader clamps to 1,000,000.
- Bloom requires has_reached(wormhole_nexus) and run total >= 1e6.
- earned seeds = max(1, floor((run_total/1e6)^0.35)); added to existing balance.
- Reset balance, run total, goddess stage, all generator counts, buff and cooldowns.
  Preserve lifetime total, relics, completed chapters, emotion/design unlocks and
  equipped design. Source also leaves buy mode unchanged.
- Tick: decrement buff/cooldowns first, calculate qps, add qps*delta, then increment
  autosave timer. At >=15s reset timer to zero and save. Emit state update.
  Preserve this order for parity; crossing buff expiry in a large tick is not
  analytically integrated in the source.

### Number formatting

Use exported suffixes and cases. Below 1000: one decimal only when value <10 and
fractional remainder >0.05, otherwise floor to integer. Above: floor(log1000(value))
selects suffix and two decimals; beyond suffix table use two-decimal mantissa and
integer base-10 exponent. Source supports nonnegative game currency, not arbitrary
signed financial numbers. Do not replace with float formatting.

## 4. Persistence and migration

Original file is Godot `user://genesis_save_v1.json` (not project-relative).
Fields: equipped_appearance, completed_chapters, quanta, lifetime_quanta,
current_run_quanta, genesis_seeds, goddess_stage, counts, relic_levels, timestamp.
Save template shows every known count/relic ID. No AI credentials belong in a save.

On load: filter chapters to known IDs; missing numeric fields default zero; clamp
stage to 0..5, counts to nonnegative, relics to 0..max_lvl or 1,000,000. Selected
appearance defaults to Ivory Queen and must be both known and unlocked. Old saves
gain design permissions from their remembered chapters without another unlock list.
Cooldowns, temporary buff, buy mode, current emotion, mute and camera mode are NOT
persisted. Do not claim that the original saves contain them.

Offline elapsed = clamp(nowUnixSeconds - timestamp,0,43200). Only if >10s, award
total_qps*elapsed*0.5. Use loaded state without temporary buff. Offline gain uses
the qps before the reward and can trigger evolution via normal add_quanta.
Apply once, then persist a new timestamp. Never accrue from timestamp zero fixtures.

Unity save location should use its platform persistence API, not Godot's literal
user:// path. A changed application identifier/storage origin does not magically
find older files. Use explicit import/export, back up originals, validate nonfinite
values, version the Unity format, and write atomically with a backup. Atomic writes
are a recommended improvement; the Godot source writes directly. Web persistence,
app pause, focus loss, offline clocks and safe-area behavior require device tests.

## 5. Character presentation, wardrobe, emotions

Source uses a transparent isolated SubViewport over a 2D canvas, orthographic camera,
4x MSAA and two directional lights. Unity analogue: separate character layer/camera
rendering to a transparent RenderTexture displayed in a UI image, with nonblocking
raycasts; select exact pipeline and alpha setup in the actual Unity project.

Source camera: full size 4.2 at (0,1.8,7); portrait 1.85 at (0,2.78,7). Godot camera
size is a full extent; Unity orthographicSize is a half-height, so do not blindly
assign the same number. Aspect/keep-aspect differences must be verified against
screenshots. Source avatar nominal viewport is 512x320 but container stretch affects
actual size. Godot geometry front is +Z and camera faces -Z; do not manually mirror
only the mesh without winding/normals/camera conversion. A glTF importer should
perform its coordinate conversion; do not convert the GLB twice.

Prototype has joint pivots, spheres/profiles, interpolated hair/ribbons, broad
curved wing panels, no skeletal skinning or animation clips. The GLB preserves
reference meshes/hierarchy, not the shader appearance or procedural runtime code.
Recreate silk shading from the shader source or replace it with authored materials.
Pose samples are visual/transform references; source `_process` defines timing.
Do not advertise the GLB as a production-quality rigged anime character.

Emotion order: Idle, Happy, Dance, Shy, Surprised, Sorrow, Courage, Anger, Compassion,
Serenity. Only Idle always available. The catalog maps nine emotions to claimable
Universal Emotions chapters. Menu selections play 6s, taps play Happy 2s only if
unlocked, normal chapter rewards play Dance 5s only if unlocked, newly claimed
emotion plays its own pose 6s. Return Idle after duration. Blinking and idle motion
remain active. Switching demo era clears newly locked expressions. Full pose math
is in FairyAvatar3D.gd; do not lose head/arm/elbow/knee/hair/gown/eye motions.

Wardrobe stable IDs/milestones are in catalog. Render all entries including locked
ones; reject locked/unknown IDs in game logic, not only UI. New unlocks never force
equip. Lower-left Designs menu is separate from lower-right Portrait button. The
empty void hides fairy and all three character controls. Appearance changes do not
grant gameplay bonuses, alter emotion allowlists, or reset camera/current motion.

Art requirement for the Unity successor: original adult anime fairy, natural human
anatomy, layered glossy hair, expressive face and ornament, ivory/violet/gold
reference direction, convincing smooth 3D or deliberately authored 2.5D. Avoid
another primitive-mesh polish loop. Author one consistent identity/shared rig with
replaceable outfits, wings and cosmetics; archive each shipped design, never replace
the catalog entry with later art. Current model remains visibly doll-like.

## 6. UI, input, audio and demo contract

Main.tscn is only a root Control with Main.gd: UI is built in code. Recreating the
scene file alone loses the entire UI. Main.gd is the authoritative hierarchy and
copy/theme source. Features: currency/qps HUD; fairy; goddess title/multiplier;
next-discovery progress; chapter claim/progress and chronicle; generator/relic tabs;
miracles; buy x1/x10/x100/MAX; Bloom confirmation; notifications; help/mute; compact
demo selector. Ensure overlays and character image do not eat button/tap events.

- Nominal desktop 1280x720. Breakpoint width <1000: stacked columns, 230px fairy,
  hidden long goddess lore/scale label, smaller card icons/fonts. Wide fairy 260px.
- Purchase and claim targets >=48px. Cards stay horizontal on phones, not towers.
- Whole page scrolls vertically. Dialog available size viewport minus (24,48).
- Tested sizes: 360x800,390x844,800x360,1280x720. Match screenshot framing, but improve
  density/safe-area support deliberately; current source is pixel-sized prototype.
- Core tap gives gain and eight sparks, max128; particles expire after0.65s;
  no double-firing touch and emulated mouse event. Distinguish scroll drag from tap.
- Quieter moon backdrop and at most three newest companions; suppress companions
  in portrait. Preserve initial empty void.
- Five WAV cues: tap/purchase/chapter/evolution/miracle. Initial level -12dB;
  tap rate limit65ms. Mute utility affects output. No ambient score/voice is included.

Demo mode must never load/save normal player data. All18 presets are exported.
Selecting a preset resets counts/relics/seeds/buff/cooldowns/chapter memory/equipped
design to that preview, then funds earlier generators (10 each, target3), computes
run/lifetime totals, optionally funds next purchase at1.25 base cost, remembers
corresponding chapters, selects form silently, and emits state update. See exact
code/snapshots. Demo reset is NOT the normal permanent-progression behavior.

## 7. Reuse, exclusions and acceptance checks

126 runtime assets:111 fairy icons,6 portraits,1 nebula SVG,3 particle PNGs,5 WAVs.
Generated originals use project procedural art, not downloaded franchise assets.
Read ASSET_NOTES for provenance/limits. Main queen is geometry, not the SVG portrait.
No fonts, authored 3D character, animation clips or personal reference JPG bundled.
AI service scripts are optional historical code, not required by core gameplay;
do not ship service keys in a client. Revalidate providers before any future use.

Before declaring parity:
1. Load all catalog IDs and imported art; verify package hashes and dimensions.
2. Match120 purchase/production cases,10 number-format examples,18 demo states.
3. Complete chain and107 claims; duplicate claim and locked purchase rejection.
4. Preserve legacy chapter IDs and inserted-layer discovery behavior.
5. Test miracles, harmony caps, milestone count25/50, relic caps and Bloom math.
6. Test real save roundtrip, old/malformed save defaults, offline cap/clock rollback,
   pause/focus/autosave and import exactly once (no duplicate offline rewards).
7. All ten emotions gated by claims; unlock persists through Bloom; camera/tap safe.
8. All six designs gated by claims, no auto-equip, switch backward, persist selection,
   retain collection through Bloom; unknown/locked restored selection falls back.
9. PlayMode test four viewport sizes, popups, tabs, dropdowns, scroll and input.
10. Actual Android/browser/desktop builds, performance, battery, safe area, audio,
    storage/reload and character clipping; these were not validated in Godot.

No Unity editor/pipeline/package version is chosen by this package. First achieve
behavior parity, then improve artwork and UX without silently changing economy.

## Official Unity references checked for migration planning

Consult the versions appropriate to your chosen editor:
- Unity Manual, JSON Serialization (serialization rules/dictionary limitations).
- Unity Scripting API, Application.persistentDataPath (platform save locations).
- Unity Scripting API, RenderTexture (offscreen rendering).
- Unity-Technologies/com.unity.cloud.gltfast repository README (glTF import).

These are planning references, not installed dependencies or verified build support.