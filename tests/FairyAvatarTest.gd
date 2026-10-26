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
	var game: Node = root.get_node("Game")
	game.set_process(false)
	game.load_demo_era(15)
	var scene: PackedScene = load("res://scenes/Main.tscn")
	var ui: Control = scene.instantiate()
	root.add_child(ui)
	await process_frame
	var visual: Control = ui.get("visualizer") as Control
	var avatar: Node = visual.get("fairy_avatar") as Node
	avatar.set_process(false)
	_check(avatar.viewport_3d.own_world_3d, "Character has an isolated 3D world")
	_check(avatar.viewport_3d.transparent_bg, "Character background is transparent")
	_check(avatar.viewport_3d.msaa_3d == Viewport.MSAA_4X, "Character uses antialiased rendering")
	_check(avatar.dress_material is ShaderMaterial and avatar.hair_material is ShaderMaterial, "Costume and hair use stylized silk shaders")
	var costume_color: Color = avatar.dress_material.get_shader_parameter("base_color")
	avatar.set_palette(Color.YELLOW)
	_check(avatar.dress_material.get_shader_parameter("base_color") == costume_color, "Progression preserves the ivory costume palette")
	avatar.set_portrait_view(true)
	_check(avatar.camera.size < 2.0 and avatar.portrait_view, "Portrait camera exposes face details")
	avatar.set_portrait_view(false)
	_check(is_equal_approx(avatar.camera.size, 4.2), "Full body camera is restored")
	var style: FairyAppearance = FairyAppearance.new()
	style.appearance_id = "test_variant"
	style.outfit_color = Color("a3cabb")
	style.wings_visible = false
	style.cosmetics_visible = false
	style.wing_scale = 0.8
	_check(avatar.apply_appearance(style), "Appearance resource can be applied")
	_check(avatar.appearance_id == "test_variant", "Appearance identity is independent of story state")
	_check(avatar.dress_material.get_shader_parameter("base_color") == style.outfit_color, "Outfit color follows appearance")
	_check(not avatar.wings[0].visible and not avatar.cosmetics_root.visible, "Wings and head cosmetics are independent slots")
	_check(is_equal_approx(avatar.wings[0].scale.x, 0.8), "Wing scale follows appearance")
	_check(avatar.outfit_root.get_child_count() > 0, "Outfit has a dedicated attachment root")
	style.wing_scale = NAN
	_check(not avatar.apply_appearance(style), "Nonfinite appearance scale is rejected")
	_check(not avatar.apply_appearance(null), "Null appearance is rejected")
	_check(avatar.apply_appearance(FairyAppearance.new()), "Default appearance can be restored")
	_check(avatar.wings[0].visible and avatar.cosmetics_root.visible, "Default restores wings and cosmetics")
	_check(avatar.play_emotion("Happy"), "Changing appearance preserves emotion permissions")
	_check(avatar.mouse_filter == Control.MOUSE_FILTER_IGNORE, "Character does not swallow core taps")
	_check(avatar.arms.size() == 2 and avatar.legs.size() == 2 and avatar.wings.size() == 2, "Articulated limbs and wings are built")
	_check(avatar.rig.get_meta("character_age") == 25, "Queen is explicitly an adult character")
	_check(avatar.CHARACTER_HEIGHT / avatar.HEAD_HEIGHT >= 6.0, "Adult proportions replace oversized chibi head")
	_check(avatar.forearms.size() == 2 and avatar.calves.size() == 2, "Elbow and knee joints are present")
	_check(avatar.hair_locks.size() == 2 and avatar.gown.name == "SlitGown", "Queen has animated long hair and a flowing gown")
	_check(visual.emotion_menu.get_popup().item_count == 10, "All ten emotion controls are available")
	for state: String in ["Idle", "Happy", "Dance", "Shy", "Surprised", "Sorrow", "Courage", "Anger", "Compassion", "Serenity"]:
		_check(avatar.play_emotion(state, 1.0), "Accept emotion: " + state)
		avatar._process(0.1)
		_check(avatar.emotion == state, "State is playing: " + state)
		_check(avatar.rig.transform.is_finite(), "Finite rig transform: " + state)
		avatar._process(1.0)
		_check(avatar.emotion == "Idle", "State returns to idle: " + state)
	_check(not avatar.play_emotion("Unknown"), "Unknown emotion rejected")
	avatar.play_emotion("Dance", 5.0)
	avatar._process(0.2)
	var arm_pose: Vector3 = avatar.arms[0].rotation
	avatar._process(0.2)
	_check(not avatar.arms[0].rotation.is_equal_approx(arm_pose), "Dance animates the arms over time")
	avatar.clock = 3.85
	avatar.play_emotion("Idle")
	avatar._process(0.01)
	_check(avatar.eyes[0].scale.y < 0.2, "Full eye assembly blinks")
	avatar.clock = 0.0
	avatar._process(0.01)
	_check(avatar.eyes[0].scale.y == 1.0, "Eyes reopen after blinking")
	var tap: InputEventMouseButton = InputEventMouseButton.new()
	tap.button_index = MOUSE_BUTTON_LEFT
	tap.pressed = true
	tap.position = visual.size * 0.5
	visual._gui_input(tap)
	_check(avatar.emotion == "Happy", "Core tapping triggers happiness")
	ui.call("_on_chapter_completed", {"lore": "Test celebration", "reward": 1.0})
	_check(avatar.emotion == "Dance", "Chapter completion triggers dancing")
	visual._on_emotion_selected(3)
	_check(avatar.emotion == "Shy", "Menu selection changes expression")
	game.load_demo_era(0)
	visual._process(0.01)
	_check(not avatar.visible and not visual.emotion_menu.visible, "Void hides fairy and emotion controls")
	_check(avatar.emotion == "Idle", "Switching to an early preview clears a locked expression")
	_check(not avatar.play_emotion("Happy"), "Avatar rejects locked expressions")
	_check(visual.emotion_menu.get_popup().is_item_disabled(1), "Locked expression menu entry is disabled")
	_check(not visual.play_unlocked_emotion("Dance"), "Reaction wrapper rejects locked dances")
	game.load_demo_era(2)
	visual._process(0.01)
	_check(avatar.visible and visual.emotion_menu.visible, "Discovery restores fairy and controls")
	visual._gui_input(tap)
	_check(avatar.emotion == "Idle", "Early taps cannot bypass the joy chapter")
	ui.call("_on_chapter_completed", {"lore": "Early chapter", "reward": 1.0})
	_check(avatar.emotion == "Idle", "Early chapter rewards cannot bypass the dance chapter")
	game.load_demo_era(14)
	_check(game.get_active_chapter().kind == "emotion_joy", "Emotion preview begins at joy")
	_check(game.buy_generator("emotion_joy"), "Joy discovery can be purchased")
	_check(not game.is_emotion_unlocked("Happy"), "Purchasing alone does not unlock the animation")
	_check(game.claim_creation_chapter(), "Claiming joy remembers the emotion")
	_check(game.is_emotion_unlocked("Happy") and avatar.emotion == "Happy", "New emotion plays immediately on chapter claim")
	_check(not visual.emotion_menu.get_popup().is_item_disabled(1), "Claim immediately enables the expression control")
	_check(not game.is_emotion_unlocked("Dance"), "Joy does not unlock later emotions")
	_check(game.perform_big_bloom(), "Advanced emotion preview can bloom")
	_check(game.is_emotion_unlocked("Happy"), "Remembered emotions survive a Big Bloom")
	_check(not game.is_emotion_unlocked("Unknown"), "Unknown emotion is never unlocked")
	game.load_demo_era(15)
	var previous_pose: Vector3 = Vector3.ZERO
	for state: String in ["Sorrow", "Courage", "Anger", "Compassion", "Serenity"]:
		visual.play_unlocked_emotion(state)
		avatar._process(0.01)
		var pose: Vector3 = Vector3(avatar.head.rotation.x, avatar.arms[0].rotation.z, avatar.forearms[0].rotation.x)
		_check(not pose.is_equal_approx(previous_pose), "New emotion has a distinct pose: " + state)
		previous_pose = pose
	game.load_demo_era(0)
	_check(game.is_appearance_unlocked("ivory_queen"), "Original design is always retained")
	_check(not game.is_appearance_unlocked("unknown"), "Unknown designs cannot unlock")
	_check(not game.equip_appearance("angel_dawn"), "Locked designs cannot be equipped")
	game.counts["angel_realm"] = 1
	_check(not game.is_appearance_unlocked("angel_dawn"), "Purchasing alone does not unlock a design")
	game.load_demo_era(2)
	game.completed_chapters.erase("first_life")
	_check(not game.is_appearance_unlocked("earth_bloom"), "Biosphere preview has not claimed its new design yet")
	_check(game.claim_creation_chapter(), "Biosphere chapter can grant its design")
	_check(game.is_appearance_unlocked("earth_bloom"), "Claiming Biosphere unlocks Earth Bloom")
	_check(game.equipped_appearance == "ivory_queen", "New unlock does not force an outfit change")
	_check(game.equip_appearance("earth_bloom"), "Earlier unlocked design can be equipped")
	visual._sync_wardrobe()
	_check(avatar.appearance_id == "earth_bloom", "Wardrobe selection updates the visible avatar")
	game.restore_equipped_appearance({"equipped_appearance": "earth_bloom"})
	_check(game.equipped_appearance == "earth_bloom", "Saved unlocked selection is restored")
	game.restore_equipped_appearance({"equipped_appearance": "angel_dawn"})
	_check(game.equipped_appearance == "ivory_queen", "Saved locked selection falls back safely")
	game.restore_equipped_appearance({})
	_check(game.equipped_appearance == "ivory_queen", "Older saves default to original design")
	game.load_demo_era(17)
	_check(not game.is_appearance_unlocked("soul_concord"), "Final design waits for its claimed chapter")
	_check(game.buy_generator("embodied_soul_dimension") and game.claim_creation_chapter(), "Final design milestone can be claimed")
	var design_ids: Array[String] = []
	for design: Dictionary in FairyWardrobe.DESIGNS:
		_check(str(design.id) not in design_ids, "Design IDs remain unique")
		design_ids.append(str(design.id))
		_check(game.is_appearance_unlocked(str(design.id)), "Final chapter retains every earlier design")
		_check(game.equip_appearance(str(design.id)), "Every retained design remains selectable")
		visual._sync_wardrobe()
		_check(avatar.appearance_id == design.id, "Every design has an applicable appearance")
	_check(game.perform_big_bloom(), "Wardrobe test can perform Big Bloom")
	_check(game.equipped_appearance == "soul_concord", "Bloom preserves the equipped design")
	for id: String in design_ids:
		_check(game.is_appearance_unlocked(id), "Bloom preserves collected design: " + id)
	_check(game.equip_appearance("ivory_queen"), "Original design remains wearable after all unlocks and Bloom")
	ui.queue_free()
	await process_frame
	print("FAIRY 3D TESTS: %d checks, %d failures" % [checks, failures])
	quit(1 if failures > 0 else 0)