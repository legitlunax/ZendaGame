# Warm-hearted Sanctum theme

October 26, 2026.

## Design

### Dimension theme progression / October 26, 2026

- The three static styles are superseded by **30 dimension themes**: Primordial
  plus the catalog's 29 named worlds. Primordial starts available. Named themes
  unlock using their exact `chapter_id` and existing `IsChronicleUnlocked` rules:
  meeting the dimension's story milestone or having completed it. Unlock does not
  require claiming a reward, and purchasing an earlier creation within a layer
  does not substitute for the dimension milestone (Yin is not Duality).
- Themes are small 192 x 320 generated-once, cached UI textures, released with the
  view. They use world palettes and border motifs, not scene/character artwork.
  Wood/Fae use leaves, Fire/Water use flowing edges, Earth/Metal use geometry,
  Duality uses opposing bands, other worlds use varied celestial/ribbon borders.
- A 238-unit-tall modal replaces the former 354-unit style list: one preview,
  short world name, left/right selector, availability, Cancel and Confirm. Locked
  worlds may be previewed but cannot be applied, including through SetSceneStyle.
  Selection stays pending until Confirm; reopening resets it to the applied world.
  App background remains consistent across tabs. No reward/economy changes.
  Unity validation: 363 checks passed at 2026-10-26T20:35:43Z using a temporary,
  isolated Sanctum component; covers all themes, fresh locks, exact story unlocks,
  setter/confirm lock enforcement, pending/apply, tabs and compact modal bounds.
  This is not a physical-phone performance or visual-acceptance result.

### App-background correction / October 26, 2026

- App backgrounds are now decorative UI surfaces, not garden/location scenery:
  **Rose silk** (rose ribbons), **Celestial blue** (cool orbital/constellation
  borders), and **Sage botanical** (green paper/leaf frame). Existing image paths
  and GUIDs remain stable; the old filenames are compatibility names only.
- Generator: `E:\ZendaGame\Genesis\Tools\Migration\Generate-AppBackgrounds.ps1`.
  The previous generator entry point delegates to it.
- Style selection now applies across Goddess, Idle RPG, storyline and gallery;
  tab navigation no longer silently swaps the selected app wallpaper.
- Draw the background once across the full window, with no second center-canvas
  crop/seam. These abstract surfaces fill the viewport without cropping away edge
  decoration; their shape can stretch with the window. Picker thumbnails show
  the same complete composition. Cards retain opaque, readable surfaces.
- Background selection no longer changes the Goddess's light colors. Character
  framing, story art, economy and control geometry are unchanged.
- Source PNGs were generated and visually inspected. Unity MCP refresh timed out
  waiting for editor readiness; live visual acceptance is not claimed here.

### Previous garden implementation (superseded by the correction above)

- Rosewater, ivory, peach, sage and antique-gold palette; warm plum text.
- Three original procedural floral garden PNGs (780 x 1400), generated with
  E:\ZendaGame\Genesis\Tools\Migration\Generate-WarmBackgrounds.ps1 using
  Windows System.Drawing. These are local illustrations, not AI paintings.
  The available external tool search did not expose a usable image generator.
- Rose garden for Goddess, sage garden for RPG/gallery, ivory garden for stories.
- Rounded pearl buttons with hover, pressed/focus and unavailable treatments;
  ivory keepsake cards and light header/navigation surfaces.
- Serif headings where a supported installed font exists; default-font fallback
  on other platforms. No system font files are copied into the project.
- Body text remains 15 logical pixels; secondary text increases from 12 to 13.
- Existing button rectangles, input handling and gameplay logic are preserved.

## Verification

- Warm-theme smoke: 32 checks passed, 2026-10-26T18:20:58.3815223Z.
  Validates imported images, interaction textures, configured button contrast
  (at least 4.5:1), text size and existing Goddess touch targets.
- Icon layout: 31 checks passed; illustrated-reader integration: 10 passed.
- Storyline regression: 229 checks passed in fresh Play,
  2026-10-26T18:21:27.2267323Z. Initial attempt rejected a non-fresh session;
  the reported pass is from the clean restart.
- Final regression Console query returned zero warnings/errors.
- Reviewed E:\ZendaGame\Genesis\Builds\warm-storyline.png and
  E:\ZendaGame\Genesis\Builds\warm-goddess.png. Backgrounds and buttons render;
  Goddess camera transparency lets the illustrated garden show behind Zenda.
- Physical-phone readability/performance, platform serif availability, and
  complete gallery/RPG visual acceptance remain unverified. Goddess capture
  retains bright avatar lighting and existing tight companion-label spacing.
  The previously reported blank story portrait remains a separate open issue.
- Preserve the dirty preview scene without saving it.

## Avatar clearance refinement

- Fairy icons use opaque dusty-rose medallions with subtle borders and a darker
  selected outline, keeping pale artwork readable against the gardens.
- Header (title and Hope rate), avatar stage, and lower invocation controls now
  occupy separate vertical bands. Full-body framing fits the reserved stage;
  close-up framing intentionally crops the lower body, not the head.
- Invocation keeps its 76-pixel touch target; inspiration text sits beside it.
- Updated icon smoke passed 35 checks across logical heights 620, 700, 844 and
  1000, including header/action separation and control bounds/non-overlap.
- Reviewed full-body and close-up captures at
  E:\ZendaGame\Genesis\Builds\warm-goddess.png and
  E:\ZendaGame\Genesis\Builds\zenda-clear-header-closeup.png.
- Console query returned zero entries. Physical-device acceptance remains open.

## Goddess controls and style picker

- Removed the duplicate floating Dimensions shortcut; Idle RPG remains in the
  navbar. Five floating controls remain, with unchanged touch-target sizes.
- Star cluster now opens a modal background picker with previews and a selected
  marker: Rosewater garden, Ivory story garden, and Sage friendship garden.
  Selection changes the actual garden artwork across the Goddess canvas, not
  merely the transparent camera clear color. Selection is session-only.
- Heart opens five main-Goddess emotions: happy wave, relaxed breath, sad bow,
  angry pout, and surprised gasp. Each has a three-second procedural head/body
  gesture and matching VRM expression. Explicit reactions take precedence over
  social affection during playback, then return to the normal social mood.
- Popups block underlying invocation, navbar, and companion-pointer handling;
  close via Close, an outside tap, or programmatic tab navigation.
- October 26, 2026 validation: Goddess controls 54 checks passed at
  19:28:00 UTC; icon layout 31; warm theme 28; companion emotions 1938.
  Lower layout/theme counts reflect removal of one control at four heights.
- Reviewed E:\ZendaGame\Genesis\Builds\background-picker.png and
  E:\ZendaGame\Genesis\Builds\reaction-picker.png. Console query returned
  zero entries. Physical-phone input and animation aesthetics remain unverified.

## Press-and-drag avatar rotation

- Press Zenda and drag horizontally to turn her about her vertical axis in
  full-body or close-up mode. The angle remains on release and wraps at 360
  degrees; sensitivity is 0.6 degrees per logical canvas pixel.
- Pointer capture keeps a drag active outside the initial avatar bounds.
  Movement over 8 logical pixels suppresses invocation (including vertical
  drags); a stationary tap or small jitter still invokes normally.
- Floating controls, companion hit area, help and modal popups take priority.
  Tab changes, focus loss, app pause and component disable cancel the gesture.
- Story portraits use the original forward-facing rotation; returning to the
  Goddess restores the selected angle. Rotation is session-only and cosmetic.
- October 26, 2026: final rotation smoke passed 28 checks at 19:36:32 UTC,
  including dragging back to the starting point; icon layout 31, Goddess
  controls 54 and companion emotions 1938 also passed before that refinement.
  Final Console query reported one Unity AI account-connectivity warning, no
  gameplay errors. Pointer events were simulated in the editor; physical
  phone gesture acceptance remains unverified.
- Screenshot: E:\ZendaGame\Genesis\Builds\goddess-rotation.png.

## Mellow hexagonal fairy buttons

- Replaced circular fairy-button surfaces with six-sided, softly cornered
  hexagons: opaque lavender-to-peach pastel fill, pearl inset bevel, muted
  mauve border and darker plum selected outline. Applies to floating controls
  and navbar icons; artwork insets, button rectangles and input are unchanged.
- October 26, 2026 at 19:39:32 UTC: hexagon texture validation passed 9227
  checks (mostly per-pixel symmetry), covering the silhouette, transparency,
  pastel center, selected border and antialiased edges. Icon layout passed 31
  checks; warm theme passed 28. These are not physical-device acceptance tests.
- Reviewed E:\ZendaGame\Genesis\Builds\warm-goddess.png. Console contained
  only the Unity AI account-connectivity warning, no gameplay errors.

## Visual-first Idle RPG

- Replaced repeated gathering instructions with 112-unit fairy cards: larger
  artwork, resident fairy names, level/rate, timed sage production bars, rose
  milestone tracks and compact purchase controls. Creation names and exact
  milestone/gathered totals remain available as desktop hover metadata.
- Gather, Journey and Worlds have separate views with selected underlines.
  Route artwork, lock shapes, countdown bars, ready indicator and world chapter
  tracks replace repeated status sentences. A slim rose scroll indicator replaces
  the default black scrollbar; touch drag and wheel scrolling remain available.
- Short action labels and costs remain explicit. The optional question-mark
  guide explains symbols and progression on touch devices; it is hidden by default.
- Gameplay/economy rules are unchanged. Starter progress has its own track and
  does not replace the discovery progress track.
- October 26, 2026: gathering domain 121 checks, integration 11 checks, visual
  Idle RPG 235 checks (including all 106 fairy names/artwork) passed. Journey
  progression passed 13 checks in a fresh Play session; an initial sequential
  run failed its state prerequisite after the other suite had already run a
  journey. No gameplay change was needed. Phone touch acceptance remains open.
- Evidence: E:\ZendaGame\Genesis\Builds\idle-rpg-visual-validation.txt,
  E:\ZendaGame\Genesis\Builds\idle-rpg-visual.png,
  E:\ZendaGame\Genesis\Builds\idle-rpg-journey.png and
  E:\ZendaGame\Genesis\Builds\idle-rpg-worlds.png.

## Open-eyed face and full-body reactions

- Resting relaxed expression is reduced from 0.7 to 0.08; happy is 0.28 so
  the model's combined eyelid/smile morph does not hold a closed-eye smile.
  Blinks last 0.16 seconds every 4.7 seconds, with no blink at session start.
- Five reactions now last six seconds, with smooth entry/exit. Happy uses
  alternating leg lifts/knee bends, heel motion, hip and shoulder counter-sway,
  arm sweeps and wrists. Relaxed adds a slow swaying dance; sad a whole-body
  bow/breath; angry alternating playful steps; surprised a bent-knee recoil.
- Ten additional humanoid bones are captured/reset every sample. Animation
  does not modify root yaw or imported assets. Storytelling excludes reactions.
- These are procedural in-place animations, not mocap clips or foot-IK dances.
  Foot planting, clothing intersections and phone performance need acceptance.
- October 26, 2026: full-body 129 checks, Goddess controls 54, rotation 28,
  companion emotions 1938 passed. Preview-only ValidateIdle could not run:
  the separate dirty preview scene's idle controller is not initialized in this
  mixed-scene setup. Its expected face/blink values were updated but are not
  reported as a pass. Initial body-test sample times had symmetric slow-sway
  poses; corrected to non-symmetric timestamps before the successful run.
- Reviewed open-eye portrait and E:\ZendaGame\Genesis\Builds\goddess-dance-held.png.
  Immediate same-call render strips can contain stale skinning poses; use the
  held pose captured after subsequent frames for visual evidence. Console query
  contained only the Unity AI connectivity warning. No preview scene was saved.