class_name FairyAvatar3D
extends SubViewportContainer

const EMOTIONS: Array[String] = ["Idle", "Happy", "Dance", "Shy", "Surprised", "Sorrow", "Courage", "Anger", "Compassion", "Serenity"]
var unlocked_emotions: Array[String] = ["Idle"]
const CHARACTER_AGE: int = 25
const CHARACTER_HEIGHT: float = 3.5
const HEAD_HEIGHT: float = 0.56
const SILK_SHADER: Shader = preload("res://shaders/CelestialSilk.gdshader")
const DETAILS = preload("res://scripts/QueenArtDetails.gd")
const APPEARANCE = preload("res://scripts/FairyAppearance.gd")
var outfit_root: Node3D
var cosmetics_root: Node3D
var appearance_id: String = "ivory_queen"
var portrait_view: bool = false
var camera: Camera3D
var viewport_3d: SubViewport
var rig: Node3D
var head: Node3D
var arms: Array[Node3D] = []
var legs: Array[Node3D] = []
var wings: Array[Node3D] = []
var eyes: Array[Node3D] = []
var mouth: MeshInstance3D
var dress_material: ShaderMaterial
var hair_material: ShaderMaterial
var accent_material: ShaderMaterial
var forearms: Array[Node3D] = []
var calves: Array[Node3D] = []
var hair_locks: Array[Node3D] = []
var gown: Node3D
var emotion: String = "Idle"
var emotion_remaining: float = 0.0
var clock: float = 0.0

func _ready() -> void:
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	stretch = true
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	viewport_3d = SubViewport.new()
	viewport_3d.size = Vector2i(512, 320)
	viewport_3d.own_world_3d = true
	viewport_3d.transparent_bg = true
	viewport_3d.gui_disable_input = true
	viewport_3d.msaa_3d = Viewport.MSAA_4X
	add_child(viewport_3d)
	var environment_node: WorldEnvironment = WorldEnvironment.new()
	var environment: Environment = Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = Color(0, 0, 0, 0)
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color("c8bade")
	environment.ambient_light_energy = 0.3
	environment_node.environment = environment
	viewport_3d.add_child(environment_node)
	var light: DirectionalLight3D = DirectionalLight3D.new()
	light.rotation_degrees = Vector3(-25, -25, 0)
	light.light_color = Color("fff0de")
	light.light_energy = 0.7
	viewport_3d.add_child(light)
	var rim_light: DirectionalLight3D = DirectionalLight3D.new()
	rim_light.rotation_degrees = Vector3(-15, 145, 0)
	rim_light.light_color = Color("bea3ff")
	rim_light.light_energy = 0.25
	viewport_3d.add_child(rim_light)
	camera = Camera3D.new()
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.size = 4.2
	camera.position = Vector3(0, 1.8, 7)
	camera.current = true
	viewport_3d.add_child(camera)
	rig = Node3D.new()
	viewport_3d.add_child(rig)
	_build_fairy()

func set_portrait_view(enabled: bool) -> void:
	portrait_view = enabled
	camera.size = 1.85 if enabled else 4.2
	camera.position = Vector3(0, 2.78, 7) if enabled else Vector3(0, 1.8, 7)

func _silk(color: Color, sheen: float) -> ShaderMaterial:
	var material: ShaderMaterial = ShaderMaterial.new()
	material.shader = SILK_SHADER
	material.set_shader_parameter("base_color", color)
	material.set_shader_parameter("sheen", sheen)
	return material

func _material(color: Color) -> StandardMaterial3D:
	var material: StandardMaterial3D = StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.55
	material.diffuse_mode = BaseMaterial3D.DIFFUSE_BURLEY
	return material

func _ball(parent: Node3D, at: Vector3, dimensions: Vector3, material: Material) -> MeshInstance3D:
	var mesh_node: MeshInstance3D = MeshInstance3D.new()
	var sphere: SphereMesh = SphereMesh.new()
	sphere.radius = 0.5
	sphere.height = 1.0
	sphere.radial_segments = 32
	sphere.rings = 20
	mesh_node.mesh = sphere
	mesh_node.material_override = material
	mesh_node.position = at
	mesh_node.scale = dimensions
	parent.add_child(mesh_node)
	return mesh_node

func _joint(parent: Node3D, at: Vector3) -> Node3D:
	var pivot: Node3D = Node3D.new()
	pivot.position = at
	parent.add_child(pivot)
	return pivot

func _build_fairy() -> void:
	# A clearly adult queen: approximately six heads tall, not a rescaled chibi.
	rig.name = "AdultFairyQueen"
	rig.set_meta("character_age", CHARACTER_AGE)
	outfit_root = _joint(rig, Vector3.ZERO)
	outfit_root.name = "OutfitSlot"
	var skin: StandardMaterial3D = _material(Color("f8d9ce"))
	var white: StandardMaterial3D = _material(Color("faf0ff"))
	var dark: StandardMaterial3D = _material(Color("362c4c"))
	var iris: StandardMaterial3D = _material(Color("7773d9"))
	var lips: StandardMaterial3D = _material(Color("b46b83"))
	var gold: StandardMaterial3D = _material(Color("dcb675"))
	gold.metallic = 0.65
	gold.roughness = 0.22
	gold.emission_enabled = true
	gold.emission = Color("b38c38") * 0.12
	dress_material = _silk(Color("eee3fa"), 0.30)
	hair_material = _silk(Color("7962ab"), 0.70)
	accent_material = _silk(Color("58327c"), 0.60)
	# Continuous fitted bodice, with an opaque underdress above the slit.
	_profile(outfit_root, [Vector3(0.30, 1.65, 0.22), Vector3(0.33, 1.82, 0.23), Vector3(0.22, 2.05, 0.17), Vector3(0.28, 2.30, 0.22), Vector3(0.34, 2.46, 0.22), Vector3(0.31, 2.52, 0.19)], dress_material)
	_ball(rig, Vector3(0, 2.52, 0), Vector3(0.61, 0.33, 0.34), skin)
	_ball(rig, Vector3(0, 2.73, 0), Vector3(0.17, 0.30, 0.17), skin)
	_profile(outfit_root, [Vector3(0.31, 1.42, 0.24), Vector3(0.34, 1.70, 0.25), Vector3(0.29, 1.89, 0.22)], accent_material)
	gown = _joint(outfit_root, Vector3.ZERO)
	gown.name = "SlitGown"
	_profile(gown, [Vector3(0.68, 0.16, 0.40), Vector3(0.58, 0.55, 0.37), Vector3(0.44, 1.08, 0.30), Vector3(0.35, 1.44, 0.25), Vector3(0.33, 1.76, 0.24)], dress_material, true)
	# Layered petal overskirt and long violet ribbons break up the cone silhouette.
	for index: int in range(9):
		var angle: float = TAU * float(index) / 9.0
		var outward: Vector3 = Vector3(sin(angle), 0, cos(angle))
		var panel: Node3D = _joint(gown, outward * 0.23 + Vector3(0, 1.83, 0))
		panel.rotation.y = angle
		DETAILS.ribbon(panel, [Vector3.ZERO, Vector3(0, -0.18, 0.10), Vector3(0, -0.38, 0.16), Vector3(0, -0.48, 0.12)], [0.08, 0.13, 0.13, 0.01], accent_material)
	for side: float in [-1.0, 1.0]:
		DETAILS.ribbon(gown, [Vector3(side * 0.23, 1.94, 0.26), Vector3(side * 0.40, 1.57, 0.34), Vector3(side * 0.55, 1.14, 0.38), Vector3(side * 0.42, 0.70, 0.40)], [0.07, 0.10, 0.08, 0.01], accent_material)
		for index: int in range(7):
			var y: float = 1.74 - float(index) * 0.21
			var radius: float = 0.33 + (1.76 - y) * 0.22
			_ball(gown, Vector3(side * sin(0.5) * radius, y, cos(0.5) * (0.24 + (1.76 - y) * 0.10)), Vector3(0.025, 0.035, 0.025), gold)
	# Off-shoulder ivory neckline and gold waist ornament.
	for side: float in [-1.0, 1.0]:
		_rod(rig, Vector3(0, 2.42, 0.23), Vector3(side * 0.37, 2.53, 0.10), 0.065, white)
		_rod(rig, Vector3(0, 2.02, 0.18), Vector3(side * 0.23, 2.10, 0.12), 0.025, gold)
		_rod(rig, Vector3(0, 2.66, 0.10), Vector3(side * 0.16, 2.61, 0.13), 0.014, gold)
	_ball(rig, Vector3(0, 2.59, 0.17), Vector3(0.075, 0.12, 0.035), iris)
	for side: float in [-1.0, 1.0]:
		var bow: MeshInstance3D = _ball(rig, Vector3(side * 0.12, 2.36, 0.25), Vector3(0.25, 0.19, 0.07), accent_material)
		bow.rotation.z = side * 0.3
		_rod(rig, Vector3(side * 0.26, 2.40, 0.22), Vector3(side * 0.12, 2.14, 0.18), 0.017, gold)
	_ball(rig, Vector3(0, 2.36, 0.31), Vector3(0.09, 0.12, 0.06), gold)
	_ball(rig, Vector3(0, 2.36, 0.345), Vector3(0.055, 0.075, 0.025), iris)
	_ball(rig, Vector3(0, 2.04, 0.20), Vector3(0.09, 0.12, 0.045), gold)
	for side: float in [-1.0, 1.0]:
		var arm: Node3D = _joint(rig, Vector3(side * 0.37, 2.52, 0))
		arms.append(arm)
		_ball(arm, Vector3(0, -0.20, 0), Vector3(0.15, 0.46, 0.15), skin)
		_ball(arm, Vector3(0, -0.09, 0), Vector3(0.21, 0.15, 0.19), white)
		var forearm: Node3D = _joint(arm, Vector3(0, -0.42, 0))
		forearms.append(forearm)
		_ball(forearm, Vector3(0, -0.18, 0), Vector3(0.115, 0.39, 0.12), skin)
		_ball(forearm, Vector3(0, -0.38, 0), Vector3(0.12, 0.19, 0.07), skin)
		_ball(forearm, Vector3(0, -0.30, 0), Vector3(0.13, 0.05, 0.13), gold)
		var leg: Node3D = _joint(rig, Vector3(side * 0.17, 1.68, 0))
		legs.append(leg)
		_ball(leg, Vector3(0, -0.36, 0), Vector3(0.25, 0.77, 0.24), skin)
		var calf: Node3D = _joint(leg, Vector3(0, -0.74, 0))
		calves.append(calf)
		_ball(calf, Vector3(0, -0.34, 0), Vector3(0.17, 0.73, 0.18), skin)
		_ball(calf, Vector3(0, -0.69, 0.07), Vector3(0.17, 0.12, 0.32), accent_material)
		_ball(calf, Vector3(0, -0.58, 0), Vector3(0.17, 0.035, 0.19), gold)
		var wing: Node3D = _joint(rig, Vector3(side * 0.16, 2.40, -0.21))
		wings.append(wing)
		var wing_material: StandardMaterial3D = _material(Color("ddcfee"))
		wing_material.set_meta("wing_surface", true)
		wing_material.emission_enabled = true
		wing_material.emission = Color("bba6de") * 0.15
		_wing(wing, side, 1.0, wing_material, gold)
		var lower_wing: Node3D = _joint(wing, Vector3(0, -0.12, -0.015))
		lower_wing.rotation.z = -side * 1.15
		_wing(lower_wing, side, 0.62, wing_material, gold)
	head = _joint(rig, Vector3(0, 3.04, 0))
	head.name = "AdultFace"
	cosmetics_root = _joint(head, Vector3.ZERO)
	cosmetics_root.name = "HeadCosmeticsSlot"
	_ball(head, Vector3(0, 0.045, -0.065), Vector3(0.53, 0.62, 0.43), hair_material)
	_profile(head, [Vector3(0.025, -0.27, 0.06), Vector3(0.115, -0.22, 0.13), Vector3(0.195, -0.13, 0.185), Vector3(0.23, 0.01, 0.20), Vector3(0.215, 0.16, 0.175), Vector3(0.12, 0.26, 0.10), Vector3(0.005, 0.28, 0.005)], skin)
	_ball(head, Vector3(0, -0.07, 0.245), Vector3(0.045, 0.085, 0.065), skin)
	for side: float in [-1.0, 1.0]:
		var lock: Node3D = _joint(head, Vector3(side * 0.22, 0.13, -0.08))
		hair_locks.append(lock)
		for layer: int in range(4):
			var spread: float = float(layer) * 0.045
			DETAILS.strand(lock, [Vector3(side * spread, 0, -spread), Vector3(side * (0.06 + spread), -0.24, -spread), Vector3(side * (0.10 + spread), -0.53, -0.06 - spread), Vector3(side * (0.17 + spread), -0.84, -0.03 - spread), Vector3(side * (0.09 + spread), -1.15, 0.04 - spread)], [0.06, 0.075, 0.065, 0.045, 0.002], hair_material)
		var eye: Node3D = _joint(head, Vector3(side * 0.103, -0.025, 0.208))
		_ball(eye, Vector3.ZERO, Vector3(0.15, 0.105, 0.032), white)
		_ball(eye, Vector3(0, 0, 0.021), Vector3(0.077, 0.094, 0.021), iris)
		_ball(eye, Vector3(0, -0.021, 0.031), Vector3(0.060, 0.033, 0.013), _material(Color("b2b0fa")))
		eyes.append(eye)
		_ball(eye, Vector3(0, 0, 0.034), Vector3(0.025, 0.060, 0.012), dark)
		_ball(eye, Vector3(-0.011, 0.020, 0.043), Vector3(0.018, 0.022, 0.008), white)
		_rod(eye, Vector3(-0.068, 0.024, 0.008), Vector3(0.068, 0.033, 0.008), 0.012, dark)
		for lash: int in range(3):
			var x: float = side * (0.04 + float(lash) * 0.014)
			_rod(eye, Vector3(x, 0.033, 0.02), Vector3(x + side * 0.018, 0.057 + float(lash) * 0.004, 0.015), 0.008, dark)
		_rod(head, Vector3(side * 0.045, 0.070, 0.211), Vector3(side * 0.16, 0.085, 0.18), 0.012, dark)
		_ball(cosmetics_root, Vector3(side * 0.235, -0.12, 0.025), Vector3(0.035, 0.11, 0.035), gold)
		for index: int in range(3):
			var x: float = side * (0.045 + float(index) * 0.06)
			DETAILS.strand(head, [Vector3(x * 0.5, 0.26, 0.11), Vector3(x, 0.19, 0.17), Vector3(x + side * 0.025, 0.10, 0.20), Vector3(x + side * 0.035, 0.045 + float(index) * 0.025, 0.19)], [0.055, 0.06, 0.045, 0.002], hair_material)
		_rod(cosmetics_root, Vector3(side * 0.23, 0.22, 0.11), Vector3(side * 0.13, 0.32, 0.13), 0.023, gold)
		_rod(cosmetics_root, Vector3(side * 0.13, 0.32, 0.13), Vector3(0, 0.28, 0.18), 0.023, gold)
	_ball(cosmetics_root, Vector3(0, 0.28, 0.20), Vector3(0.055, 0.10, 0.03), iris)
	mouth = _ball(head, Vector3(0, -0.16, 0.226), Vector3(0.082, 0.023, 0.016), lips)

func _rod(parent: Node3D, start: Vector3, end: Vector3, width: float, material: Material) -> void:
	var rod: MeshInstance3D = _ball(parent, (start + end) * 0.5, Vector3(width, start.distance_to(end), width), material)
	rod.quaternion = Quaternion(Vector3.UP, (end - start).normalized())

func _profile(parent: Node3D, rings: Array[Vector3], material: Material, slit: bool = false) -> void:
	rings = DETAILS.curved_points(rings, 4)
	var surface: SurfaceTool = SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	for row: int in range(rings.size() - 1):
		for segment: int in range(48):
			var points: Array[Vector3] = []
			for offset: Vector2i in [Vector2i(0, 0), Vector2i(1, 0), Vector2i(1, 1), Vector2i(0, 1)]:
				var ring: Vector3 = rings[row + offset.y]
				var opening: float = clampf((1.65 - ring.y) * 0.35, 0.0, 0.50) if slit else 0.0
				var angle: float = 0.5 + opening + (TAU - opening * 2.0) * float(segment + offset.x) / 48.0
				var pleat: float = 1.0 + (0.025 * cos(angle * 12.0) if slit else 0.0)
				points.append(Vector3(sin(angle) * ring.x * pleat, ring.y, cos(angle) * ring.z * pleat))
			for vertex: int in [0, 2, 1, 0, 3, 2]:
				surface.add_vertex(points[vertex])
	surface.generate_normals()
	var mesh_node: MeshInstance3D = MeshInstance3D.new()
	mesh_node.mesh = surface.commit()
	mesh_node.material_override = material
	parent.add_child(mesh_node)

func _wing(parent: Node3D, side: float, length: float, material: StandardMaterial3D, trim: StandardMaterial3D) -> void:
	var surface: SurfaceTool = SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	var outline: Array[Vector3] = []
	for index: int in range(25):
		var t: float = float(index) / 24.0
		var angle: float = TAU * t
		var along: float = (1.0 - cos(angle)) * 0.5
		outline.append(Vector3(side * (along * 1.03 + sin(angle) * 0.24), along * 0.84 - sin(angle) * 0.27, -along * 0.10 - sin(PI * along) * 0.16) * length)
	for index: int in range(24):
		surface.add_vertex(Vector3(side * 0.46, 0.38, -0.04) * length)
		surface.add_vertex(outline[index])
		surface.add_vertex(outline[index + 1])
		_rod(parent, outline[index], outline[index + 1], 0.014, trim)
	for index: int in [5, 10, 14, 19]:
		_rod(parent, Vector3.ZERO, outline[index], 0.009, trim)
	surface.generate_normals()
	var mesh_node: MeshInstance3D = MeshInstance3D.new()
	mesh_node.mesh = surface.commit()
	material.cull_mode = BaseMaterial3D.CULL_DISABLED
	mesh_node.material_override = material
	parent.add_child(mesh_node)

func set_palette(color: Color) -> void:
	# Progression colors tint only the shader edge light, not skin/hair/costume identity.
	dress_material.set_shader_parameter("edge_color", Color("e6d4ff").lerp(color, 0.12))

func apply_appearance(style: FairyAppearance) -> bool:
	if style == null or rig == null or not is_finite(style.wing_scale) or style.wing_scale < 0.5 or style.wing_scale > 1.5:
		return false
	appearance_id = style.appearance_id
	dress_material.set_shader_parameter("base_color", style.outfit_color)
	accent_material.set_shader_parameter("base_color", style.accent_color)
	hair_material.set_shader_parameter("base_color", style.hair_color)
	for wing: Node3D in wings:
		wing.visible = style.wings_visible
		wing.scale = Vector3.ONE * style.wing_scale
		for node: Node in wing.find_children("*", "MeshInstance3D", true, false):
			var mesh_node: MeshInstance3D = node as MeshInstance3D
			var material: StandardMaterial3D = mesh_node.material_override as StandardMaterial3D
			if material != null and material.get_meta("wing_surface", false):
				material.albedo_color = style.wing_color
	cosmetics_root.visible = style.cosmetics_visible
	return true

func play_emotion(state: String, duration: float = 4.0) -> bool:
	if state not in EMOTIONS or state not in unlocked_emotions:
		return false
	emotion = state
	emotion_remaining = maxf(0.0, duration)
	return true

func _process(delta: float) -> void:
	if rig == null or not visible:
		return
	clock += delta
	emotion_remaining = maxf(0.0, emotion_remaining - delta)
	if emotion_remaining <= 0.0:
		emotion = "Idle"
	var dance: bool = emotion == "Dance"
	var happy: bool = emotion == "Happy"
	var shy: bool = emotion == "Shy"
	var sorrow: bool = emotion == "Sorrow"
	var compassion: bool = emotion == "Compassion"
	var courage: bool = emotion == "Courage"
	var anger: bool = emotion == "Anger"
	var serenity: bool = emotion == "Serenity"
	rig.position.y = sin(clock * (3.0 if happy or dance else 1.5)) * (0.035 if happy or dance else 0.018)
	rig.rotation.y = sin(clock * 1.7) * 0.35 if dance else 0.12
	rig.rotation.z = sin(clock * 2.5) * 0.035 if dance else 0.0
	head.rotation.z = 0.12 if shy else sin(clock * 1.7) * 0.025
	head.rotation.x = 0.12 if shy else 0.0
	if sorrow:
		head.rotation.x = 0.24
		head.rotation.z = -0.07
	elif courage or anger:
		head.rotation.x = -0.06
	elif compassion or serenity:
		head.rotation.z = 0.06
	for index: int in range(2):
		var side: float = -1.0 if index == 0 else 1.0
		arms[index].rotation.z = side * (0.95 if happy else 0.18)
		if dance:
			arms[index].rotation.z = side * (0.7 + sin(clock * 2.5 + float(index)) * 0.35)
		arms[index].rotation.x = -0.45 if shy else 0.0
		forearms[index].rotation.x = -0.8 if shy or happy else -0.18 - (sin(clock * 2.5 + float(index)) * 0.25 if dance else 0.0)
		legs[index].rotation.x = sin(clock * 2.5 + float(index) * PI) * 0.09 if dance else 0.0
		calves[index].rotation.x = maxf(0.0, sin(clock * 2.5 + float(index) * PI)) * 0.12 if dance else 0.0
		wings[index].rotation.y = side * sin(clock * 3.0) * 0.18
		hair_locks[index].rotation.z = sin(clock * 1.7 + float(index)) * 0.035
		if sorrow:
			arms[index].rotation.z = side * 0.08
			forearms[index].rotation.x = -0.35
		elif compassion:
			arms[index].rotation.z = side * (0.6 + sin(clock * 1.5) * 0.06)
			arms[index].rotation.x = -0.4
			forearms[index].rotation.x = -0.55
		elif courage or anger:
			arms[index].rotation.z = side * (0.35 if courage else 0.5)
			forearms[index].rotation.x = -1.1 if courage else -0.65
			rig.rotation.y = 0.12 + sin(clock * 2.0) * (0.025 if courage else 0.05)
		elif serenity:
			arms[index].rotation.z = side * 0.4
			forearms[index].rotation.x = -0.3
	gown.rotation.z = sin(clock * 2.5 - 0.3) * 0.018 if dance else 0.0
	var blink: float = 0.12 if fmod(clock, 4.0) > 3.84 else 1.0
	for eye: Node3D in eyes:
		eye.scale.y = blink * (0.18 if serenity else (0.55 if happy or shy or sorrow else (0.7 if anger else 1.0)))
		eye.rotation.z = (0.15 if sorrow else (-0.18 if anger else 0.0)) * (-1.0 if eye == eyes[0] else 1.0)
	mouth.scale.y = 0.065 if emotion == "Surprised" else (0.035 if happy or dance else 0.023)