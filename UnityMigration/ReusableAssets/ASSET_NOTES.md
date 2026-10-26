# Original Genesis starter pack

These assets were generated specifically for this project by
`E:\Godot\GenesisIdleRPG\tools\generate_assets.py`.
No third-party images, fonts, sound recordings, samples, or downloaded packs were used.
No third-party attribution is required for this generated pack. This provenance statement
does not make a legal claim about copyright protection for AI-assisted output.

## Inventory

- 111 transparent 384px SVG fairy illustrations: 106 generators, three relics,
  two miracles. Each has a large chibi face, glossy eyes, translucent wings, a celestial
  tiara, sailor-inspired collar, contrasting bow, pleated skirt, boots, and motif staff.
- Six transparent 512px SVG Zenda fairy evolution portraits. These are stylized
  procedural vector illustrations, not painted or diffusion-generated anime art.
- One 1024px SVG nebula background.
- Three 64px RGBA PNG particle masks: glow, sparkle, ring. White masks can be tinted.
- Five mono 16-bit 22,050 Hz WAV cues: tap, purchase, chapter, evolution, miracle.
- `manifest.json` lists all 126 runtime assets, categories, and SHA-256 hashes.
  Recognition fairies introduce eye, infinity, heart, and living-seed staff motifs.

## Rebuild

```powershell
python 'E:\Godot\GenesisIdleRPG\tools\generate_assets.py'
& 'E:\Godot\Godot_v4.7.2-stable_win64_console.exe' --headless --path 'E:\Godot\GenesisIdleRPG' --editor --quit
```

The generator uses Python's standard library only and a fixed random seed.
Regeneration overwrites the listed generated assets; put hand-edited replacements
in a separate directory. Godot creates its own import metadata.

## Integration and limitations

Generator/relic cards and miracle buttons display fairy representatives. The cosmic canvas
uses the nebula, a procedural 3D queen, floating SVG companions, glow, and bounded tap particles. CreationAudio
plays successful purchases, taps, chapter claims, evolutions, and miracles.
Tap audio is rate-limited and all cues start at a conservative -12 dB player volume.

This is an editable prototype art direction, not a final illustration or music pack.
There is no ambient music, voice acting, or audio settings menu in this pass.
Review loudness and visual contrast on actual target devices before release.
Omnielle and the confluence companions use an original overlapping-world staff motif.

## Celestial magical-girl redesign

All 111 icons and six portraits use original chibi magical-girl designs, with six
hairstyle silhouettes, varied skin tones, accent bows, and arm poses. Inspired by
classic magical-girl visual language; no franchise characters or downloaded art.
This remains procedural SVG prototype artwork, not hand-painted character art.

## Soft moe character direction

All 117 SVG character assets use softer outlines, rounded cheeks, glossy pastel eyes,
hair highlights, and varied expressions. Three costume families alternate: playful
petal-clip sprites in capelet dresses, side-fringe ribbon guardians in lace-trimmed
pinafore dresses, and celestial magical girls. Six hair silhouettes are combined
with skin, iris, hair, and accent palettes. These are original fantasy characters,
not replicas of the anime references. Artwork remains procedural SVG prototype art.

## Dimensional Life Realms

Eighteen additional discovery icons represent the five living realms and their
concord. Angel icons introduce halo-and-wing staff motifs, demonic icons use
horned crests, gods use crowns, sylvan peoples use leaves and flowers, and shadow
peoples use moons. These are illustrative motifs, not separate 3D models or realm
environments. Existing discovery art stays stable because the saga is appended;
shared relic/miracle designs retain their original fixed appearance indices.

Ten Universal Emotions discovery icons add symbolic sun, water, spark, heart,
seed, flame, moon, flower, and confluence staffs. They retain existing SVG art
conventions; character animation unlocks are handled in gameplay, not SVG files.
