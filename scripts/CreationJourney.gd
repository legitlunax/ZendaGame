class_name CreationJourney
extends RefCounted

# Stable historical chapter IDs retain already claimed rewards in older saves.
static var CHAPTERS: Array[Dictionary] = _build_chapters()

static func _build_chapters() -> Array[Dictionary]:
	var result: Array[Dictionary] = [
		{"id": "first_light", "title": "01 / Nothing but hope", "goal": "Gather 10 Hope. Tap the void; discover Elpis for 5 Hope.", "target": 10.0, "kind": "quanta", "reward": 15.0, "lore": "There was nothing. Yet you hoped something could be."}
	]
	var historical_ids: Dictionary = {
		"hope_fluctuation": "first_possibility", "quark_filament": "first_thread",
		"hadron_forge": "first_atoms", "cosmic_dust": "first_nebula",
		"protostar_core": "first_star", "gaia_biosphere": "first_life",
		"black_hole_anchor": "first_galaxy", "white_hole_fountain": "first_web",
		"wormhole_nexus": "first_universe", "parallel_universe": "first_daughter",
		"multiverse_loom": "first_multiverse", "omniverse": "first_omniverse", "reality": "first_reality"
	}
	for generator: Dictionary in StageCatalog.generators():
		var is_dimension: bool = not str(generator.dimension).is_empty()
		result.append({"id": str(historical_ids.get(generator.id, "discover_" + str(generator.id))),
			"title": "%02d / %s" % [result.size() + 1, "Build " + str(generator.dimension) if is_dimension else str(generator.fairy) + " awakens"],
			"goal": "%s %s / %s. %s" % ["Build" if is_dimension else "Discover", generator.fairy, generator.name, generator.era],
			"dimension": generator.dimension,
			"target": 1.0, "kind": generator.id,
			"reward": float(generator.base_cost) * 0.25, "lore": generator.desc})
	return result

static func get_progress(chapter: Dictionary, run_quanta: float, generator_counts: Dictionary) -> float:
	if chapter.kind == "quanta":
		return minf(run_quanta, float(chapter.target))
	return minf(float(generator_counts.get(chapter.kind, 0)), float(chapter.target))