class_name CosmicVisualizer
extends Control

const Avatar3D = preload("res://scripts/FairyAvatar3D.gd")
var fairy_avatar: FairyAvatar3D
var emotion_menu: MenuButton
var portrait_button: Button
var wardrobe_menu: MenuButton

signal core_tapped

@export var goddess_texture: Texture2D
var stage_textures: Array[Texture2D] = []
var fairy_textures: Array[Texture2D] = []
var nebula_texture: Texture2D
var sparkle_texture: Texture2D
var glow_texture: Texture2D
var ring_texture: Texture2D
var creation_sparks: Array[Dictionary] = []

var time_acc: float = 0.0
var pulse_scale: float = 1.0
var bg_stars: Array = []
var floating_texts: Array = []

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_STOP
	clip_contents = true
	fairy_avatar = Avatar3D.new()
	add_child(fairy_avatar)
	emotion_menu = MenuButton.new()
	emotion_menu.text = "Emotion"
	emotion_menu.position = Vector2(8, 8)
	emotion_menu.custom_minimum_size = Vector2(100, 48)
	add_child(emotion_menu)
	for state: String in Avatar3D.EMOTIONS:
		emotion_menu.get_popup().add_item(state)
	emotion_menu.get_popup().id_pressed.connect(_on_emotion_selected)
	portrait_button = Button.new()
	portrait_button.text = "Portrait"
	portrait_button.position = Vector2(118, 8)
	portrait_button.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_RIGHT)
	portrait_button.offset_left = -108
	portrait_button.offset_top = -56
	portrait_button.offset_right = -8
	portrait_button.offset_bottom = -8
	portrait_button.custom_minimum_size = Vector2(100, 48)
	portrait_button.pressed.connect(_toggle_portrait)
	add_child(portrait_button)
	wardrobe_menu = MenuButton.new()
	wardrobe_menu.text = "Designs"
	wardrobe_menu.custom_minimum_size = Vector2(100, 48)
	add_child(wardrobe_menu)
	wardrobe_menu.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_LEFT)
	wardrobe_menu.offset_left = 8
	wardrobe_menu.offset_top = -56
	wardrobe_menu.offset_right = 108
	wardrobe_menu.offset_bottom = -8
	for design: Dictionary in FairyWardrobe.DESIGNS:
		wardrobe_menu.get_popup().add_radio_check_item(str(design.name))
	wardrobe_menu.get_popup().id_pressed.connect(_on_design_selected)
	_sync_emotion_unlocks()
	nebula_texture = load("res://assets/generated/backgrounds/nebula.svg") as Texture2D
	sparkle_texture = load("res://assets/generated/particles/sparkle.png") as Texture2D
	glow_texture = load("res://assets/generated/particles/glow.png") as Texture2D
	ring_texture = load("res://assets/generated/particles/ring.png") as Texture2D
	for stage: int in range(DB.GODDESS_FORMS.size()):
		stage_textures.append(load("res://assets/generated/portraits/zenda_%d.svg" % stage) as Texture2D)
	for generator: Dictionary in DB.GENERATORS:
		fairy_textures.append(load("res://assets/generated/icons/%s.svg" % generator.id) as Texture2D)
	for i in range(90):
		bg_stars.append({
			"pos": Vector2(randf(), randf()),
			"size": randf_range(1.0, 2.6),
			"speed": randf_range(0.5, 2.5),
			"phase": randf() * TAU
		})

func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var gained := Game.click_cosmic_core()
		play_unlocked_emotion("Happy", 2.0)
		pulse_scale = 1.22
		core_tapped.emit()
		_spawn_creation_sparks(event.position)
		spawn_floating_text(event.position, "+%s" % DB.format_num(gained), Color(1.0, 0.92, 0.55))

func spawn_floating_text(pos: Vector2, text: String, col: Color) -> void:
	floating_texts.append({
		"pos": pos,
		"text": text,
		"color": col,
		"alpha": 1.0,
		"life": 1.1
	})

func _process(delta: float) -> void:
	time_acc += delta
	if fairy_avatar != null:
		_sync_emotion_unlocks()
		fairy_avatar.visible = Game.get_creation_level() > 0
		fairy_avatar.set_palette(DB.GODDESS_FORMS[Game.goddess_stage].aura_color)
		emotion_menu.visible = fairy_avatar.visible
		portrait_button.visible = fairy_avatar.visible
		wardrobe_menu.visible = fairy_avatar.visible
		_sync_wardrobe()
	pulse_scale = lerp(pulse_scale, 1.0, delta * 8.0)
	pulse_scale = clampf(pulse_scale, 1.0, 1.22)
	for index: int in range(creation_sparks.size() - 1, -1, -1):
		creation_sparks[index].life -= delta
		creation_sparks[index].pos += creation_sparks[index].velocity * delta
		if creation_sparks[index].life <= 0.0:
			creation_sparks.remove_at(index)
	
	for i in range(floating_texts.size() - 1, -1, -1):
		floating_texts[i].pos.y -= 48.0 * delta
		floating_texts[i].life -= delta
		floating_texts[i].alpha = clamp(floating_texts[i].life, 0.0, 1.0)
		if floating_texts[i].life <= 0.0:
			floating_texts.remove_at(i)
	
	queue_redraw()

func _draw() -> void:
	var center := size * 0.5
	var form_data: Dictionary = DB.GODDESS_FORMS[Game.goddess_stage]
	var aura_col: Color = form_data.aura_color
	var creation_level: int = Game.get_creation_level()
	
	draw_rect(Rect2(Vector2.ZERO, size), Color("030408"))
	if Game.has_reached("cosmic_dust"):
		draw_texture_rect(nebula_texture, Rect2(Vector2.ZERO, size), false, Color(1, 1, 1, 0.7))
	if creation_level > 0:
		var moon_center: Vector2 = center + Vector2(0, -size.y * 0.15)
		draw_circle(moon_center, size.y * 0.34, Color("19182f"))
		draw_arc(moon_center, size.y * 0.34, 0, TAU, 96, Color(0.79, 0.68, 0.95, 0.55), 1.5, true)
		draw_arc(moon_center, size.y * 0.37, 0.2, PI * 1.7, 96, Color(0.95, 0.8, 0.51, 0.4), 1.0, true)
	if not Game.has_reached("hadron_forge"):
		var hope_size: float = 15.0 + minf(float(creation_level), 3.0) * 13.0 + sin(time_acc * 1.3) * 2.0
		var hope_rect: Vector2 = Vector2.ONE * hope_size * pulse_scale
		draw_texture_rect(glow_texture, Rect2(center - hope_rect * 0.5, hope_rect), false, Color(0.8, 0.85, 1.0, 0.65))
		draw_circle(center, 1.5 + float(creation_level), Color("e4e6ff"))
		_draw_feedback()
		return
	
	for s in bg_stars:
		if not Game.has_reached("protostar_core"):
			break
		var sx: float = s.pos.x * size.x
		var sy: float = s.pos.y * size.y
		var creation_light: float = clampf(log(1.0 + Game.current_run_quanta) / 18.0, 0.0, 1.0)
		var alpha: float = (0.04 + creation_light * 0.65) * (0.5 + 0.5 * sin(time_acc * s.speed + s.phase))
		draw_circle(Vector2(sx, sy), s.size, Color(0.9, 0.95, 1.0, alpha))
	
	for idx in range(DB.GENERATORS.size()):
		# Limit visible companions on phones; the newest fairies represent the current era.
		if idx < maxi(0, creation_level - 3) or fairy_avatar.portrait_view:
			continue
		var gen: Dictionary = DB.GENERATORS[idx]
		var owned: int = Game.counts.get(gen.id, 0)
		if owned <= 0:
			continue
		
		var orbit_slot: int = idx - maxi(0, creation_level - 3)
		var orbit_radius: float = minf(72.0 + float(orbit_slot) * 12.0, minf(size.x, size.y) * 0.5 - 24.0)
		var ring_col: Color = gen.color
		ring_col.a = 0.16
		draw_arc(center, orbit_radius, 0.0, TAU, 64, ring_col, 1.2, true)
		
		var body_count: int = 1
		var angular_speed: float = (0.8 - float(idx) * 0.05) * (1.0 if idx % 2 == 0 else -1.0)
		for b in range(body_count):
			var angle: float = time_acc * angular_speed * 0.08 + float(orbit_slot) * 2.1
			var orb_pos: Vector2 = center + Vector2(cos(angle) * (size.x * 0.5 - 25.0), sin(angle) * orbit_radius * 0.7)
			var fairy_size: Vector2 = Vector2.ONE * 30.0
			draw_texture_rect(fairy_textures[idx], Rect2(orb_pos - fairy_size * 0.5, fairy_size), false)
	
	var wing_count: int = form_data.wings
	for w in range(wing_count):
		var wing_angle: float = (TAU / float(wing_count)) * float(w) + sin(time_acc * 1.5) * 0.12
		var dir := Vector2(cos(wing_angle), sin(wing_angle))
		var tip := center + dir * (58.0 * pulse_scale)
		draw_line(center, tip, Color(aura_col.r, aura_col.g, aura_col.b, 0.45), 4.0, true)
		draw_circle(tip, 4.0, Color(1.0, 0.95, 0.8, 0.8))
	
	draw_arc(center, 46.0 * pulse_scale, time_acc, time_acc + PI * 1.4, 48, aura_col, 2.5, true)
	draw_arc(center, 54.0 * pulse_scale, -time_acc * 0.7, -time_acc * 0.7 + PI * 1.2, 48, Color(1, 0.9, 0.6, 0.6), 1.8, true)
	
	var glow_size: Vector2 = Vector2(200, 200) * pulse_scale
	draw_texture_rect(glow_texture, Rect2(center - glow_size * 0.5, glow_size), false, aura_col)
	_draw_feedback()

func _draw_feedback() -> void:
	var center: Vector2 = size * 0.5
	for spark: Dictionary in creation_sparks:
		var opacity: float = clampf(float(spark.life) / 0.65, 0.0, 1.0)
		var spark_size: Vector2 = Vector2.ONE * (14.0 + 14.0 * opacity)
		draw_texture_rect(sparkle_texture, Rect2(spark.pos - spark_size * 0.5, spark_size), false, Color(1.0, 0.9, 0.6, opacity))
	if pulse_scale > 1.01:
		var ring_size: Vector2 = Vector2.ONE * (160.0 + (1.22 - pulse_scale) * 600.0)
		draw_texture_rect(ring_texture, Rect2(center - ring_size * 0.5, ring_size), false, Color(0.6, 1.0, 0.9, (pulse_scale - 1.0) * 3.0))
	
	var font := ThemeDB.fallback_font
	for ft in floating_texts:
		var col: Color = ft.color
		col.a = ft.alpha
		draw_string(font, ft.pos, ft.text, HORIZONTAL_ALIGNMENT_CENTER, -1, 18, col)

func _spawn_creation_sparks(position_local: Vector2) -> void:
	for index: int in range(8):
		if creation_sparks.size() >= 128:
			creation_sparks.pop_front()
		var angle: float = TAU * float(index) / 8.0 + randf_range(-0.2, 0.2)
		creation_sparks.append({"pos": position_local, "velocity": Vector2.from_angle(angle) * randf_range(35.0, 90.0), "life": 0.65})

func _on_emotion_selected(index: int) -> void:
	if index >= 0 and index < Avatar3D.EMOTIONS.size():
		play_unlocked_emotion(Avatar3D.EMOTIONS[index], 6.0)

func _on_design_selected(index: int) -> void:
	if index >= 0 and index < FairyWardrobe.DESIGNS.size():
		Game.equip_appearance(str(FairyWardrobe.DESIGNS[index].id))
		_sync_wardrobe()

func _sync_wardrobe() -> void:
	for index: int in range(FairyWardrobe.DESIGNS.size()):
		var design: Dictionary = FairyWardrobe.DESIGNS[index]
		var unlocked: bool = Game.is_appearance_unlocked(str(design.id))
		wardrobe_menu.get_popup().set_item_disabled(index, not unlocked)
		wardrobe_menu.get_popup().set_item_checked(index, Game.equipped_appearance == design.id)
		wardrobe_menu.get_popup().set_item_text(index, str(design.name) if unlocked else str(design.name) + " (locked)")
		wardrobe_menu.get_popup().set_item_tooltip(index, "Wear " + str(design.name) if unlocked else "Claim " + str(design.requirement))
	if fairy_avatar.appearance_id != Game.equipped_appearance:
		fairy_avatar.apply_appearance(FairyWardrobe.make_appearance(Game.equipped_appearance))

func _toggle_portrait() -> void:
	fairy_avatar.set_portrait_view(not fairy_avatar.portrait_view)
	portrait_button.text = "Full body" if fairy_avatar.portrait_view else "Portrait"

func play_unlocked_emotion(state: String, duration: float = 4.0) -> bool:
	_sync_emotion_unlocks()
	return fairy_avatar.play_emotion(state, duration)

func _sync_emotion_unlocks() -> void:
	fairy_avatar.unlocked_emotions.clear()
	for index: int in range(Avatar3D.EMOTIONS.size()):
		var state: String = Avatar3D.EMOTIONS[index]
		var unlocked: bool = Game.is_emotion_unlocked(state)
		if unlocked:
			fairy_avatar.unlocked_emotions.append(state)
		emotion_menu.get_popup().set_item_disabled(index, not unlocked)
		emotion_menu.get_popup().set_item_text(index, state if unlocked else state + " (locked)")
		emotion_menu.get_popup().set_item_tooltip(index, "Claim the %s chapter in Universal Emotions." % str(Game.EMOTION_CHAPTERS.get(state, "")) if not unlocked else "Play " + state)
	if fairy_avatar.emotion not in fairy_avatar.unlocked_emotions:
		fairy_avatar.play_emotion("Idle")
	emotion_menu.tooltip_text = "%d/%d expressions unlocked. Claim Universal Emotions chapters to remember more." % [fairy_avatar.unlocked_emotions.size(), Avatar3D.EMOTIONS.size()]
