class_name StageCatalog
extends RefCounted

const DATA_PATH: String = "res://data/creation_stages.json"

static func rows() -> Array:
	var file: FileAccess = FileAccess.open(DATA_PATH, FileAccess.READ)
	if file == null:
		push_error("Creation stage catalog is missing")
		return []
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	if parsed is Array:
		return parsed
	push_error("Creation stage catalog must contain an array")
	return []

static func generators() -> Array[Dictionary]:
	var result: Array[Dictionary] = []
	for row: Array in rows():
		var index: int = result.size()
		var cost: float = 5.0 * pow(3.2, float(index))
		result.append({
			"id": str(row[0]), "name": str(row[1]), "fairy": str(row[2]),
			"era": str(row[3]), "scale": str(row[3]), "desc": str(row[4]),
			"color": Color(str(row[5])), "motif": str(row[6]),
			"dimension": str(row[7]) if row.size() > 7 else "",
			"base_cost": cost, "cost_mult": 1.15 + minf(float(index) * 0.003, 0.15),
			"base_prod": maxf(0.5, cost * 0.025)
		})
	return result

static func fairy_names() -> Dictionary[String, String]:
	var result: Dictionary[String, String] = {}
	for row: Array in rows():
		result[str(row[0])] = str(row[2])
	result.merge({"relic_chronos": "Chronelle", "relic_seraph_grace": "Seraphine",
		"relic_wormhole_loom": "Filoria", "genesis_burst": "Aurielle", "chrono_harmony": "Tempora"})
	return result