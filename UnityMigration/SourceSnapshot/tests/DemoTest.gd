extends SceneTree

var failures: int = 0
var checks: int = 0

func _initialize() -> void:
	call_deferred("_run")

func _check(condition: bool, description: String) -> void:
	checks += 1
	if not condition:
		failures += 1
		push_error(description)

func _run() -> void:
	root.size = Vector2i(1280, 720)
	var game: Node = root.get_node("Game")
	game.set_process(false)
	_check(game.demo_mode and not game.persistence_enabled, "Demo never loads or writes a player save")
	_check(not game.load_demo_era(-1) and not game.load_demo_era(game.DEMO_ERAS.size()), "Invalid preview eras are rejected")
	var scene: PackedScene = load("res://scenes/Main.tscn")
	var ui: Control = scene.instantiate()
	root.add_child(ui)
	await process_frame
	_check(ui.demo_selector.item_count == game.DEMO_ERAS.size(), "Compact selector contains every playable era")
	var utility_popup: PopupMenu = ui.utility_menu.get_popup()
	var was_muted: bool = AudioServer.is_bus_mute(0)
	utility_popup.id_pressed.emit(0)
	_check(AudioServer.is_bus_mute(0) != was_muted, "Compact menu toggles sound")
	utility_popup.id_pressed.emit(0)
	_check(AudioServer.is_bus_mute(0) == was_muted, "Sound toggle can restore its previous state")
	utility_popup.id_pressed.emit(1)
	_check(ui.help_dialog.visible, "Compact menu opens How to play")
	ui.help_dialog.hide()
	for era: int in range(game.DEMO_ERAS.size()):
		ui.demo_selector.item_selected.emit(era)
		await process_frame
		_check(game.demo_era == era, "Era button selects preset %d" % era)
		_check(ui.demo_selector.selected == era, "Selected era is highlighted")
		_check(game.get_total_qps() > 0.0 if era > 0 else game.get_total_qps() == 0.0, "Only Hope starts without passive income")
		var before: float = game.quanta
		game.click_cosmic_core()
		_check(game.quanta > before, "Each era remains playable")
	game.activate_miracle("chrono_harmony")
	game.completed_chapters.append("first_light")
	game.load_demo_era(0)
	_check(game.quanta == 0.0 and game.get_creation_level() == 0, "Opening contains nothing but hope")
	_check(game.is_generator_discovered("hope_fluctuation") and not game.is_generator_discovered("quark_filament"), "Only the first speculative discovery is available")
	_check(not game.buy_generator("reality"), "Late-game purchasing cannot bypass discoveries")
	game.activate_miracle("genesis_burst")
	_check(game.quanta == 0.0, "Miracles cannot skip the empty opening")
	var db: Node = root.get_node("DB")
	for index: int in range(db.GENERATORS.size()):
		var generator: Dictionary = db.GENERATORS[index]
		_check(game.get_creation_level() == index, "Discovery level advances exactly one layer")
		game.add_quanta(float(generator.base_cost))
		_check(game.buy_generator(generator.id), "Every layer can be reached through the discovery chain")
		if not game.has_reached("wormhole_nexus"):
			_check(game.get_pending_genesis_seeds() == 0, "Prestige cannot bypass construction of the first universe")
	_check(game.get_creation_level() == db.GENERATORS.size(), "The complete chain reaches Reality")
	var dimension_count: int = 0
	var arc_ends: Array[String] = []
	for index: int in range(db.GENERATORS.size()):
		var stage: Dictionary = db.GENERATORS[index]
		if not str(stage.dimension).is_empty():
			dimension_count += 1
			_check(str(CreationJourney.CHAPTERS[index + 1].title).contains("Build "), "Dimension chapters explicitly ask for construction")
		if index == db.GENERATORS.size() - 1 or stage.era != db.GENERATORS[index + 1].era:
			_check(not str(stage.dimension).is_empty(), "Every major arc concludes with a dimension: " + str(stage.era))
			arc_ends.append(str(stage.id))
	_check(dimension_count == 29, "Twenty-nine explicit dimension milestones exist")
	var confluence_ids: Array[String] = ["infinite_universes", "universe_superposition", "independent_clocks", "beyond_distance", "lawful_dissonance", "confluence_resonance", "uncollapsed_possibility", "confluence_concord", "omniverse"]
	var confluence_index: int = -1
	for index: int in range(db.GENERATORS.size()):
		if db.GENERATORS[index].id == "infinite_universes":
			confluence_index = index
	_check(confluence_index >= 0, "Omnielle has a dedicated confluence arc")
	if confluence_index >= 0:
		for offset: int in range(confluence_ids.size()):
			var stage: Dictionary = db.GENERATORS[confluence_index + offset]
			_check(stage.id == confluence_ids[offset] and stage.era == "Omnielle's Universe Confluence", "Confluence unfolds in its intended sequence")
			_check(is_finite(float(stage.base_cost)) and is_finite(float(stage.base_prod)), "Infinite-universe lore retains finite economy values")
		_check(CreationJourney.CHAPTERS[confluence_index + confluence_ids.size()].id == "first_omniverse", "Omnielle retains her historical chapter ID")
	_check(CosmicProgression.can_discover("infinite_universes", {"omniverse": 1}, db.GENERATORS), "Legacy Omnielle saves retain access to the inserted confluence")
	_check(not CosmicProgression.can_discover("omniverse", {"law_weave": 1}, db.GENERATORS), "New creations must complete the confluence before its dimension")
	_check("dao_dimensions" in arc_ends, "Three Thousand Dao culminates in a dimension family")
	_check(CosmicProgression.can_discover("dao_seed", {"reality": 1}, db.GENERATORS), "Legacy Reality saves retain access to inserted Dao foundations")
	_check(not CosmicProgression.can_discover("dao_dimensions", {"omniverse": 1}, db.GENERATORS), "New worlds cannot skip the Dao construction sequence")
	_check(db.GENERATORS.size() == 106, "One hundred and six discoveries form the dimension journey")
	_check(game.has_reached("hope_return"), "The full chain reaches recognition beyond Reality")
	_check(is_equal_approx(game.get_living_unity_multiplier(), 1.02), "One living recognition set grants two percent")
	game.counts["gaia_biosphere"] = 0
	_check(game.get_living_unity_multiplier() == 1.0, "Divine attributes cannot substitute for nurturing life")
	game.counts["gaia_biosphere"] = 1
	for id: String in ["gaia_biosphere", "all_seeing", "omnipresence", "omnipotence"]:
		game.counts[id] = 1000
	_check(game.get_living_unity_multiplier() == 2.0, "Living Unity has a finite fifty-set cap")
	for id: String in ["gaia_biosphere", "all_seeing", "omnipresence", "omnipotence"]:
		game.counts[id] = 1
	_check(game.has_reached("time_flow") and game.has_reached("space_dimensions"), "Time and space are part of the complete chain")
	_check(game.has_reached("yin_yang") and game.has_reached("elemental_harmony"), "Duality and all five elements precede reality")
	_check(is_equal_approx(game.get_harmony_multiplier(), 1.01 * 1.02), "One balanced pair and elemental set grant their bonuses")
	game.counts["yin"] = 100
	_check(is_equal_approx(game.get_harmony_multiplier(), 1.01 * 1.02), "Unbalanced yin alone does not increase harmony")
	game.counts["element_water"] = 0
	_check(is_equal_approx(game.get_harmony_multiplier(), 1.01), "Incomplete elemental sets do not receive the fivefold bonus")
	game.counts["element_water"] = 1
	for chapter_index: int in range(CreationJourney.CHAPTERS.size()):
		_check(game.claim_creation_chapter(), "Each catalog chapter can be claimed in order")
	_check(not game.claim_creation_chapter(), "The completed journey cannot grant duplicate final rewards")
	var migrated: Dictionary = {"gaia_biosphere": 3}
	_check(CosmicProgression.can_discover("time_flow", migrated, db.GENERATORS), "Legacy worlds keep access to newly inserted earlier layers")
	_check(not CosmicProgression.can_discover("reality", migrated, db.GENERATORS), "Legacy worlds cannot jump to reality")
	game.load_demo_era(0)
	_check(game.chrono_buff_timer == 0.0 and game.completed_chapters.is_empty(), "Switching eras resets temporary demo state")
	game.load_demo_era(3)
	_check(game.goddess_stage == 5, "Multiverse preview shows the final goddess form")
	_check(not game.has_reached("life_memory") and game.get_active_chapter().kind == "life_memory", "Old Reality endpoint now reveals life's memory")
	game.load_demo_era(4)
	_check(game.has_reached("all_seeing") and not game.has_reached("omnipresence"), "All-seeing preview preserves later recognition unlocks")
	game.load_demo_era(5)
	_check(game.get_active_chapter().kind == "angel_awakening", "Living Divinity now opens the dimensional life saga")
	game.load_demo_era(6)
	_check(game.has_reached("dao_dimensions") and not game.has_reached("reality"), "Dao preview ends at the dimension family before Reality")
	_check(ui.gen_rows["dao_dimensions"].prod.text.contains("Dimension built"), "Built dimensions have visible status")
	game.load_demo_era(7)
	_check(game.has_reached("confluence_concord") and not game.has_reached("omniverse"), "Omnielle preview prepares the concluding dimension build")
	_check(game.get_active_chapter().id == "first_omniverse", "Omnielle preview focuses her historical chapter")
	_check(ui.gen_rows["omniverse"].btn.text.begins_with("Build"), "Omnielle's confluence can be built from the preview")
	_check(game.buy_generator("omniverse"), "Preview currency funds Omnielle's dimension")
	_check(game.claim_creation_chapter(), "Building the confluence completes Omnielle's chapter")
	_check(CreationJourney.CHAPTERS.size() == 107, "Opening plus one hundred and six discovery chapters exist")
	var realm_groups: Array = [
		["angel_awakening", "angel_covenant", "angel_realm"],
		["demon_awakening", "demon_covenant", "demon_realm"],
		["sylvan_awakening", "sylvan_covenant", "elf_fairy_realm"],
		["god_awakening", "god_covenant", "gods_realm"],
		["shadow_awakening", "shadow_covenant", "shadow_realm"],
		["realm_envoys", "realm_bridges", "life_realms_concord"]
	]
	for group_index: int in range(realm_groups.size()):
		var group: Array = realm_groups[group_index]
		for offset: int in range(3):
			var stage_index: int = 72 + group_index * 3 + offset
			var stage: Dictionary = db.GENERATORS[stage_index]
			_check(stage.id == group[offset], "Life-realm saga preserves its discovery sequence")
			_check(not str(stage.dimension).is_empty() if offset == 2 else str(stage.dimension).is_empty(), "Each realm arc ends with one construction milestone")
			_check(is_finite(float(stage.base_cost)) and is_finite(float(stage.base_prod)), "Life realm economy is finite")
			_check(CreationJourney.CHAPTERS[stage_index + 1].id == "discover_" + str(stage.id), "Realm chapters use stable discovery IDs")
		game.load_demo_era(8 + group_index)
		_check(game.get_active_chapter().kind == group[2], "Realm preview targets its dimension build")
		_check(not game.has_reached(group[2]), "Realm preview does not pre-build its target")
		_check(ui.gen_rows[group[2]].btn.text.begins_with("Build"), "Realm preview displays a Build action")
		_check(game.buy_generator(group[2]), "Realm preview funds its construction")
		_check(game.claim_creation_chapter(), "Realm construction completes its chapter")
		_check(not game.claim_creation_chapter(), "Realm reward cannot be claimed twice or skip the next discovery")
		_check(not ui.gen_rows[group[2]].btn.text.begins_with("Build"), "Built realm updates its purchase action")
	_check(game.get_active_chapter().kind == "emotion_joy", "Realm concord opens the universal emotions saga")
	_check(CosmicProgression.can_discover("angel_awakening", {"hope_return": 1}, db.GENERATORS), "Existing completed universes unlock the saga opening")
	_check(not CosmicProgression.can_discover("angel_realm", {"hope_return": 1}, db.GENERATORS), "Existing universes cannot skip the angel discoveries")
	_check(not CosmicProgression.can_discover("shadow_realm", {"angel_realm": 1}, db.GENERATORS), "Early realm construction cannot bypass later realms")
	game.load_demo_era(14)
	_check(not game.is_emotion_unlocked("Happy"), "Emotion saga begins with its animations locked")
	for index: int in range(90, 100):
		var stage: Dictionary = db.GENERATORS[index]
		game.add_quanta(float(stage.base_cost))
		_check(game.buy_generator(stage.id), "Emotion saga is sequentially reachable")
		_check(game.claim_creation_chapter(), "Emotion chapter can be claimed")
		for state: String in game.EMOTION_CHAPTERS:
			if game.EMOTION_CHAPTERS[state] == stage.id:
				_check(game.is_emotion_unlocked(state), "Emotion chapter unlocks its expression")
	_check(game.get_active_chapter().kind == "human_earth_life", "Universal Heart opens human life and soul resonance")
	game.load_demo_era(16)
	_check(game.get_active_chapter().kind == "human_earth_life", "Human preview begins with earthly life")
	var soul_ids: Array[String] = ["human_earth_life", "soul_resonance", "angelic_presence", "many_realm_presence", "grounded_connection", "embodied_soul_dimension"]
	for offset: int in range(soul_ids.size()):
		var stage: Dictionary = db.GENERATORS[100 + offset]
		_check(stage.id == soul_ids[offset], "Soul saga retains its intended order")
		_check(CreationJourney.CHAPTERS[101 + offset].id == "discover_" + soul_ids[offset], "Soul chapters use stable discovery IDs")
		_check(is_finite(float(stage.base_cost)) and is_finite(float(stage.base_prod)), "Soul economy remains finite")
		game.add_quanta(float(stage.base_cost))
		_check(game.buy_generator(stage.id), "Soul discoveries are sequentially purchasable")
		_check(game.claim_creation_chapter(), "Soul chapters are claimable")
		_check(not game.claim_creation_chapter(), "Soul chapter rewards cannot be claimed twice")
	_check(game.get_active_chapter().is_empty(), "Embodied Soul concludes the extended journey")
	_check(ui.journey_title.text.contains("Embodied Soul"), "Journey completion names the new finale")
	game.load_demo_era(17)
	_check(game.get_active_chapter().kind == "embodied_soul_dimension", "Final preview targets soul dimension construction")
	_check(not game.has_reached("embodied_soul_dimension"), "Preview does not pre-build the soul dimension")
	_check(game.buy_generator("embodied_soul_dimension") and game.claim_creation_chapter(), "Final preview funds a claimable construction")
	game.load_demo_era(2)
	_check(game.counts["gaia_biosphere"] == 3, "Living Worlds preview contains biospheres")
	ui.visualizer.floating_texts.clear()
	ui.popup_banner.visible = false
	await process_frame
	await process_frame
	_check(ui.size.x >= 1280.0, "Demo lays out at desktop viewport width")
	if "--capture-demo" in OS.get_cmdline_user_args():
		await process_frame
		var image: Image = root.get_texture().get_image()
		var output: String = ProjectSettings.globalize_path("res://demo-preview.png")
		if "--capture-divinity" in OS.get_cmdline_user_args():
			game.load_demo_era(4)
			ui.visualizer.floating_texts.clear()
			await process_frame
			await process_frame
			image = root.get_texture().get_image()
			output = ProjectSettings.globalize_path("res://divinity-preview.png")
		if "--capture-opening" in OS.get_cmdline_user_args():
			game.load_demo_era(0)
			ui.visualizer.floating_texts.clear()
			ui.visualizer.creation_sparks.clear()
			ui.visualizer.pulse_scale = 1.0
			await process_frame
			await process_frame
			image = root.get_texture().get_image()
			output = ProjectSettings.globalize_path("res://demo-opening.png")
		_check(image.save_png(output) == OK, "Demo screenshot saved")
		print("DEMO PREVIEW: %s" % output)
	if DisplayServer.get_name() != "headless":
		await create_timer(1.6).timeout
	ui.queue_free()
	await process_frame
	print("DEMO TESTS: %d checks, %d failures" % [checks, failures])
	quit(1 if failures > 0 else 0)