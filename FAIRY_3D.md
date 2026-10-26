# Animated 3D fairy prototype

The main cosmic display now renders an original procedural adult fairy queen inside a
transparent, isolated SubViewport. Existing discovery icons and orbit companions
remain SVGs. No external models, downloads, or plugins are required.

## Interactions

- Idle: hovering, head movement, blinking, and wing flapping.
- Happy: raised arms, gentle floating, and a brighter facial expression; triggered by taps.
- Dance: swaying, turning, and alternating limb movement; triggered by chapter rewards.
- Shy: lowered head, tilted pose, and softened eyes.
- Surprised: open mouth.
- Sorrow: lowered head, softened eyes, and lowered arms.
- Courage: lifted chin and a resolute bent-arm pose.
- Anger: narrowed tilted eyes and a firm protective stance.
- Compassion: open, gently moving arms and an inviting head tilt.
- Serenity: nearly closed eyes, relaxed arms, and quiet hovering.
- The Emotion menu previews unlocked states for six seconds. Reactions return to idle.
- The fairy remains hidden during the initial void. Dress and hair colors follow
  a stable ivory/violet palette; stage colors tint only edge lighting. Emotion
  choices do not alter saves or progression.

## Implementation and limits

The queen is designed as a 25-year-old adult, with approximately six-head-tall
proportions rather than chibi anatomy. Her outfit uses an opaque fitted bodice,
off-shoulder neckline, gold trim, and an opaque underdress above a side slit in
the long gown. Long hair, a tiara, earrings, and veined wings complete the design.
Elbows, knees, hair locks, and gown sway are animated alongside the existing joints.

`scripts/FairyAvatar3D.gd` builds simple meshes and joint pivots and animates them
procedurally. This is not a finished anime model, a skinned Skeleton3D rig, or an
AnimationPlayer clip library. There is currently one shared 3D body design, not
separate 3D character models for every discovery. A future production model needs authored topology,
textures, skinning, facial blend shapes, and animation clips. Real phone rendering
performance and platform exports still need device validation.

`tests/FairyAvatarTest.gd` covers state transitions, pose changes, blinking,
interaction triggers, and visibility. Run with `-- --demo` to isolate saves.

## Unlock contract

Only Idle is available initially. `Game.is_emotion_unlocked()` derives permanent
unlocks from completed Universal Emotions chapter IDs. `CosmicVisualizer` syncs
the menu and avatar allowlist, including when demo previews change. Automatic
reactions use the same gate as menu playback. Claims play the new expression;
counts alone do not unlock it. Big Bloom keeps completed chapters and expressions.
The Universal Heart demo exposes all ten states. These remain procedural poses,
not production-authored clips. See `E:\Godot\GenesisIdleRPG\PROGRESSION.md`.

## Reference-guided rendering pass

Visual reference: `E:\Godot\Sailor Moon Characters personal creation.jpg`.
The illustration guides color, layered costume detail, luminous hair highlights,
gold ornament, and moonlit composition; it is not pasted onto the 3D model or
included as a runtime asset. The heroine remains an original adult character.

`E:\Godot\GenesisIdleRPG\shaders\CelestialSilk.gdshader` adds softened lighting
bands, controlled highlights, and an edge glow to hair and fabric. Four-sample
MSAA, warm/cool directional lighting, a stable palette, curved tapered hair locks,
eyelashes, a shaped face, petal overskirt, and ribbon panels improve the prototype.
`E:\Godot\GenesisIdleRPG\scripts\QueenArtDetails.gd` builds the strand/panel meshes.
The canvas shows fewer, smaller companions and a quieter lunar backdrop.

The Portrait button switches to an upper-body camera; Full body restores framing.
Capture: `E:\Godot\GenesisIdleRPG\queen-portrait-preview.png`.
Rendered checks use the Compatibility renderer on the desktop GPU. Real-device
performance is still unverified. This is not reference-quality character art:
production topology, authored textures, detailed hair, skinning, and facial blend
shapes remain necessary. Shader improvements do not replace those assets.

## Softer surfaces and future wardrobe foundation

Hair paths, ribbon paths, and body/gown profiles now use interpolated curves.
Round primitives have increased tessellation; skin uses softer lighting, silk has
continuous light transitions, and wings have a gently curved surface. Desktop
portrait inspection still shows a doll-like procedural character, assembled limbs,
and broad simple wings. This is not a completed human-anatomy or 2.5D art solution.
Increased geometry has not been profiled on a physical phone.

`E:\Godot\GenesisIdleRPG\scripts\FairyAppearance.gd` is a visual-only Resource.
`FairyAvatar3D.apply_appearance()` sets outfit/accent/hair colors, wing color and
scale, wing visibility, and head-cosmetic visibility without resetting animation,
camera, progression, or emotion permissions. Invalid wing scales are rejected.
The bodice/underdress/gown share an OutfitSlot; tiara and earrings share a
HeadCosmeticsSlot. Sleeves, shoes, and body jewelry still attach to limb/body nodes.
This is a partial modular foundation, not a complete interchangeable wardrobe.

Story-earned appearance variants and a Designs menu are now included (see below).
Alternate outfit meshes and alternate wing designs are not included. Production art should keep the adult
identity and provide a shared skinned body/rig, fitted interchangeable outfits,
wing attachments, hair and cosmetic attachments, facial expressions, and clips.
Future story unlocks should use stable chapter IDs and remain separate from the
chosen equipped appearance; a Bloom should not erase remembered cosmetic unlocks.

## Collectible story designs

The Designs menu at the lower-left of the fairy canvas lists every design, with
locked entries disabled. New unlocks never replace the currently equipped design.
All earlier unlocked designs remain wearable, including after Big Bloom.

| Design | Claimed chapter required |
| --- | --- |
| Ivory Queen | Always available |
| Earth Bloom | Living Biosphere (`first_life`) |
| Angel Dawn | Angel Realm |
| Sylvan Bloom | Elves and Fairies Realm |
| Shadow Moon | Shadow Realm |
| Soul Concord | Embodied Soul Dimension |

Unlocks derive from persistent completed chapter IDs, not current generator counts.
The equipped stable design ID is saved separately as `equipped_appearance`.
Old saves use Ivory Queen; invalid or locked saved selections fall back to it.
Demo era changes intentionally reset previews to Ivory Queen and use that era's
remembered chapters; demo mode never reads or writes the player's normal save.

Catalog: `E:\Godot\GenesisIdleRPG\scripts\FairyWardrobe.gd`.
These six designs are palette/wing-size/head-cosmetic variants of the current
procedural mesh, not six authored outfits or archived historical prototype models.
Future authored designs should append stable IDs rather than overwrite earlier
entries. There are no stat bonuses or emotion permission changes from clothing.