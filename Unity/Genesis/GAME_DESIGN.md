# Zenda / goddess-centered design foundation

## Zenda-led storytelling / October 26, 2026

Chronicles now opens as Zenda's Stories: an animated character portrait above
player-paced dialogue, not a lore-only reading panel. Each of the 107 chapters
includes a first-person introduction, the complete original lore framed as a
memory, and Zenda's account of her participation. The first seven introductions
and participation passages are bespoke; later passages use named companions and
era-based or domain-based templates, not 107 individually authored scripts.
Original chapter IDs, fictional framing, progression goals and rewards are kept.

Pages carry expression cues, held while the story is visible. Zenda nods, gestures
and performs timed silent mouth animation; no voice audio or audio-driven lip sync
is included. Players can continue immediately, scroll text, revisit a prior page,
or leave and return without a forced listening timer. Finish Zenda's story marks
completion, then a separate gift button claims the existing one-time reward.

The same VRM is framed in a story portrait; leaving restores the Goddess camera
and closeup preference. Idle RPG and the story library do not render its camera.
Gathering continues during storytelling. Reader state is still session-only.

Validation: 541 content checks across all chapters and 22 live performance/reader
checks passed. Reports: E:\ZendaGame\Genesis\Builds\story-content-validation.txt
and E:\ZendaGame\Genesis\Builds\story-integration-validation.txt.

## Compact gathering redesign / October 26, 2026

This supersedes the earlier large Idle RPG cards and Dimension-first ordering.
Idle RPG starts with a manual 2-second Gather action awarding 5 Hope, then compact
116-unit discovery rows: artwork, level, effective rate, cycle bar, cumulative
gathered resources, next 25-level doubling milestone, and unlock/upgrade purchase.
Only revealed discoveries are listed, with the next locked name teased below.
Journeys and Dimension selection are collapsed below production. Chronicles remain
separate, and discovery purchases still unlock the same stories.

Owned discoveries auto-gather across all three tabs in 2–6-second cycles. Each
cycle deposits its integrated production into the shared resource balance; these
are not separate ingredient inventories. Pending work retains its original value
when upgraded; overflow time completes multiple cycles without a per-cycle loop.
Average production, prices and milestone bonuses are unchanged. The legacy core
Tick API still produces continuously by default; Sanctum disables that income and
advances GenesisGathering instead to prevent double credit. All cycle state is
session-only. No persistence, combat or offline progression is added.

## Current navigation / October 26, 2026

This section supersedes the earlier two-tab and claim-before-reading layouts.

1. Main Goddess: large animated Zenda, translucent icon controls, tap boost.
2. Idle RPG: incremental creation upgrades, passive income, Dimension browsing,
   and timed journeys. Inspired by the earning/upgrading/universe-progression loop
   of Annihilation - Space Tycoon, not its destruction premise, assets or branding.
   Combat, equipment and independently simulated universe laws are not implemented.
3. Chronicles: a separate library with progression-locked lore. Discoveries and
   chapter goals unlock entries without awarding them. Open the reader, advance
   through every page and choose Finish reading, then explicitly Receive reward.
   Reopening a story never grants its reward again. The game records reader
   completion, not actual human comprehension. No forced reading timer is used.

Chapter IDs, prices and reward amounts are unchanged. Sanctum claims can be made
for any individually unlocked and read entry; earlier unread entries do not block
later discovery stories. Legacy core ClaimChapter() retains sequential behavior
for existing callers; the ID overload checks progress and duplicate claims.
Reader/unlock state remains session-only, matching the existing prototype.

## Core correction: superposed Dimensions

Countless universes can occupy identical (x,y,z) coordinates without spatial
obstruction, retain distinct laws and time, and cooperate. This is the intended
superposition, not additive upgrade layers. The Dimension/universe taxonomy has
not yet been finalized. Zenda remains the main goddess of creation.

### Implemented structural pass

The catalog's 29 named dimension milestones plus a Primordial Possibility foundation
now partition all 107 chapters and 106 discoveries into 30 layers. This is a derived
catalog projection; original data, stable IDs, chapter order and save format remain
unchanged. A named milestone closes its layer; discoveries since the preceding
milestone belong to that layer. The opening Hope/Intention/Breath discoveries form
Primordial Possibility; Yin/Yang and their synthesis form Duality.

This boundary convention is a first design interpretation, not a claim that all
chapter assignments have received narrative approval. It can be replaced later
with explicitly authored membership without changing discovery/chapter IDs.

Idle RPG has a Dimension selector and expandable index. Each provisional group
shows formation, claimed stories and income. Discoveries and unlock goals appear
there; lore reading and rewards live in Chronicles. A group is formed when at
least one discovery is owned, not when its stories have all been claimed.

The current prototype only keeps all formed groups productive simultaneously;
this additive income is not an implementation of the intended superposition.
Their income contributions sum to the existing total production.
Browsing a layer changes presentation only; it does not reset, replace, pause or
multiply other layers. Journeys require the selected layer to be formed, and store
their source Dimension at departure even if the player browses another one.

### Required next systems (not yet implemented)

- Layer-specific laws, resources, inhabitants and RPG encounters tied to creation.
- Explicit inter-layer dependencies and resonance, rather than arbitrary bonuses.
- Visible overlapping cosmic geometry around Zenda, with layer visibility controls.
- Dimension completion rituals and narrative transitions.
- Rework the generic journey routes into dimension-authored exploration content.
- Persistence/offline progression and physical-device UI validation.

### Validation

18 mobile regression checks and 10 dimension checks passed through live Unity MCP:
complete unique membership, invalid selection rejection, unformed travel gating,
layer formation, additive income decomposition, view/economy independence, journey
identity and simultaneous production. A script-reload null initialization issue was
found, fixed with lazy rebuilding of the derived map, then tests were rerun.
Report: `E:\ZendaGame\Genesis\Builds\dimension-validation.txt`.
No visual superposition, phone build or standalone verification is claimed.

## Journey progression follow-up / October 26, 2026

This supersedes the cosmetic-only bond rank and single-route descriptions below.

- First Light lasts 30 seconds; Moon Garden lasts 60 seconds and unlocks at three
  claimed journeys; Astral Sea lasts 90 seconds and unlocks at six claims.
- Bond rank is 1 + floor(claimed journeys / 3). Each rank above one grants +10%
  journey rewards, additively. It does not multiply taps or passive production.
- A route's Hope reward is max(10, current passive income * duration / 2), times
  the current bond bonus, fixed at departure. Previewed rewards may change before
  departure as production/rank changes, but not while traveling or awaiting claim.
- Route selection is blocked while traveling or awaiting claim. Completion never
  automatically starts another trip or consumes overflow time.
- Chronicle shows route unlocks, reward preview, progress, and claim status.
  The Goddess tab has a completion shortcut to Chronicle.
- Drag tracking now requires a pointer-down inside the Chronicle viewport and
  remains active outside it until release. Physical device gesture testing remains
  pending; these are still IMGUI controls, not a finished mobile UI framework.

Validation: existing 18 mobile checks and 11 journey progression checks passed in
Unity Play mode via MCP. Journey checks cover invalid/locked route selection,
selection lock during travel/claim, reward stability, rank bonus, 60/90-second
routes, no early completion, and completion while on the Goddess tab.
Report: `E:\ZendaGame\Genesis\Builds\journey-validation.txt`.
Repeatable request: `E:\ZendaGame\Genesis\Tools\MCP\journey-smoke.json`
(run after the mobile smoke request in a fresh session).
An account-service timeout warning was present. No phone build, screenshot review,
combat system, or persistence is claimed by this update.

## Mobile demo update / October 26, 2026

The Sanctum now uses two bottom navigation tabs instead of the original desktop
split-screen. The older interface description below records the first iteration.

- **Goddess:** the main Zenda model rendered into a bounded 600x720 stage texture,
  three scene/aura lighting presets (Midnight, Rose dawn, Aurora), portrait/full-body
  framing, goddess responses, Inspiration, and invocation.
- **Chronicle:** the active chapter and claim action, idle starlight journeys,
  discovery purchases, and remembered chapter lore.
- The goddess camera stops rendering while Chronicle is selected. Core production
  and expedition timers continue regardless of the selected tab.
- Layout uses Screen.safeArea, a 390-point reference width, 44–52 point primary
  controls, and a vertically scrollable Chronicle with drag handling. Landscape
  is contained rather than expanded into the former desktop layout.
- Cosmetics currently mean stage lighting/background and framing. No model outfits,
  hair meshes, or textures are changed; real wardrobe swaps remain future work.

### First idle RPG mechanic

Awaken one discovery to unlock starlight exploration. Start a 30-second journey;
its reward is fixed at departure to max(10, 15 seconds of current passive income).
Completion requires an explicit claim and cannot be claimed twice. Every three
claimed journeys increases the displayed bond rank. Rank is cosmetic for now:
combat, equipment, skill trees and stat bonuses are not implemented.

The demo remains nonpersistent. Journeys advance during this running session only;
app suspension/offline catch-up and mobile deployment are not implemented here.
No Android/iOS build, phone touch test, screen-reader navigation, performance
profiling, or visual screenshot verification has been completed. IMGUI remains a
prototype layer; production UI should migrate to the project's supported UI stack.
No global orientation or build-target settings were changed.

Live MCP mobile smoke: 18 checks passed across four synthetic display sizes,
safe-area conversion, tab state, first chapter/purchase/history, idle income,
expedition gating/completion/single claim, and cosmetic/progression separation.
Report: `E:\ZendaGame\Genesis\Builds\mobile-validation.txt`.
Request: `E:\ZendaGame\Genesis\Tools\MCP\mobile-smoke.json` (fresh Play session).

## Direction

Zenda, the supplied VRoid/VRM character, is the main goddess/fairy. She is not an
optional portrait or a secondary preview character. The procedural Astra character
in the earlier demo is a legacy stand-in, not the intended protagonist.

The player acts with Zenda to bring a universe into existence. The intended tone
is gentle cosmic creation: readable goals, visible growth, and responsive character
feedback rather than combat or punishment for inactivity.

## Implemented first playable: Sanctum

Scene: `E:\ZendaGame\Genesis\Assets\Genesis\Scenes\ZendaSanctum.unity`.
Open through **Genesis > Open goddess Sanctum**, then press Play.

- Zenda is the actual imported skinned model, lit and framed as the focal point.
- Invoke Possibility grants the existing core tap reward.
- Tapping the goddess or invocation icon adds two seconds of Inspiration, capped
  at sixty seconds. The activating tap and subsequent taps grant twice the core
  tap reward. Repeated taps replenish duration, not multiplier strength.
- The timer continues on Chronicle. A compact timer and duration meter show the
  remaining boost. Goddess input uses projected model bounds; overlay controls
  receive input first and the help overlay disables goddess tapping.
- The goddess uses procedural breathing, hovering, head/chest sway, a relaxed arm
  pose, blinking, and a short tap-triggered nod/hand gesture. This is not an
  imported motion-capture animation set; model assets and metadata are unchanged.
- Inspiration does not multiply passive income. This keeps idle progression useful
  without making continuous clicking necessary for baseline production.
- Discoveries cost the existing catalog prices and generate passive income.
- Memory chapters show a concrete goal, progress, and reward; claims are explicit.
- Zenda reacts happily to invocation/purchase, relaxed to a memory claim, then
  returns to neutral after 2.5 seconds.
- No player saves, offline reward calculation, monetization, or model metadata edits.

## Interface hierarchy

1. Top bar: identity, current resource, income rate, discovery count.
2. Left stage: large 3D goddess rather than a tiny decorative portrait.
3. Bottom-left: goddess response, Inspiration gauge, primary invocation action.
4. Right-top: one next-memory objective and claim action.
5. Right-bottom: revealed discovery cards, cost, owned count, base production.

The first pass uses the project's existing IMGUI approach with a 1280x720
letterboxed design space. It is not a final Canvas/mobile-safe-area implementation.
The stage framing and small-screen readability still need visual iteration.

## Next design work (not implemented)

- Stage transitions that visibly create matter, stars, ecosystems, and dimensions.
- Authored idle/casting/celebration clips, relaxed arms, gaze, and reaction blending.
- A clear era map, with milestone previews but no spoiler-heavy locked list.
- Miracles as deliberate cooldown abilities; show effect, duration, and readiness.
- Big Bloom as an explicit-confirmation prestige ritual with a reward preview.
- Wardrobe as real compatible skinned outfits, not misleading palette-only controls.
- Sound, VFX, keyboard/controller navigation, accessibility, touch, and safe areas.
- Measure the opening minute before changing core prices or production curves.

Existing economy, miracles, relics and prestige remain available in the legacy
demo/core. The new Sanctum deliberately exposes only the opening loop so its
character, goals, and primary action can be evaluated before expanding navigation.

## Asset and scene safety

This is local development using the user-supplied model as requested. Its embedded
permissions still require author clarification before distribution. The model and
metadata are unchanged, no standalone build was created, and existing build-scene
settings were not replaced. The original preview and legacy demo remain intact.
Opening the Sanctum is additive so unsaved scenes are not silently discarded.

## Verified October 26, 2026

Unity compiled the new assemblies. Live MCP Play-mode smoke passed 12 assertions:
fresh state, Inspiration threshold, doubled reward, expiry, first chapter claim,
purchase, discovery chapter claim, passive production, goddess initialization,
happy reaction, neutral reset, and invalid-time rejection.

Report: `E:\ZendaGame\Genesis\Builds\sanctum-validation.txt`.
Repeatable request: `E:\ZendaGame\Genesis\Tools\MCP\sanctum-smoke.json`
(requires a fresh Sanctum Play session).

Console query returned an account-service timeout warning. Visual screenshot capture
was requested but no image file was available at verification time; visual rendering
quality and standalone behavior are not claimed to have been validated.
## Resident Lady friendships / October 26, 2026

Every one of the 106 existing creations has an explicitly named resident Lady and
home in the catalog's `resident_friends` field. Zenda visits, helps construct her
home, and becomes her friend; each Lady remains a resident with her own agency.
Fire's companion is Lady Ignis. The Soul discovery also introduces Lady Death and
the Death Dimension as a connected home, not a new priced generator or milestone.
Gathering rows display Meet/Friend names; chapter retellings add friendship and
return-visit pages while preserving original lore, IDs, costs, goals and rewards.
Ignis and Death have specific scenes; other friendships use a shared structure.
These are narrative friendships, not a new friendship-level/reward system. No new
NPC models, voice recordings, or persistence are included.

## Illustrated story reader / October 26, 2026

Selected style: detailed fantasy visual novel. Reader supports per-chapter scene PNGs
and per-page overrides, with editable narration over the lower image area. Missing
art retains the animated portrait. See `E:\ZendaGame\Genesis\STORY_ART_DIRECTION.md`
for eight encounter briefs, Lady names/titles and delivery conventions. Death is now
Lady Morthelia, Keeper of the Last Garden. No new artwork has been generated.
Validation: 854 resident checks, 541 story checks, 10 live reader integration checks
using a temporary test texture. Actual illustrated composition and phone readability
remain unvalidated. Play stopped; dirty preview scene preserved.

## Lady gallery / October 26, 2026

Open Zenda's Stories > Lady gallery. Two-column, touch-scrollable collection uses
the existing chibi discovery icons, not Zenda/VRM/Astra main-character art. Cards
show names, homes, titles and Our story links. Only purchased creations or their
completed discovery chapters count as met; visible-but-unpurchased rows do not.
Includes both Soul residents; Morthelia reuses shadow_realm chibi art. No new art
or rewards. Fresh game shows an empty-state hint. Domain: 438 checks passed;
live integration: 13 checks passed. Repeat Tools/MCP/lady-gallery-smoke.json and
lady-gallery-integration.json (integration requires a fresh Play session).

## Lady pets and histories / October 26, 2026

Gallery Select / History opens a scrollable first-person history with chibi portrait.
Accompany Zenda / Use as Pet equips one cosmetic chibi companion; selecting another
card only browses history, while equipping replaces the current pet. Send companion
home removes her. Visit Zenda shows the floating 2D chibi beside the Goddess; tapping
the companion opens her history without invoking Hope. Full story still requires
explicit Chronicle reading and completion. Morthelia has her own Death history, not
Animelle's Soul lore. No bonuses, rewards or new models. Selection is session-only.
Validation: 336 companion checks across 107 Ladies; 438 gallery domain checks.
Repeat E:\ZendaGame\Genesis\Tools\MCP\lady-companion-smoke.json in a fresh Play session.
