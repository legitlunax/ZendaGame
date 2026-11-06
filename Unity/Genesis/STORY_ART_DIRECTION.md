# Zenda: illustrated memories

Selected style: detailed fantasy visual novel; polished character artwork, dramatic
lighting, cinematic environments. All resident Ladies are adult women with distinct
silhouettes, personalities and culturally coherent fantasy costumes. Use the existing
Zenda model/portraits as character references; do not invent a replacement appearance.
Keep Zenda's face, hair, clothing and proportions consistent across scenes.

## Delivery and integration

No new scene illustrations have been generated. Existing portraits are fallback art,
not finished two-character scenes. Use a reference-capable image generator or artist.
Do not embed text in generated art: Unity overlays readable, editable narration on the
image's lower area. Leave the lower 45 percent quiet/dark and keep faces in the upper
half. Export PNG, preferably 1440 x 1800; review phone crops and text contrast.
Import as Texture2D with readable text disabled in the art itself. No paid provider
or runtime network dependency has been added.

Resources location: Assets/Genesis/Resources/Genesis/Art/stories/<chapter_id>/
`scene.png` supplies the chapter image; `page_01.png`, `page_02.png`, etc. override
individual pages. Missing page art falls back to scene, then existing 3D portrait.
Only the current illustration is held by the reader; imported assets belong to Unity
Resources and are not destroyed by the reader. Illustration mode disables the portrait
camera and silent mouth animation; leaving restores the Goddess view.

## Common prompt direction

Detailed fantasy visual-novel illustration, two adult female friends, Zenda matching
the supplied reference and the named resident Lady, visibly collaborating rather than
one commanding the other. Cinematic environmental storytelling, expressive hands and
faces, refined costumes, dramatic soft light, no text, no watermark, no UI. Compose
for a vertical mobile story page, important characters upper half, quiet dark lower
area for separately rendered narration. Maintain consistent character references.

## First encounter scenes

### Lady Morthelia, Keeper of the Last Garden
Chapter: `discover_soul_dawn`

Zenda and Morthelia plant silver flowers beside a quiet gate in the Death Dimension. Morthelia wears dusk-violet robes with pale floral embroidery, compassionate rather than frightening. A lantern marks a farewell; no gore. Also prepare a separate Animelle Soul scene, since this chapter has two residents.

### Lady Vivielle, the First Bloom
Chapter: `discover_divine_life`

Zenda and Vivielle kneel beside the first living ocean, cupping a luminous seed together. Vivielle has botanical embroidery and sea-green accents. Dawn reflects in tiny newly living forms.

### Lady Ignis, the Hearthkindler
Chapter: `discover_element_fire`

Zenda and Ignis build a stone hearth beside an ember river. Ignis wears copper-trimmed artisan clothing and ember-red accents. A small flame illuminates their relieved smiles; fire gives shelter rather than destruction.

### Lady Dimensia, Weaver of Horizons
Chapter: `discover_space_dimensions`

The friends hold opposite ends of a luminous doorway, shaping a meeting place between distant star fields. Dimensia has geometric indigo clothing; layered horizons suggest space, not control over time.

### Lady Chronaria, Keeper of Becoming
Chapter: `discover_time_flow`

The friends arrange luminous moment-seeds along a flowing ribbon, with a bud, bloom and falling petal visible together. Chronaria has bronze and midnight-blue fabric; show patient collaboration, not a mechanical clock queen.

### Lady Sylphariel, Tender of the World-Tree
Chapter: `discover_elf_fairy_realm`

Zenda and Sylphariel guide a living branch into a bridge between world-tree homes. Petal villages glow below. Sylphariel has leaf-silk clothing and delicate translucent wings; diverse residents work freely nearby.

### Lady Velkaria, the Freeflame
Chapter: `discover_demon_realm`

The friends raise the first free foundry arch above an obsidian garden. Velkaria has elegant horns and practical ember-lit artisan clothing. Warmth, consent and creative passion define the scene, not evil or conquest.

### Lady Seravelle, the Open Wing
Chapter: `discover_angel_realm`

The friends open the gates of a cloudborne refuge. Seravelle has luminous wings and pearl-blue sanctuary robes. Winged residents tend healing gardens; welcoming gestures, no kneeling worshippers.

For each encounter expand into arrival, meeting, shared construction and farewell/return. Map artwork to actual story pages after pagination; do not assume four text pages per chapter.
