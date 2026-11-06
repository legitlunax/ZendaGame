# Game-wide Enchanted presentation

The theme now applies beyond the full-story reader. Existing IMGUI screen controllers
draw the real kit's sliced panels, buttons and hex sockets through
`E:\ZendaGame\Genesis\Assets\Genesis\Sanctum\GenesisSanctum.EnchantedTheme.cs`.
The conversation retains its existing uGUI adapter. No second game model is introduced.

Affected presentation: Goddess header/inspiration/navigation, gathering cards and
controls, journeys, worlds, journal, gallery, background/reaction dialogs, code input,
completion and asset credits. Existing art, fonts, audio and controller actions remain.
This is a shared-component visual redesign, not a replacement layout for every screen.

Install resource references with **Genesis > Presentation > Install Game-wide
Enchanted Theme**. It creates a Resources theme asset referencing imported artwork;
runtime cleanup destroys only generated fallback/transparent textures.

Validation artifacts are under `E:\ZendaGame\Genesis\Builds`:
`enchanted-game-*-1080-1920.png`, `enchanted-game-*-1280-720.png`,
`enchanted-game-captures.json`, and `enchanted-game-validation.txt`.
The capture script restores the selected tab/section, does not grant currency,
and does not import the older conversation-test progress backup. Normal idle game
time and automatic saves continue during capture. Original scenes are not saved.

Script reload while playing loses the session-only game model in the existing
controller. Stop/restart Play mode normally before validation. Do not invoke Awake
manually or restore an unrelated historical test backup.

Still required: screen-specific layout refinement (especially compact/wrapped button
captions), physical input, popup/credits review, long text/notches, and fresh player
build/device tests. The previous successful WebGL output predates this theme extension.