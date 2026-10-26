extends Node

var FAIRY_NAMES: Dictionary[String, String] = StageCatalog.fairy_names()

const SUFFIXES := [
	"", "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc",
	"UDc", "DDc", "TDc", "QaDc", "QiDc", "SxDc", "SpDc", "OcDc", "NoDc", "Vg"
]

var GENERATORS: Array[Dictionary] = StageCatalog.generators()

const GODDESS_FORMS := [
	{
		"stage": 0,
		"title": "Zenda, Child of the Silent Void",
		"req_quanta": 0.0,
		"mult_bonus": 1.0,
		"aura_color": Color(0.5, 0.7, 1.0),
		"wings": 2,
		"lore": "A fragile spark awakening in absolute nothingness."
	},
	{
		"stage": 1,
		"title": "Zenda, Maiden of Quantum Light",
		"req_quanta": 1.0e11,
		"mult_bonus": 1.5,
		"aura_color": Color(0.3, 0.95, 0.8),
		"wings": 2,
		"lore": "Her fingertips hum with subatomic warmth as the first atoms bind."
	},
	{
		"stage": 2,
		"title": "Zenda, Weaver of Golden Stars",
		"req_quanta": 2.0e14,
		"mult_bonus": 2.5,
		"aura_color": Color(1.0, 0.8, 0.3),
		"wings": 4,
		"lore": "Four solar wings unfurl, igniting nebulae with every heartbeat."
	},
	{
		"stage": 3,
		"title": "Zenda, Warden of Event Horizons",
		"req_quanta": 2.0e19,
		"mult_bonus": 5.0,
		"aura_color": Color(0.8, 0.4, 1.0),
		"wings": 6,
		"lore": "She balances Black and White Holes in the palms of her hands."
	},
	{
		"stage": 4,
		"title": "Zenda, Seraph of the Multiverse",
		"req_quanta": 7.0e21,
		"mult_bonus": 12.0,
		"aura_color": Color(1.0, 0.45, 0.75),
		"wings": 8,
		"lore": "Countless parallel universes shimmer inside the feathers of her wings."
	},
	{
		"stage": 5,
		"title": "Zenda, Goddess of Absolute Reality",
		"req_quanta": 2.0e23,
		"mult_bonus": 35.0,
		"aura_color": Color(1.0, 0.95, 0.7),
		"wings": 12,
		"lore": "Omnipresent and radiant—she is the living canvas of existence itself."
	}
]

const MIRACLES := [
	{
		"id": "genesis_burst",
		"name": "Genesis Chorus",
		"desc": "Instantly grants 30 seconds worth of your current Quanta/sec.",
		"cooldown": 25.0,
		"duration": 0.0
	},
	{
		"id": "chrono_harmony",
		"name": "Chrono-Weave Overdrive",
		"desc": "Warps spacetime to multiply all Creation output by 3x for 20s.",
		"cooldown": 60.0,
		"duration": 20.0
	}
]

const RELICS := [
	{
		"id": "relic_chronos",
		"name": "Hourglass of Eternal Dawn",
		"desc": "+50% base production speed per level across all realities.",
		"base_seed_cost": 1,
		"effect_per_lvl": 0.50
	},
	{
		"id": "relic_seraph_grace",
		"name": "Crown of the First Seraph",
		"desc": "Clicking the Cosmic Core grants +2% of your Quanta/sec per level.",
		"base_seed_cost": 2,
		"effect_per_lvl": 0.02
	},
	{
		"id": "relic_wormhole_loom",
		"name": "Singularity Loom",
		"desc": "Reduces all Creation generator costs by 5% per level (Max 10).",
		"base_seed_cost": 3,
		"effect_per_lvl": 0.05,
		"max_lvl": 10
	}
]

func format_num(val: float) -> String:
	if val < 1000.0:
		if val < 10.0 and fmod(val, 1.0) > 0.05:
			return "%.1f" % val
		return "%d" % floor(val)
	
	var exp_idx: int = int(floor(log(val) / log(1000.0)))
	if exp_idx < SUFFIXES.size():
		var scaled: float = val / pow(1000.0, exp_idx)
		return "%.2f %s" % [scaled, SUFFIXES[exp_idx]]
	else:
		var sci_exp: int = int(floor(log(val) / log(10.0)))
		var mantissa: float = val / pow(10.0, sci_exp)
		return "%.2fe%d" % [mantissa, sci_exp]