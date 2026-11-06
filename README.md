# Genesis: Goddess of Creation

A Godot 4 idle-game prototype about nurturing a universe, rather than annihilating one.

## Unity project

The Unity implementation is available in [`Unity/Genesis`](Unity/Genesis) and
requires Unity **6000.6.7f1**. Build instructions are in
[`Unity/Genesis/BUILDING.md`](Unity/Genesis/BUILDING.md); local package dependencies
are under `Unity/Tools`, and build/validation tools are under
`Unity/Genesis/Tools`. Unity editor caches, build outputs, signing material, and
non-redistributable character models are not included. The Godot project remains
at the repository root.

## Play

### View the playable demo

Double-click `E:\Godot\GenesisIdleRPG\Play Demo.bat`.
The preview dropdown offers eighteen presets: Only Hope, Five Elements, Living Worlds,
Reality, All-Seeing, Living Divinity, Dao Dimensions, Omnielle, Angel Realm, Demon Realm,
Elves & Fairies, Gods Realm, Shadow Realm, Realm Concord, Universal Emotions, Universal Heart, Human & Soul, and Embodied Soul. All presets remain playable: tap, buy generators, claim chapters,
try miracles, and perform a Big Bloom. Switching eras restarts that preview.
Demo mode never loads or writes your normal save; demo progress is temporary.
Use **Sound: ON/OFF** to mute and **How to play** for the quick-start guide.

```powershell
& 'E:\Godot\Godot_v4.7.2-stable_win64.exe' --path 'E:\Godot\GenesisIdleRPG' -- --demo
```

### Normal saved game

Open `E:\Godot\GenesisIdleRPG\project.godot` in Godot and press **F6** in the main scene or **F5** to run the project.
Tap the light to create Quanta, claim Creation Journey rewards, and buy generators in Cosmic Creation.
Generator production doubles at each 25 owned. Miracles offer bursts and temporary acceleration.
After constructing a Universe Horizon and earning at least one million run Creation
Potential, the optional Big Bloom exchanges the current universe for Genesis Seeds.
Seeds, relics, lifetime creation, and completed journey chapters survive; the run resets.
The Chronicle records completed chapters. Journey rewards are once per save, not once per universe.

## Verify

```powershell
& 'E:\Godot\Godot_v4.7.2-stable_win64_console.exe' --headless --path 'E:\Godot\GenesisIdleRPG' --editor --quit
& 'E:\Godot\Godot_v4.7.2-stable_win64_console.exe' --headless --path 'E:\Godot\GenesisIdleRPG' --script 'res://tests/CreationJourneyTest.gd' -- --genesis-test
& 'E:\Godot\Godot_v4.7.2-stable_win64_console.exe' --headless --path 'E:\Godot\GenesisIdleRPG' --script 'res://tests/DemoTest.gd' -- --demo
```

Tests use the existing autoloads but disable save loading and writing with `--genesis-test`.
Player saves remain in Godot's user data folder as `genesis_save_v1.json`.
Older saves without journey data remain supported.

## Local Godot MCP

The existing server lives at `E:\Godot\godot-mcp\build\index.js`; both workspace and project have `mcp_servers.json` configuration.
It is a **stdio server**, started by an MCP client when needed, not an HTTP service.
The development bridge uses the server's already-installed MCP SDK; no extra packages or cloud authentication are needed.

```powershell
node 'E:\Godot\GenesisIdleRPG\tools\godot_mcp.mjs' list
node 'E:\Godot\GenesisIdleRPG\tools\godot_mcp.mjs' get_godot_version
node 'E:\Godot\GenesisIdleRPG\tools\godot_mcp.mjs' get_project_info '{"projectPath":"E:/Godot/GenesisIdleRPG"}'
```

Each bridge invocation opens and closes its own MCP session. Stateful tools such as `run_project` and `get_debug_output` should be used within one persistent MCP-client session, not separate bridge invocations.

## Current scope

Playable prototype with 106 discoveries and 107 guided chapters.
Beyond Reality, sixteen discoveries explore life's latent divinity, all-seeing
awareness, omnipresence, omnipotence, understanding, compassion, and Omniall.
This is the setting's fictional philosophy: life recognizes a shared divine nature,
not a statement that the player possesses supernatural abilities.
The opening begins with zero Hope, no idle income, no goddess portrait, and no stars.
Tap the void, discover a Possibility Fluctuation for five Hope, then grow through
intention, yin/yang, Wood/Fire/Earth/Metal/Water, time, space, particles, atoms,
nebulae, stars, living worlds, galaxies, a universe, daughter universes,
a multiverse, an omniverse, and reality. Hope becomes Creation Potential
when the first quarks are nurtured; this is one resource with a changing narrative name.
Future generator cards stay hidden until their preceding layer exists; miracles
unlock at atoms, and optional prestige requires the first universe.
The speculative beginning and transcendent stages are fictional cosmology,
not a representation of established physics. Existing saves retain owned generators.
Includes an original generated starter pack: generator/relic/miracle icons, six Zenda
fairy portraits, nebula background, particle masks, and five synthesized sound cues.
All 111 gameplay icons depict named anime-inspired chibi fairies, with
layered hair, expressive eyes, modest petal dresses, translucent wings, and staffs.
Creation fairies replace abstract orbiting bodies; the empty opening is preserved,
and Elpis appears only after the first Possibility Fluctuation is discovered.
Art is original procedural vector prototype artwork, not production-painted anime art.
Detailed progression, harmony bonuses, balancing limitations, and save compatibility:
`E:\Godot\GenesisIdleRPG\PROGRESSION.md`.

## Human life and soul resonance

The fairy's **Designs** menu collects story-unlocked appearances. Ivory Queen is
always available; Living Biosphere, Angel Realm, Elves and Fairies Realm, Shadow
Realm, and Embodied Soul Dimension chapters unlock additional variants when claimed.
Earlier designs remain selectable, unlocks survive Big Blooms, and your equipped
choice is saved separately. Current variants share one mesh; they are not yet
separately authored outfits. See `E:\Godot\GenesisIdleRPG\FAIRY_3D.md`.

Six further discoveries imagine humans living on Earth while their souls may also
reside in the Angel Realm or other realms simultaneously. This fictional saga
emphasizes coexistence, earthly care, and respect for different beliefs, not escape
from the body or claims of scientific proof. It ends in the Embodied Soul Dimension.
Select **Human & Soul** to play its opening or **Embodied Soul** to build its finale.
Provenance and rebuild instructions: `E:\Godot\GenesisIdleRPG\assets\generated\ASSET_NOTES.md`.
Responsive portrait/landscape layout and Web/Android export presets are now included.
See `E:\Godot\GenesisIdleRPG\PLATFORM_GUIDE.md` for build steps and current blockers:
matching export templates are missing and Android's Java SDK path is unset.
No APK/browser build has been successfully exported or tested on a physical phone yet.
Painted character artwork, music, saved audio preferences, device validation, and balance tuning
remain future work.
## Dimension and phone preview update

Major arcs now end in dimension construction. Existence rests on interwoven domains,
not a mandatory cosmic ranking. The game follows one sequential path; it does not
yet offer free-order building. Four Dao discoveries culminate in a collective
Three Thousand Dao Dimensions milestone before Reality.

Launch `E:\Godot\GenesisIdleRPG\Play Demo.bat` for a 390x844 phone-sized
portrait window, or `E:\Godot\GenesisIdleRPG\Play Desktop Demo.bat` for desktop.
Fresh portrait capture: `E:\Godot\GenesisIdleRPG\mobile-preview.png`.
This desktop simulation does not replace testing an actual phone.

## Omnielle: Universe Confluence

Eight new discoveries lead to Omnielle's Omniverse Confluence Dimension, a dedicated
nine-stage arc about infinite superposed universes with independent time, space,
and laws. This is fictional cosmology, not a scientific claim. Universes coexist
without collapse or erased individuality. Select the Omnielle preview to build
her dimension in the phone-sized demo. The economy and rendering remain finite.

## Dimensional Life Realms

After Living Divinity, eighteen discoveries introduce Angel, Demon, Elves and
Fairies, Gods, and Shadow realms. Each has an awakening, a covenant, and a dimension
build. A final Concord of Living Realms connects their distinct homes without
erasing their inhabitants. There are now 29 dimension construction milestones.
Choose a realm preset to fund its Build action. Earlier discoveries, costs, chapter
IDs, and demo indices are retained. This is a lore and progression expansion, not
a realm-travel or combat system. Existing 3D queen animations remain unchanged.

## Universal Emotions

Ten further discoveries link feelings with creation through fictional lore, ending
in the Universal Heart Dimension. Claiming Joy, Sorrow, Wonder, Tenderness, Courage,
Anger, Compassion, Serenity, and Celebration unlocks their corresponding character
animations. Idle is always available; other menu entries remain visibly locked
until remembered. Unlocks survive Big Blooms through existing saved chapter IDs.
Tap happiness and chapter dances now respect unlocks. The Universal Emotions demo
starts the arc; Universal Heart previews all expressions. These use the placeholder
3D character, not a new production-quality model. Full mapping and limitations:
`E:\Godot\GenesisIdleRPG\PROGRESSION.md`.
