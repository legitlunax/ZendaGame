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
	game.load_demo_era(2)
	var scene: PackedScene = load("res://scenes/Main.tscn")
	var ui: Control = scene.instantiate()
	root.add_child(ui)
	root.size = Vector2i(390, 844)
	for era: int in range(game.DEMO_ERAS.size()):
		game.load_demo_era(era)
		for frame: int in range(10):
			await process_frame
		_check(ui.page_content.size.x <= 390.0, "Every era fits phone width: %d" % era)
		_check(ui.compact_layout and ui.body_layout.vertical, "Every era uses stacked phone layout")
		_check(ui.visualizer.get_global_rect().position.y < 210.0, "Fairy canvas stays near the top in every preview")
		_check(ui.visualizer.get_global_rect().end.y < 500.0, "Fairy canvas is fully visible without scrolling")
		_check(ui.demo_selector.size.y <= 48.0, "Era controls occupy only a single compact row")
		_check(not ui.lbl_goddess_lore.visible, "Long goddess lore does not crowd the phone header")
		_check(ui.visualizer.wardrobe_menu.visible == (game.get_creation_level() > 0), "Wardrobe respects the initial void")
		if ui.visualizer.wardrobe_menu.visible:
			_check(ui.visualizer.wardrobe_menu.size.y >= 48.0, "Wardrobe has a phone-sized touch target")
			_check(not ui.visualizer.wardrobe_menu.get_global_rect().intersects(ui.visualizer.portrait_button.get_global_rect()), "Wardrobe and camera controls do not overlap")
		var next_chapter: Dictionary = game.get_active_chapter()
		if not next_chapter.is_empty() and not str(next_chapter.get("dimension", "")).is_empty():
			_check(ui.gen_rows[next_chapter.kind].btn.text.begins_with("Build"), "Next dimension has a Build button on phone")
	game.load_demo_era(7)
	for dimensions: Vector2i in [Vector2i(360, 800), Vector2i(390, 844), Vector2i(800, 360), Vector2i(1280, 720)]:
		root.size = dimensions
		for frame: int in range(10):
			await process_frame
		_check(ui.compact_layout == (dimensions.x < 1000), "Responsive breakpoint: %s" % dimensions)
		_check(ui.body_layout.vertical == ui.compact_layout, "Portrait content stacks vertically")
		_check(ui.page_content.size.x <= float(dimensions.x), "No page-wide horizontal overflow: %s" % dimensions)
		if ui.page_content.size.x > float(dimensions.x):
			for control: Node in ui.find_children("*", "Control", true, false):
				if control is Control and control.is_visible_in_tree() and control.get_combined_minimum_size().x > float(dimensions.x - 24):
					print("WIDE CONTROL: ", control.get_path(), " minimum=", control.get_combined_minimum_size())
		_check(ui.page_scroll.horizontal_scroll_mode == ScrollContainer.SCROLL_MODE_DISABLED, "Page scrolls vertically only")
		var button: Button = ui.gen_rows["hope_fluctuation"].btn
		_check(button.size.y >= 48, "Purchase targets have touch-friendly height")
		_check(not ui.card_layouts[0].vertical, "Purchase cards do not stack into oversized towers")
		_check(button.get_global_rect().end.x <= float(dimensions.x) + 1.0, "Purchase buttons fit screen width")
		ui._popup_fitted(ui.help_dialog)
		_check(ui.help_dialog.size.x <= dimensions.x, "Help dialog fits viewport width")
		ui.help_dialog.hide()
		if "--capture-mobile" in OS.get_cmdline_user_args() and dimensions.x == 390:
			await process_frame
			var image: Image = root.get_texture().get_image()
			_check(image.save_png("res://mobile-preview.png") == OK, "Portrait screenshot saved")
			_check(image.save_png("res://omnielle-mobile-preview.png") == OK, "Omnielle phone screenshot saved")
	root.size = Vector2i(390, 844)
	game.load_demo_era(12)
	for frame: int in range(10):
		await process_frame
	_check(ui.journey_title.text.contains("Shadow Realm"), "Phone preview identifies the realm construction objective")
	_check(ui.page_content.size.x <= 390.0, "Life realm saga fits phone width")
	if "--capture-mobile" in OS.get_cmdline_user_args():
		await process_frame
		var realm_image: Image = root.get_texture().get_image()
		_check(realm_image.save_png("res://life-realms-mobile-preview.png") == OK, "Life realm phone screenshot saved")
	game.load_demo_era(15)
	for frame: int in range(10):
		await process_frame
	_check(ui.page_content.size.x <= 390.0, "Universal emotions fit phone width")
	_check(not ui.visualizer.emotion_menu.get_popup().is_item_disabled(9), "Completed saga exposes its final emotion on phone")
	ui.visualizer._on_emotion_selected(8)
	if "--capture-mobile" in OS.get_cmdline_user_args():
		await process_frame
		var emotion_image: Image = root.get_texture().get_image()
		_check(emotion_image.save_png("res://emotions-mobile-preview.png") == OK, "Universal emotions phone screenshot saved")
	ui.visualizer._toggle_portrait()
	for frame: int in range(5):
		await process_frame
	_check(ui.visualizer.fairy_avatar.portrait_view, "Portrait control frames the face and costume")
	_check(ui.visualizer.portrait_button.get_global_rect().end.x < 390.0, "Portrait control fits phone width")
	if "--capture-mobile" in OS.get_cmdline_user_args():
		var portrait_image: Image = root.get_texture().get_image()
		_check(portrait_image.save_png("res://queen-portrait-preview.png") == OK, "Queen detail screenshot saved")
	ui.visualizer._toggle_portrait()
	_check(not ui.visualizer.fairy_avatar.portrait_view, "Full body framing can be restored")
	game.load_demo_era(17)
	for frame: int in range(10):
		await process_frame
	_check(ui.journey_title.text.contains("Embodied Soul"), "Phone identifies the soul dimension objective")
	_check(ui.page_content.size.x <= 390.0, "Human and soul saga fits phone width")
	_check(ui.gen_rows["embodied_soul_dimension"].btn.text.begins_with("Build"), "Soul dimension has a phone construction action")
	if "--capture-mobile" in OS.get_cmdline_user_args():
		var soul_image: Image = root.get_texture().get_image()
		_check(soul_image.save_png("res://soul-realms-mobile-preview.png") == OK, "Soul realm phone screenshot saved")
	ui.visualizer._on_design_selected(4)
	_check(game.equipped_appearance == "shadow_moon", "Phone wardrobe selects an unlocked earlier design")
	if "--capture-mobile" in OS.get_cmdline_user_args():
		await process_frame
		await process_frame
		var wardrobe_image: Image = root.get_texture().get_image()
		_check(wardrobe_image.save_png("res://wardrobe-mobile-preview.png") == OK, "Wardrobe preview saved")
	ui.queue_free()
	await process_frame
	print("RESPONSIVE TESTS: %d checks, %d failures" % [checks, failures])
	quit(1 if failures > 0 else 0)