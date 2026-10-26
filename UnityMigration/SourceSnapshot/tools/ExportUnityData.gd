extends SceneTree

# Run with --headless --path <project> --script res://tools/ExportUnityData.gd -- --demo
var output: String = "res://UnityMigration/Data/"
var checks: int = 0

func _initialize() -> void:
	call_deferred("_run")

func _portable(value: Variant) -> Variant:
	if value is Color:
		return {"r": value.r, "g": value.g, "b": value.b, "a": value.a}
	if value is Vector3:
		return [value.x, value.y, value.z]
	if value is Dictionary:
		var result: Dictionary = {}
		for key: Variant in value:
			result[str(key)] = _portable(value[key])
		return result
	if value is Array:
		var result: Array = []
		for entry: Variant in value:
			result.append(_portable(entry))
		return result
	return value

func _write(name: String, value: Variant) -> void:
	var file: FileAccess = FileAccess.open(output + name, FileAccess.WRITE)
	assert(file != null, "Cannot write migration data: " + name)
	file.store_string(JSON.stringify(_portable(value), "  ", true, true) + "\n")
	file.close()
	checks += 1

func _state(game: Node) -> Dictionary:
	return {"equipped_appearance": game.equipped_appearance,
		"completed_chapters": game.completed_chapters.duplicate(),
		"quanta": game.quanta, "lifetime_quanta": game.lifetime_quanta,
		"current_run_quanta": game.current_run_quanta, "genesis_seeds": game.genesis_seeds,
		"goddess_stage": game.goddess_stage, "counts": game.counts.duplicate(),
		"relic_levels": game.relic_levels.duplicate(), "timestamp": 0.0}

func _hierarchy(node: Node3D) -> Dictionary:
	var children: Array = []
	for child: Node in node.get_children():
		if child is Node3D:
			children.append(_hierarchy(child))
	return {"name": str(node.name), "type": node.get_class(), "position": node.position,
		"rotation_radians": node.rotation, "scale": node.scale, "children": children}

func _run() -> void:
	var game: Node = root.get_node("Game")
	var db: Node = root.get_node("DB")
	assert(game.demo_mode and not game.persistence_enabled, "Exporter requires --demo to protect player saves")
	game.set_process(false)
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(output))
	var dimensions: Array = []
	for index: int in range(db.GENERATORS.size()):
		var generator: Dictionary = db.GENERATORS[index]
		if not str(generator.dimension).is_empty():
			dimensions.append({"generator_id": generator.id, "chapter_id": CreationJourney.CHAPTERS[index + 1].id, "name": generator.dimension, "order": index})
	var emotions: Array = [{"id": "Idle", "chapter_id": "", "always_available": true}]
	for state: String in game.EMOTION_CHAPTERS:
		emotions.append({"id": state, "chapter_id": "discover_" + str(game.EMOTION_CHAPTERS[state]), "always_available": false})
	_write("catalog.json", {"schema_version": 1, "source_engine": Engine.get_version_info(),
		"generators": db.GENERATORS, "chapters": CreationJourney.CHAPTERS,
		"dimensions": dimensions, "goddess_forms": db.GODDESS_FORMS,
		"relics": db.RELICS, "miracles": db.MIRACLES, "number_suffixes": db.SUFFIXES,
		"fairy_names": db.FAIRY_NAMES, "demo_presets": game.DEMO_ERAS,
		"emotions": emotions, "wardrobe": FairyWardrobe.DESIGNS})
	var previews: Array = []
	for index: int in range(game.DEMO_ERAS.size()):
		game.load_demo_era(index)
		var unlocked: Array[String] = []
		for design: Dictionary in FairyWardrobe.DESIGNS:
			if game.is_appearance_unlocked(str(design.id)):
				unlocked.append(str(design.id))
		previews.append({"index": index, "name": game.DEMO_ERAS[index].name,
			"state": _state(game), "creation_level": game.get_creation_level(),
			"resource_name": game.get_resource_name(), "active_chapter": game.get_active_chapter(),
			"qps": game.get_total_qps(), "global_multiplier": game.get_global_multiplier(),
			"pending_seeds": game.get_pending_genesis_seeds(), "unlocked_designs": unlocked})
	_write("demo_snapshots.json", {"timestamp_note": "Zero is a deterministic fixture sentinel, not a real save timestamp. Skip offline accrual when testing these snapshots.", "snapshots": previews})
	game.load_demo_era(0)
	_write("save_template_godot.json", _state(game))
	var fixtures: Array = []
	for index: int in [0, 1, 30, 73, 99, 105]:
		for owned: int in [0, 1, 24, 25, 50]:
			for mode: int in [1, 10, 100, -1]:
				game.load_demo_era(0)
				var generator: Dictionary = db.GENERATORS[index]
				game.counts[generator.id] = owned
				game.quanta = float(generator.base_cost) * 1000.0
				game.buy_multiplier_mode = mode
				fixtures.append({"generator_id": generator.id, "owned": owned, "balance": game.quanta,
					"buy_mode": mode, "discount": game.get_cost_discount(),
					"bulk_result": game.get_bulk_buy_info(generator),
					"generator_qps": game.get_generator_qps(generator), "global_multiplier": game.get_global_multiplier()})
	var formats: Array = []
	for number: float in [0.0, 1.0, 1.06, 9.9, 10.5, 999.0, 1000.0, 1.0e12, 1.0e63, 1.0e70]:
		formats.append({"input": number, "expected": db.format_num(number)})
	_write("parity_fixtures.json", {"relative_tolerance": 0.000001, "bulk_cases": fixtures, "number_format_cases": formats})
	var avatar: FairyAvatar3D = FairyAvatar3D.new()
	root.add_child(avatar)
	await process_frame
	avatar.set_process(false)
	avatar.unlocked_emotions.assign(FairyAvatar3D.EMOTIONS)
	_write("character_hierarchy.json", {"coordinate_system": "Godot right-handed, Y up; source front is +Z. Convert coordinates and triangle winding when importing manually.", "hierarchy": _hierarchy(avatar.rig)})
	var poses: Array = []
	for emotion: String in FairyAvatar3D.EMOTIONS:
		for sample_time: float in [0.0, 0.25, 0.5, 1.0, 2.0, 3.85]:
			avatar.clock = sample_time
			avatar.play_emotion(emotion, 10.0)
			avatar._process(0.0)
			poses.append({"emotion": emotion, "time_seconds": sample_time, "hierarchy": _hierarchy(avatar.rig)})
	_write("emotion_pose_samples.json", {"note": "Procedural pose reference samples, not skinned animation clips. Source animation code remains authoritative.", "samples": poses})
	avatar.clock = 0.0
	avatar.play_emotion("Idle", 10.0)
	avatar._process(0.0)
	var gltf: GLTFDocument = GLTFDocument.new()
	var gltf_state: GLTFState = GLTFState.new()
	var gltf_error: Error = gltf.append_from_scene(avatar.rig, gltf_state)
	if gltf_error == OK:
		gltf_error = gltf.write_to_filesystem(gltf_state, ProjectSettings.globalize_path(output + "fairy_prototype.glb"))
	_write("export_report.json", {"generators": db.GENERATORS.size(), "chapters": CreationJourney.CHAPTERS.size(),
		"dimensions": dimensions.size(), "demo_presets": previews.size(), "wardrobe_designs": FairyWardrobe.DESIGNS.size(),
		"emotions": emotions.size(), "bulk_fixture_cases": fixtures.size(), "pose_samples": poses.size(),
		"glb_export_error": int(gltf_error), "glb_warning": "Static procedural geometry only. ShaderMaterials do not reproduce the Godot silk shader in Unity. No Skeleton3D, skin weights, or authored clips exist.",
		"player_save_copied": false, "credentials_copied": false})
	avatar.queue_free()
	await process_frame
	print("UNITY DATA EXPORT: %d JSON files; GLB error %d" % [checks, gltf_error])
	quit(0 if gltf_error == OK else 1)