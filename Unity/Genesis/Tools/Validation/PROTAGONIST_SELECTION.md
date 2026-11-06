# Light / Darkness protagonist and compact home header

## Behavior

- First launch requires choosing Light (the original scene model) or Darkness
  (the supplied ZendaGenesis VRM), with an editable randomly suggested name.
- Confirm before entering the game. Random rerolls the pending name; invalid or
  blank names are rejected. Names support Unicode, are limited to 24 text elements,
  and remove control/format characters and markup delimiters.
- Each character keeps an independent name. Tap the homepage name to rename or
  switch, with live 3D previews. Cancel discards pending edits and restores the
  equipped model. Character selection is presentation-only, not an economy bonus.
- Existing version-2 saves without a profile prompt once without resetting game
  progress. Confirmed profiles skip selection on later launches. Reset removes
  the profile and asks again. Profile data is an optional `protagonist` object.
- The homepage header is 52 logical pixels tall: editable name on the first row,
  current progression dimension on the second. Accumulated Hope and Hope/second
  appear in a matching panel to the right of the tap button, mirroring the
  inspiration panel on the left. Companion names and mood captions are not drawn
  as floating text; fairy animations, emotes and interactions remain unchanged.
- Story character IDs, narrative text and fairy names are not renamed by this
  feature. Halos remain equipped independently and follow either model's head.

## Implementation

- Profile: `E:\ZendaGame\Genesis\Assets\Genesis\Scripts\Core\GenesisProtagonistProfile.cs`.
- Runtime and selection UI: `E:\ZendaGame\Genesis\Assets\Genesis\Sanctum\GenesisSanctum.Protagonist.cs`.
- Darkness source: `E:\ZendaGame\Genesis\Migration\CharacterSource\ZendaGenesis.vrm`.
- Packaged resource: `E:\ZendaGame\Genesis\Assets\Genesis\Resources\Genesis\Characters\ZendaGenesis.vrm`.
  The copy is SHA-256-identical to the source, imported through the existing
  UniVRM importer with the Universal Render Pipeline configuration. Neither
  original model nor the saved scene is modified. The second model is loaded
  lazily and only the displayed model is active, with the shared idle controller.
- The chooser owns the input surface and pauses session advancement until
  confirmed; focus/offline catch-up retains the pre-existing persistence rules.

## Validation

- `E:\ZendaGame\Genesis\Tools\Validation\IdleRegression\Program.cs`: profile
  defaults, Unicode names, validation, individual names, roundtrip, legacy and
  corrupt identity handling, plus existing economy simulations.
- `E:\ZendaGame\Genesis\Tools\MCP\protagonist-smoke.json`: isolated Unity
  profile, modal, save, legacy, reset, resource and header checks.
- `E:\ZendaGame\Genesis\Tools\MCP\protagonist-live-validate.json`: live Darkness
  idle/reactions and supported-material verification.
- Selection screenshot: `E:\ZendaGame\Genesis\Builds\protagonist-darkness-selection.png`.
- Homepage screenshot: `E:\ZendaGame\Genesis\Builds\protagonist-darkness-home.png`.

No player builds or hardware touch/keyboard tests have been performed for this
feature. The additional VRM increases build size and runtime memory when loaded;
mobile/WebGL performance and on-screen keyboard layout remain device acceptance work.