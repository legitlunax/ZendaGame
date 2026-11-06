# Dimension fairy population

The catalog now contains 255 recruitable fairies: the original 106 progression
creations plus 149 optional resident recruits. All 30 dimensions have at least
eight gathering fairies, including at least three new residents per dimension.
Lady Morthelia remains an additional named resident of the Death Dimension in
the existing Soul storyline; she is not a separate world milestone.

## Recruitment and continuity

- Residents unlock when their `recruit_after` home anchor is owned. This is the
  last original creation in their home dimension, not a new progression step.
- Each resident has an independent stable generator ID, level, cost, production
  cycle, gallery history, home story link and companion selection.
- Residents do not increase CreationLevel or bypass prerequisite world stories.
- Costs are 30–120% of the home anchor's initial cost; base production is 6–12%
  of its base production. They are optional side investments, not prerequisites.
- Original generator order, costs, chapter IDs and chapter objectives are intact.
  Save restoration initializes missing resident counts to zero. Existing owned
  creations and completed/read chapters retain their original IDs.
- The 107 original chapters remain the quest structure. Home-anchor dialogues
  introduce their additional residents; recruited residents have personal gallery
  histories. These additions lengthen the relevant conversations.

## Mobile presentation

Gather has previous/next world arrows and the Worlds list opens the chosen
world's recruitment page. New progression still automatically follows a newly
formed world. A 132-point scrollable neighbourhood panel shows up to 12 owned
fairies, with unscaled-time bob/sway animation. Tap a resident to open her
history. It shows only recruited residents, not locked or unowned characters.
The production rows below remain individually upgradeable and scrollable.

## Artwork and regeneration

149 128px transparent chibi portraits use the archived Godot `fairy_art`
renderer and existing Godot rasterizer, replacing the simplified resident template.
They retain the original glossy eyes, six hairstyle silhouettes, three costume
families, varied skin/hair/eye palettes and home-world emblems. They are
**not AI-generated**. Sources and provenance are recorded in
`E:\ZendaGame\Genesis\Migration\DimensionFairies\manifest.json`.

Regenerate idempotently with:

```powershell
& 'E:\ZendaGame\Genesis\Tools\unity-mcp\Server\.venv\Scripts\python.exe' 'E:\ZendaGame\Genesis\Tools\Migration\Expand-DimensionFairies.py'
```

Requires the project's existing Godot executable. Unity imports the resulting
PNG resources. No VRM copies or additional 3D rigs are instantiated.

For artwork-only regeneration without rewriting any catalog data, run
`E:\ZendaGame\Genesis\Tools\Migration\ResidentPortraits.py` with the same Python
executable. Its review sheet is
`E:\ZendaGame\Genesis\Builds\resident-godot-portraits.png`.

## Validation

MCP smoke inputs and timestamped reports are stored under
`E:\ZendaGame\Genesis\Tools\MCP` and `E:\ZendaGame\Genesis\Builds`:

- expanded-fairies-smoke: recruitment, upgrades, production, portraits, homes,
  dialogue, histories, stable saves, legacy restoration and progression guards.
- expanded-village-smoke: 30-world browsing, row counts, visible residents,
  history/companion selection, objective links, unchanged counts and reset.
- Existing gathering, active-world, world-story-gate, journal and code/reset
  smoke checks were also run.
- Portrait evidence: `E:\ZendaGame\Genesis\Builds\expanded-fairy-village.png`.

Rebuilt-player/device performance, physical touch interaction and long-session
economy balance remain acceptance work. No preview scene needs to be saved.