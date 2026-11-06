# Mellow buttons and differentiated fairies

## Changes

- Button synthesis: 44.1 kHz mono, 0.38 seconds, 330 Hz fundamental with quiet
  495/660 Hz overtones, smooth 25 ms attack and 120 ms release, source gain 0.18.
  Existing mute, focus, browser gesture and button routing remain in place.
- 255 original procedural vector portraits rasterized at 1024 and reduced to
  512 pixels. Nine hair silhouettes, twelve outfit silhouettes, ten wing types,
  seven headwear/prop types, six skin tones, varied palettes and head tilts.
  These are procedural illustrations, not commissioned or AI-painted art.
- The v8 renderer uses a childlike storybook-anime art direction, referenced
  against the background fairies: round cheeks, a larger head-to-body ratio,
  shorter limbs and a relaxed, non-adult silhouette. The face is scaled by
  1.38x/1.25y around its center; clothing, arms and legs are scaled by 1.1x/0.74y
  around the collar. Hair keeps its original silhouette, widened for the head
  and shortened with the body; handheld props follow the shortened arms.
  Preserved details include
  larger luminous eyes with layered catchlights, subtle blush, modest high
  necklines and less-fitted layered clothing. Character names and lore are not
  rewritten to assign ages. Medium abstract wings use prism, crescent, ribbon,
  orbit, shard, petal, ripple, constellation, filigree and comet motifs, scaled
  to 85% width and 78% height around each wing root, with stronger opacity, framing the
  silhouette. Established hair, skin and outfit palettes remain deterministic.
- Wardrobe: bow, pinafore, flower and party dresses; hoodies with jeans,
  cardigans with skirts, T-shirts with overalls, sweaters with trousers;
  tracksuits and sport tunics with fitted opaque leggings; fantasy petal dresses
  and mage coats. Distinct hems, sleeves, pockets, ribbons, pleats and sporty
  stripes replace the previous all-gown silhouettes. Outfits are fully covered,
  with opaque tights under dresses and no exposed midriffs. Wardrobe selection
  uses a separate stable ID hash, preserving existing face, hair and wing traits.
- 149 optional residents receive individually curated names and occupations.
  Established narrative character names, all generator IDs, economy parameters,
  chapter data, and unlock anchors remain unchanged.
- Old resident names resolve through catalog aliases, including companion saves
  and gallery selection/equip calls. Save schema version stays at 2.

## Files and repeatability

Runtime audio: `E:\ZendaGame\Genesis\Assets\Genesis\Sanctum\GenesisSanctum.Audio.cs`.
Roster: `E:\ZendaGame\Genesis\Tools\Migration\FairyRoster.py`.
Renderer/migration: `E:\ZendaGame\Genesis\Tools\Migration\Refine-Fairies.py`.
Active artwork renderer: `E:\ZendaGame\Genesis\Tools\Migration\FantasyLadies.py`.
Wardrobe geometry: `E:\ZendaGame\Genesis\Tools\Migration\FairyClothes.py`.
Backup catalog and original PNGs: `E:\ZendaGame\Genesis\Migration\RefinedFairies\Before`.
SVG sources/trait and hash manifest: `E:\ZendaGame\Genesis\Migration\RefinedFairies`.
Existing PNG meta/GUID files are preserved. Runtime texture imports were checked at
512 pixels. Legacy roster/art generators now refuse to overwrite refined output.

Run `E:\ZendaGame\Genesis\Tools\Validation\Test-FairyRefinement.py` with the
existing MCP Python environment. Eleven tests cover immutable gameplay/story fields,
unique names and aliases, portrait dimensions/hashes/trait coverage, deterministic
rendering, wardrobe coverage, childlike anime direction, medium abstract wings, halo mappings,
and idempotent migration. The Unity validation request is
`E:\ZendaGame\Genesis\Tools\MCP\fairy-refinement-validate.json`.

The pre-child-restyle trait/hash snapshot is
`E:\ZendaGame\Genesis\Migration\RefinedFairies\before-child-style.json`.
Regression checks compare every original trait except the deliberately changed
direction/proportions and verify byte-identical catalog and halo unlock mappings.
All 255 portrait hashes must change while names, clothing selections, halo styles,
wing styles/scales, palettes, accessories and prop selections remain intact.
This restyle affects generated fairy portraits only, not Zenda's separate VRM,
the backgrounds, the halo-collection UI or save/unlock behavior.

Review art: `E:\ZendaGame\Genesis\Builds\refined-fairies-review.png`.
Listen to `E:\ZendaGame\Genesis\Builds\mellow-button-preview.wav` at normal playback
volume; it includes the source gain. Automated audio checks cannot judge subjective
softness. No physical listening/device/player-build validation is claimed.

512-pixel portraits increase texture memory versus the previous 128-pixel assets;
mobile memory/performance needs profiling before release. Only unlocked/visible
portraits are loaded through the existing runtime paths.

## Halo collection

Every fairy has one luminous halo. Sixteen styles cover angel, demon, fire,
water, earth, wind, ice, lightning, nature, shadow, moon, sun, stars, heart,
time and arcane motifs. Elemental/angel/demon home worlds receive matching
styles; remaining assignments use a stable character-ID hash. Original headwear
remains, but the former headwear-only ring is replaced so each fairy has one halo.
The portrait viewport reserves space above the hair for unclipped halos.

`E:\ZendaGame\Genesis\Assets\Genesis\Sanctum\GenesisSanctum.Halos.cs` implements
a hexagonal fairy-and-halo button at the left of the main Goddess stage, not in
the navbar. The scrollable modal shows previews, locked/unlocked status, donor
fairies and Wear buttons. Owning at least one fairy unlocks her shared halo style;
discovery alone does not. Existing recruited fairies unlock styles automatically.
Zenda starts with Angel and wears the selected halo as a stage overlay projected
above her animated humanoid head in both portrait/full-body modes. This is not
a new mesh or a permanent modification to the protagonist's VRM asset.

The equipped style is an optional `halo` field in version-2 local saves. Older,
invalid or locked saved choices fall back to Angel. Reset clears recruitment
unlocks and restores Angel. Collection is cosmetic and does not alter economy.

Halo sources: `E:\ZendaGame\Genesis\Tools\Migration\FairyHalos.py`.
Runtime mapping: `E:\ZendaGame\Genesis\Assets\Genesis\Resources\Genesis\Data\fairy-halos.json`.
PNG resources: `E:\ZendaGame\Genesis\Assets\Genesis\Resources\Genesis\Art\halos`.
Unity import settings preserve 1024x320 NPOT preview dimensions, transparency,
and disable mipmaps/compression. Regeneration preserves the existing `.meta` files.
For first import use `E:\ZendaGame\Genesis\Tools\MCP\fairy-halos-import.json`.
Unity smoke request: `E:\ZendaGame\Genesis\Tools\MCP\fairy-halos-smoke.json`.
Report: `E:\ZendaGame\Genesis\Builds\fairy-halos-validation.txt`.
Live main-page capture: `E:\ZendaGame\Genesis\Builds\fairy-halos-main.png`.
Player builds and physical touch-device checks remain manual acceptance work.

## HD anime halo redesign

The v8 halo renderer uses luminous metallic/enamel bands, layered glow,
faceted jewels, twelve tiny engraved sigils, glints and a distinct filled
elemental crest for every style. The shared vector source updates both fairy
portraits and Zenda's equipped halo. Standalone textures are 1024x320, rendered
at 2x delivery resolution before downsampling; fairy portraits remain 512x512.
All character traits, halo assignments, recruitment gates and equipped saves
are preserved. The main-page halo button now calls `DrawEnchantedSocket`, the
same theme-backed rendering path as the other floating buttons; it no longer
draws legacy `glassDisc`/`glassRing` textures directly. The imported socket PNG
is hash-checked against the source in
`E:\ZendaGame\Enchanted_Unity_UI_Kit\Assets\EnchantedUI\Art\hex_portrait_socket.png`.

Review all sixteen designs at
`E:\ZendaGame\Genesis\Builds\anime-halos-review.png`.