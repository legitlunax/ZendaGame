class_name CosmicProgression
extends RefCounted

# Stable generator IDs preserve old saves; order defines the discovery chain.
static var TITLES: Array[String] = _titles()

static func _titles() -> Array[String]:
	var result: Array[String] = ["Only Hope"]
	for generator: Dictionary in StageCatalog.generators():
		result.append(str(generator.name))
	return result

static func level(counts: Dictionary, generators: Array) -> int:
	var result: int = 0
	for index: int in range(generators.size()):
		if int(counts.get(generators[index].id, 0)) > 0:
			result = index + 1
	return result

static func can_discover(id: String, counts: Dictionary, generators: Array) -> bool:
	for index: int in range(generators.size()):
		if generators[index].id == id:
			if index == 0 or int(counts.get(id, 0)) > 0:
				return true
			# Legacy universes retain their existing progress, including earlier layers.
			return level(counts, generators) >= index
	return false