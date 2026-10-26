class_name FairyWardrobe
extends RefCounted

# Append stable IDs; never replace an earned design when adding another.
const DESIGNS: Array[Dictionary] = [
	{"id": "ivory_queen", "name": "Ivory Queen", "chapter": "", "requirement": "Always available", "outfit": "eee3fa", "accent": "58327c", "wings": "ddcfee", "scale": 1.0},
	{"id": "earth_bloom", "name": "Earth Bloom", "chapter": "first_life", "requirement": "Living Biosphere", "outfit": "d6ead7", "accent": "397c69", "wings": "b7ddc5", "scale": 0.8},
	{"id": "angel_dawn", "name": "Angel Dawn", "chapter": "discover_angel_realm", "requirement": "Angel Realm", "outfit": "fff0d3", "accent": "b88849", "wings": "fff0d9", "scale": 1.1},
	{"id": "sylvan_bloom", "name": "Sylvan Bloom", "chapter": "discover_elf_fairy_realm", "requirement": "Elves and Fairies Realm", "outfit": "d1e9df", "accent": "936ca7", "wings": "b6e6d8", "scale": 0.9},
	{"id": "shadow_moon", "name": "Shadow Moon", "chapter": "discover_shadow_realm", "requirement": "Shadow Realm", "outfit": "aaa4cb", "accent": "413654", "wings": "8b90bb", "scale": 1.0},
	{"id": "soul_concord", "name": "Soul Concord", "chapter": "discover_embodied_soul_dimension", "requirement": "Embodied Soul Dimension", "outfit": "ead5e6", "accent": "7676b5", "wings": "d3ddef", "scale": 1.15}
]

static func find_design(id: String) -> Dictionary:
	for design: Dictionary in DESIGNS:
		if design.id == id:
			return design
	return {}

static func make_appearance(id: String) -> FairyAppearance:
	var design: Dictionary = find_design(id)
	if design.is_empty():
		return null
	var style: FairyAppearance = FairyAppearance.new()
	style.appearance_id = id
	style.outfit_color = Color(str(design.outfit))
	style.accent_color = Color(str(design.accent))
	style.wing_color = Color(str(design.wings))
	style.wing_scale = float(design.scale)
	style.cosmetics_visible = id != "earth_bloom"
	return style