extends SceneTree

var failures: int = 0
var checks: int = 0
var game: Node
var db: Node

func _initialize() -> void:
	call_deferred("_run")

func _check(condition: bool, description: String) -> void:
	checks += 1
	if not condition:
		failures += 1
		push_error(description)

func _run() -> void:
	game = root.get_node("Game")
	db = root.get_node("DB")
	game.set_process(false)
	_check(not game.persistence_enabled, "Tests must never read or write the player's save")
	_check(not game.claim_creation_chapter(), "An unfinished chapter cannot be claimed")
	game.add_quanta(10.0)
	_check(game.claim_creation_chapter(), "First light can be claimed at ten Quanta")
	_check(is_equal_approx(game.quanta, 25.0), "First light grants exactly fifteen Quanta")
	_check(not game.claim_creation_chapter(), "Claim cannot duplicate an already collected reward")
	_check(not game.buy_generator("reality"), "Undiscovered reality cannot be purchased")
	_check(game.buy_generator("hope_fluctuation"), "Hope creates the first speculative fluctuation")
	_check(game.claim_creation_chapter(), "First fluctuation completes its chapter")
	_check(game.buy_generator("first_intention"), "First rewards fund the next discovery")
	_check(game.claim_creation_chapter(), "The next fairy completes the next chapter")
	_check(game.get_total_qps() > 0.0, "First generator produces passive Quanta")
	var before: float = game.quanta
	game._process(1.0)
	_check(game.quanta > before, "One simulated second creates passive income")
	game.add_quanta(-100.0)
	_check(game.quanta >= before, "Negative creation cannot subtract progress")
	game.add_quanta(1.0e25)
	_check(game.goddess_stage == 5, "A large gain crosses every eligible evolution in one update")
	game.buy_multiplier_mode = -1
	var bulk: Dictionary = game.get_bulk_buy_info(db.GENERATORS[1])
	_check(bulk.amount > 1 and bulk.can_afford, "MAX mode calculates an affordable bulk purchase")
	game.chrono_buff_timer = 20.0
	game.miracle_cooldowns["genesis_burst"] = 25.0
	game.counts["wormhole_nexus"] = 1
	_check(game.perform_big_bloom(), "A mature universe can bloom")
	_check(game.quanta == 0.0 and game.goddess_stage == 0, "Bloom resets run currency and goddess")
	_check(game.counts["quark_filament"] == 0, "Bloom resets generators")
	_check(game.chrono_buff_timer == 0.0 and game.miracle_cooldowns["genesis_burst"] == 0.0, "Bloom resets temporary miracles")
	_check(game.completed_chapters.size() == 3, "Creation memories survive the bloom")
	_check(game.genesis_seeds > 0, "Bloom awards permanent seeds")
	_check(not game.perform_big_bloom(), "An empty universe cannot bloom twice")
	var scene: PackedScene = load("res://scenes/Main.tscn")
	var ui: Control = scene.instantiate()
	root.add_child(ui)
	await process_frame
	_check(ui.get("journey_title") != null, "Main scene builds the creation journey UI")
	_check(ui.get("chronicle") != null, "Main scene builds the persistent chronicle")
	_check(ui.get("bloom_confirmation") != null, "Prestige has a confirmation dialog")
	var manifest_file: FileAccess = FileAccess.open("res://assets/generated/manifest.json", FileAccess.READ)
	_check(manifest_file != null, "Generated asset manifest exists")
	if manifest_file != null:
		var manifest: Dictionary = JSON.parse_string(manifest_file.get_as_text())
		var assets: Array = manifest.get("assets", [])
		_check(assets.size() == db.GENERATORS.size() + 20, "Every stage and shared asset is present in the manifest")
		for asset: Dictionary in assets:
			_check(ResourceLoader.load(str(asset.path)) != null, "Asset imports and loads: %s" % asset.path)
			if str(asset.path).contains("/icons/"):
				_check(asset.category == "fairy", "Every gameplay icon is represented by a fairy")
	var visual: Control = ui.get("visualizer") as Control
	_check(visual.stage_textures.size() == 6, "All six goddess emblems are loaded")
	_check(visual.fairy_textures.size() == db.GENERATORS.size(), "Each creation layer has a fairy in the animated scene")
	for generator: Dictionary in db.GENERATORS:
		_check(db.FAIRY_NAMES.has(generator.id), "Each creation fairy has a character name")
	var tap: InputEventMouseButton = InputEventMouseButton.new()
	tap.button_index = MOUSE_BUTTON_LEFT
	tap.pressed = true
	tap.position = visual.size * 0.5
	visual._gui_input(tap)
	_check(visual.creation_sparks.size() == 8, "Tapping emits eight creation sparks")
	for index: int in range(20):
		visual._gui_input(tap)
	_check(visual.creation_sparks.size() <= 128, "Rapid tapping keeps particles bounded")
	visual._process(1.0)
	_check(visual.creation_sparks.is_empty(), "Expired creation particles are removed")
	var audio: Node = ui.get("creation_audio") as Node
	_check(audio.players.size() == 5, "All five synthesized sounds are wired")
	for stage: int in range(6):
		game.goddess_stage = stage
		visual.queue_redraw()
		await process_frame
	ui.queue_free()
	await process_frame
	print("GENESIS TESTS: %d checks, %d failures" % [checks, failures])
	quit(1 if failures > 0 else 0)