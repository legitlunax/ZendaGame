# Genesis / Playtest 01

## Zenda's Stories / October 26, 2026

- Unlock a Chronicle through gathering/discovery, then choose Zenda, tell me this
  story. Her animated portrait appears above her dialogue.
- Compare the first story's sorrowful beginning with Elpis's happy awakening.
  Expressions persist after the silent speaking motion stops; there is no audio.
- Continue/Previous navigate dialogue at your pace. Scroll longer text as needed.
  Finishing the final page enables a separate one-time gift; rereading pays nothing.
- Leave for Idle RPG and return: page position survives, production continues.
  Return to Goddess: its camera framing and closeup setting should be restored.
- Original lore is preserved. The first seven stories have bespoke new dialogue;
  remaining stories currently use companion/era-based first-person treatments.
- Repeatable tests: E:\ZendaGame\Genesis\Tools\MCP\story-content-smoke.json and
  E:\ZendaGame\Genesis\Tools\MCP\story-performance-smoke.json (fresh Play session).

## Compact Idle RPG gathering / October 26, 2026

1. Open Idle RPG. Use Gather +5; its two-second bar pays once, not on click.
2. Unlock the first discovery. Watch the automatic bar fill, then the balance rise.
3. Upgrade during a cycle: earlier pending work must not be repriced retroactively.
4. Unlock each newly revealed row; inspect levels, rates and 25-level milestones.
5. Switch to Goddess or Chronicles and back: gathering keeps advancing.
6. Expand Journeys to select/depart/claim. Expand Dimensions to choose its origin.
7. Drag the production list; releasing a drag must not purchase an upgrade.

Cycle rewards share Hope/Creation Potential, not separate material inventories.
Session resets lose all progress. Physical-device gesture validation is pending.
Repeatable domain checks: E:\ZendaGame\Genesis\Tools\MCP\gathering-smoke.json.

## Discovery navigation / October 26, 2026

- Discover now has case-insensitive search by revealed name, fairy, or era.
  Locked discovery names are not exposed by search.
- Toggle **All possibilities / Available only** to hide locked entries.
- **Next discovery** clears search and scrolls to the next available unowned entry.
- Story displays **Reward!** when the current chapter can be claimed.
- Economy rules, purchase modes, and player-save behavior are unchanged.
- Unity compiled the changes and live MCP validation passed 12 discovery/gameplay
  checks, including ten taps, the first purchase, two chapter claims, and filter
  updates after a new discovery. Report:
  `E:\ZendaGame\Genesis\Builds\discovery-validation.txt`.
- Validation used a temporary Play-mode scene; the open ZendaPreview's unsaved
  changes were not saved or replaced. No new standalone build was made.
- The separate VRM idle validation was attempted but did not pass initialization;
  its earlier runtime validation remains unresolved. No visual-polish claim is made.

## Current main-character design: Astra

The main fairy is now an original adult (25+) celestial magical heroine, using
`E:\ZendaGame\Genesis\Sailor Moon Characters personal creation.jpg` only as an
art-direction reference. Golden long hair and tall boots are the defaults.
The former long gown is replaced by an opaque fitted bodice, layered short skirt,
long gloves, star brooch, comet crown, and star earrings. Wardrobe colors, separate
wings, cosmetic switches, and six reactions remain available. No image pixels,
franchise character models, or external paid assets are included in the player.
This is a flat-shaded procedural design pass, not illustration-quality anime art
or a newly authored production skinned model.

Implementation: `E:\ZendaGame\Genesis\Assets\Genesis\Scripts\Runtime\GenesisFairy.Design.cs`.
Rendered preview: `E:\ZendaGame\Genesis\Builds\fairy-portrait.png`.
Live design smoke: 285 checks passed; existing 865 core checks and gameplay smoke
also passed. Existing standalone URP post-processing warnings are not resolved.

## Play

Run `E:\ZendaGame\Genesis\Play Demo.cmd` after the Windows build completes, or open
`E:\ZendaGame\Genesis\Assets\Genesis\Scenes\GenesisDemo.unity` in Unity and press Play.

This is a temporary, nonpersistent playtest. Closing the demo or choosing Restart
resets it. It never opens your Godot or Unity player saves. No API key required.

## Try this first

1. Tap the void ten times to gather Hope.
2. Open Story and remember the first chapter to collect its reward.
3. Open Discover and buy Elpis, then claim her story chapter.
4. Buy more generators; passive income and the creation preview grow.
5. Use the 1/10/100/MAX purchase modes. Explore miracles and Big Bloom later.
6. Use Era > / < Era to jump through all 18 exported preview states. Each jump
   replaces the demo state and clears temporary effects; it is not a normal save.
7. Use Speed 1x/10x/100x to accelerate production and cooldown time for testing.

## What is intentionally early

- The UI is an immediate-mode prototype, not the final Canvas/UI Toolkit design.
- The original fairy is visible and joint-animated. Wardrobe IDs visibly change
  the outfit/accent/wing palette and wing scale; these are not different skinned dresses.
- The creation viewport uses simple procedural geometry, not final VFX.
- Sound cues and original SVG art have been reused; no paid or third-party store
  assets were purchased, and no downloaded character license is assumed.
- A production skinned anime model, Humanoid retargeting, cinematic lighting,
  final shaders, and mobile touch/safe-area polish remain future work.

## Asset provenance

123 files prepared from the original game's procedural artwork/audio. SVG art was
rasterized to native PNG using the installed Godot command-line SVG decoder as an
offline conversion tool; the Unity demo does not depend on Godot at runtime.
The output/provenance hashes are in `E:\ZendaGame\Genesis\Migration\demo-assets.json`.
No remote asset service or in-game AI is used. Five original WAV cues are included.

## Rebuild

Unity menu: **Genesis > Build Windows playtest**. Only the demo scene is passed to
the build API; existing project build-scene settings are not replaced.

Unity menu: **Genesis > Connect local MCP** connects this editor to the local server.
The project editor utility also attempts that connection after script reload.
The server must be running on `http://127.0.0.1:8080`.

An unsaved scene encountered during setup was preserved under
`E:\ZendaGame\Genesis\Assets\Genesis\PreservedScenes` before creating the demo scene.

## Useful feedback

Tell me whether the tap/discovery loop is clear, how quickly the first purchases
feel rewarding, whether story rewards are easy to find, and which visual direction
you want next. Include your window resolution if text/buttons are clipped.

## Verified build status

- Windows development build: Succeeded, zero build errors.
- Standalone launched and logged GENESIS_PLAYTEST_READY; no startup exception
  remained after fixing shader stripping with an explicit Resources material.
- Live Unity MCP smoke test: ten taps, two successful chapter claims, first
  generator bought, creation level 1 and 0.5 income/sec.
- 847 core parity checks passed; standalone C# compile: zero warnings/errors.
- All 118 converted PNGs decoded successfully; all 123 prepared asset hashes match.
- Visual screenshot reviewed; offscreen-camera GUI clearing issue fixed.

Known rendering warning: this project's existing URP configuration reports stripped
depth-of-field/motion-blur/panini/bloom shaders in the standalone player. Those
post-processing passes do not run. The prototype does not depend on them, but
cinematic/post-processing graphics are not validated. Unity also emitted an
account-service timeout warning during an earlier editor run. A future graphics
pass should resolve pipeline configuration rather than suppress these warnings.

## Free fairy prototype

- Open Wardrobe. Earn/select the existing six outfits using the original unlock
  rules. Use Era previews to reach later unlocks faster.
- Scroll below the outfits for hair (violet/golden/short ice), shoes
  (bare/slippers/boots), independent crown/jewelry toggles, and wings
  (butterfly/petite/hidden). These demo-only overrides are nonpersistent.
- Preview Idle, Happy, Surprise, Sorrow, Casting and Celebration. Hover, blinking,
  and wing flapping run continuously; taps/purchases, chapters/Bloom and miracles
  trigger happy/celebration/casting respectively.
- Uses only the original project fairy geometry, with generated shoe primitives.
  No VRoid/Mixamo/store character or animation has been downloaded or imported.
- Conversion tool: `E:\ZendaGame\Genesis\Tools\Migration\Prepare-Fairy.py`.
  Provenance hashes: `E:\ZendaGame\Genesis\Migration\fairy-assets.json`.
- This is not a Humanoid rig and cannot directly play Mixamo retargeted clips.
  Different skinned outfit meshes, clipping checks across those outfits, and
  VRoid/UniVRM material compatibility are still unimplemented/unvalidated.

## Zenda goddess tapping and motion / October 26, 2026

### Three-tab follow-up

- Bottom icons, left to right: Main Goddess, Idle RPG, Chronicles.
- Idle RPG contains creation upgrades, Dimension browsing and journeys; it no
  longer grants story rewards directly. Goal completion unlocks a Chronicle.
- Chronicles lists locked, unlocked and rewarded entries. Open an unlocked entry,
  advance all pages, Finish reading, then Receive reward. Reread without rewards.
- Reader completion is tracked, not comprehension. Current state is session-only.
- New request: `E:\ZendaGame\Genesis\Tools\MCP\chronicle-smoke.json` (fresh Play).
  First run passed 18 checks; mobile 18, journey 11, boost 17 and icon 31 also passed.
- Earlier script-reload null references were traced to the session model being
  cleared without Awake rerunning; Update now waits for a fresh initialized session.
- No physical-phone or final three-tab visual validation is claimed.

- Tap the rendered goddess or invocation spark: each tap adds 2 seconds of
  doubled invocation rewards, capped at 60 seconds. The activating tap is boosted.
- Repeated taps replenish time, not strength. The timer continues on Chronicle;
  passive income is unchanged. The timer and small meter sit above the spark.
- Verify scene, framing, heart, navigation, and help controls do not also invoke.
  Goddess tapping is disabled while the icon guide is open. Projected renderer
  bounds are used as the hit area, not pixel-perfect mesh/silhouette picking.
- Procedural idle now includes lowered arms, arm sway, chest/head movement,
  hovering and blinking. Taps trigger a short nod and arm gesture with a smile.
- Live checks passed: 17 boost/motion, 12 updated Sanctum gameplay, 18 mobile,
  11 journey, and 31 icon-layout checks. Final error/warning query was empty.
- Reviewed the stage-only render at
  `E:\ZendaGame\Genesis\Builds\zenda-boost-stage.png`; relaxed arms confirmed.
  Whole-screen capture included other open-scene rendering, so it is not evidence
  of final mobile composition. Physical-phone touch and motion polish remain untested.
- Repeat requests: `E:\ZendaGame\Genesis\Tools\MCP\boost-smoke.json` and
  `E:\ZendaGame\Genesis\Tools\MCP\boost-capture.json`. Use a fresh Play session
  for the gameplay smoke, and another fresh session for mobile then journey smoke.

### Fairy validation / October 25, 2026 (local)

- Unity compiled the new runtime scripts and rebuilt Windows: Succeeded, errors=0.
  The synchronous MCP build call timed out, but the build-result file and updated
  player runtime assembly confirmed build completion; it was not retried.
- 240 live fairy smoke checks passed; 225 mesh renderers had supported materials.
- Original gameplay MCP smoke passed: ten taps, first purchase, two chapter claims.
- Existing core parity executable rerun: 865 checks passed. A fresh external
  `dotnet build` was unavailable because this shell has no .NET SDK; Unity itself
  compiled and built the changed runtime code successfully.
- Standalone startup logged both GENESIS_FAIRY_READY and GENESIS_PLAYTEST_READY;
  no startup exceptions found. Existing stripped post-processing warnings remain.
- Source/output character hashes verified. Rendered Unity portrait visually
  reviewed at `E:\ZendaGame\Genesis\Builds\fairy-portrait.png`.
## Resident Lady friendship validation / October 26, 2026

Repeat `E:\ZendaGame\Genesis\Tools\MCP\resident-friends-smoke.json` through
Unity MCP execute_code. Initial result: 854 checks passed across 106 creations.
Story content regression: 541 checks; gathering domain regression: 121 checks.
Manual checks: gathering rows show Meet before purchase and Friend after purchase;
long names fit without overlapping the next row; Fire introduces Lady Ignis; Soul
introduces Lady Animelle plus Lady Death in the Death Dimension. Advance to the
last friendship page before claiming a story reward. Rereading must not pay twice.
Live visual/mobile layout validation for these new rows is still outstanding.

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

Gallery screenshot reviewed at E:\ZendaGame\Genesis\Builds\zenda-lady-gallery.png: chibi grid, labels and story buttons visible. Physical-phone testing remains outstanding. Broader mobile-smoke rerun failed at 'Locked expedition started'; not counted as a regression pass. Play stopped; unsaved preview scene preserved.

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

Companion and history captures visually reviewed: E:\ZendaGame\Genesis\Builds\zenda-lady-pet.png and E:\ZendaGame\Genesis\Builds\zenda-lady-history.png. Chibi companion is visible beside Zenda; history wraps and scrolls above the action buttons. Story regression: 541 checks passed. Final Console query returned no warnings/errors. Play stopped and dirty preview preserved. Physical-phone touch testing remains outstanding.
