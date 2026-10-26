extends Node

signal state_updated
signal goddess_evolved(new_stage_data: Dictionary)
signal miracle_triggered(miracle_id: String, text_popup: String)
signal offline_progress_calculated(seconds_away: float, quanta_earned: float)
signal chapter_completed(chapter: Dictionary)

const SAVE_PATH := "user://genesis_save_v1.json"
const EMOTION_CHAPTERS: Dictionary = {
	"Happy": "emotion_joy", "Sorrow": "emotion_sorrow", "Surprised": "emotion_wonder",
	"Shy": "emotion_tenderness", "Courage": "emotion_courage", "Anger": "emotion_anger",
	"Compassion": "emotion_compassion", "Serenity": "emotion_serenity", "Dance": "emotion_celebration"
}

var quanta: float = 0.0
var lifetime_quanta: float = 0.0
var current_run_quanta: float = 0.0
var genesis_seeds: int = 0

var counts: Dictionary = {}
var relic_levels: Dictionary = {}

var goddess_stage: int = 0
var buy_multiplier_mode: int = 1

var miracle_cooldowns: Dictionary = {}
var chrono_buff_timer: float = 0.0
var autosave_timer: float = 0.0
var completed_chapters: Array[String] = []
var equipped_appearance: String = "ivory_queen"
var persistence_enabled: bool = true
var demo_mode: bool = false
var demo_era: int = 0

const DEMO_ERAS: Array[Dictionary] = [
	{"name": "Only Hope", "through": ""},
	{"name": "Five Elements", "through": "elemental_harmony"},
	{"name": "Living Worlds", "through": "gaia_biosphere"},
	{"name": "Reality", "through": "reality"},
	{"name": "All-Seeing", "through": "all_seeing"},
	{"name": "Living Divinity", "through": "hope_return"},
	{"name": "Dao Dimensions", "through": "dao_dimensions"},
	{"name": "Omnielle", "through": "confluence_concord", "fund_next": true},
	{"name": "Angel Realm", "through": "angel_covenant", "fund_next": true},
	{"name": "Demon Realm", "through": "demon_covenant", "fund_next": true},
	{"name": "Elves & Fairies", "through": "sylvan_covenant", "fund_next": true},
	{"name": "Gods Realm", "through": "god_covenant", "fund_next": true},
	{"name": "Shadow Realm", "through": "shadow_covenant", "fund_next": true},
	{"name": "Realm Concord", "through": "realm_bridges", "fund_next": true},
	{"name": "Universal Emotions", "through": "life_realms_concord", "fund_next": true},
	{"name": "Universal Heart", "through": "universal_heart"},
	{"name": "Human & Soul", "through": "universal_heart", "fund_next": true},
	{"name": "Embodied Soul", "through": "grounded_connection", "fund_next": true},
]

func _ready() -> void:
	_init_defaults()
	demo_mode = "--demo" in OS.get_cmdline_user_args()
	persistence_enabled = not "--genesis-test" in OS.get_cmdline_user_args() and not demo_mode
	if persistence_enabled:
		load_game()
	if demo_mode:
		load_demo_era(0)

func load_demo_era(era: int) -> bool:
	if not demo_mode or era < 0 or era >= DEMO_ERAS.size():
		return false
	demo_era = era
	var preset: Dictionary = DEMO_ERAS[era]
	_init_defaults()
	quanta = 0.0
	current_run_quanta = 0.0
	lifetime_quanta = 0.0
	genesis_seeds = 0
	goddess_stage = 0
	chrono_buff_timer = 0.0
	autosave_timer = 0.0
	buy_multiplier_mode = 1
	completed_chapters.clear()
	equipped_appearance = "ivory_queen"
	var target_index: int = -1
	for index: int in range(DB.GENERATORS.size()):
		if DB.GENERATORS[index].id == preset.through:
			target_index = index
	for index: int in range(target_index + 1):
		var generator: Dictionary = DB.GENERATORS[index]
		counts[generator.id] = 3 if index == target_index else 10
		current_run_quanta += float(generator.base_cost) * 15.0
	if target_index >= 0:
		quanta = float(DB.GENERATORS[target_index].base_cost) * 3.0
		if bool(preset.get("fund_next", false)) and target_index + 1 < DB.GENERATORS.size():
			quanta = float(DB.GENERATORS[target_index + 1].base_cost) * 1.25
	lifetime_quanta = current_run_quanta
	# Previews remember earlier chapters so their active objective matches their era.
	for chapter: Dictionary in CreationJourney.CHAPTERS:
		if (chapter.kind == "quanta" and current_run_quanta >= float(chapter.target)) or int(counts.get(chapter.kind, 0)) > 0:
			completed_chapters.append(chapter.id)
	# Select the form silently: changing previews should not play six evolution cues.
	for index: int in range(DB.GODDESS_FORMS.size()):
		if current_run_quanta >= float(DB.GODDESS_FORMS[index].req_quanta):
			goddess_stage = index
	state_updated.emit()
	return true

func is_emotion_unlocked(state: String) -> bool:
	if state == "Idle":
		return true
	if not EMOTION_CHAPTERS.has(state):
		return false
	return "discover_" + str(EMOTION_CHAPTERS[state]) in completed_chapters

func is_appearance_unlocked(id: String) -> bool:
	var design: Dictionary = FairyWardrobe.find_design(id)
	return not design.is_empty() and (str(design.chapter).is_empty() or str(design.chapter) in completed_chapters)

func equip_appearance(id: String) -> bool:
	if not is_appearance_unlocked(id):
		return false
	equipped_appearance = id
	save_game()
	state_updated.emit()
	return true

func restore_equipped_appearance(data: Dictionary) -> void:
	var saved_id: String = str(data.get("equipped_appearance", "ivory_queen"))
	equipped_appearance = saved_id if is_appearance_unlocked(saved_id) else "ivory_queen"

func _init_defaults() -> void:
	for gen in DB.GENERATORS:
		counts[gen.id] = 0
	for relic in DB.RELICS:
		relic_levels[relic.id] = 0
	for m in DB.MIRACLES:
		miracle_cooldowns[m.id] = 0.0

func _process(delta: float) -> void:
	if chrono_buff_timer > 0.0:
		chrono_buff_timer = max(0.0, chrono_buff_timer - delta)
	
	for m_id in miracle_cooldowns.keys():
		if miracle_cooldowns[m_id] > 0.0:
			miracle_cooldowns[m_id] = max(0.0, miracle_cooldowns[m_id] - delta)
	
	var qps := get_total_qps()
	if qps > 0.0:
		add_quanta(qps * delta)
	
	autosave_timer += delta
	if autosave_timer >= 15.0:
		autosave_timer = 0.0
		save_game()
	
	state_updated.emit()

func add_quanta(amount: float) -> void:
	if not is_finite(amount) or amount <= 0.0:
		return
	quanta += amount
	current_run_quanta += amount
	lifetime_quanta += amount
	_check_goddess_evolution()

func click_cosmic_core() -> float:
	var base_click: float = 1.0
	var seraph_lvl: int = relic_levels.get("relic_seraph_grace", 0)
	var qps_bonus: float = get_total_qps() * (0.01 + seraph_lvl * 0.02)
	var total_click: float = base_click * get_global_multiplier() + qps_bonus
	add_quanta(total_click)
	return total_click

func get_global_multiplier() -> float:
	var mult: float = DB.GODDESS_FORMS[goddess_stage].mult_bonus
	mult *= (1.0 + float(genesis_seeds) * 0.25)
	var chronos_lvl: int = relic_levels.get("relic_chronos", 0)
	mult *= (1.0 + float(chronos_lvl) * 0.50)
	mult *= get_harmony_multiplier()
	mult *= get_living_unity_multiplier()
	if chrono_buff_timer > 0.0:
		mult *= 3.0
	return mult

func get_generator_qps(gen_data: Dictionary) -> float:
	var count: int = counts.get(gen_data.id, 0)
	if count <= 0:
		return 0.0
	var milestone_pow: int = int(count / 25)
	var milestone_mult: float = pow(2.0, milestone_pow)
	return gen_data.base_prod * float(count) * milestone_mult * get_global_multiplier()

func get_total_qps() -> float:
	var total: float = 0.0
	for gen in DB.GENERATORS:
		total += get_generator_qps(gen)
	return total

func get_cost_discount() -> float:
	var loom_lvl: int = relic_levels.get("relic_wormhole_loom", 0)
	return clamp(1.0 - float(loom_lvl) * 0.05, 0.5, 1.0)

func get_bulk_buy_info(gen_data: Dictionary) -> Dictionary:
	var owned: int = counts.get(gen_data.id, 0)
	var base: float = gen_data.base_cost * get_cost_discount()
	var r: float = gen_data.cost_mult
	var first_cost: float = base * pow(r, owned)
	
	var count_to_buy: int = buy_multiplier_mode
	if buy_multiplier_mode == -1:
		if quanta < first_cost:
			count_to_buy = 1
		else:
			var max_n: float = floor(log(1.0 + (quanta * (r - 1.0)) / first_cost) / log(r))
			count_to_buy = max(1, int(max_n))
	
	var total_cost: float = first_cost * (pow(r, count_to_buy) - 1.0) / (r - 1.0)
	if count_to_buy == 1:
		total_cost = first_cost
	return {
		"amount": count_to_buy,
		"cost": total_cost,
		"can_afford": quanta >= total_cost
	}

func buy_generator(gen_id: String) -> bool:
	if not is_generator_discovered(gen_id):
		return false
	for gen in DB.GENERATORS:
		if gen.id == gen_id:
			var info := get_bulk_buy_info(gen)
			if info.can_afford:
				quanta -= info.cost
				counts[gen_id] += info.amount
				_check_goddess_evolution()
				state_updated.emit()
				return true
	return false

func _check_goddess_evolution() -> void:
	while goddess_stage < DB.GODDESS_FORMS.size() - 1:
		var next_form: Dictionary = DB.GODDESS_FORMS[goddess_stage + 1]
		if current_run_quanta >= next_form.req_quanta:
			goddess_stage += 1
			goddess_evolved.emit(next_form)
		else:
			break

func activate_miracle(miracle_id: String) -> void:
	if not has_reached("hadron_forge"):
		return
	if miracle_cooldowns.get(miracle_id, 0.0) > 0.0:
		return
	for m in DB.MIRACLES:
		if m.id == miracle_id:
			miracle_cooldowns[miracle_id] = m.cooldown
			if miracle_id == "genesis_burst":
				var burst: float = max(50.0, get_total_qps() * 30.0)
				add_quanta(burst)
				miracle_triggered.emit(miracle_id, "+%s Quanta!" % DB.format_num(burst))
			elif miracle_id == "chrono_harmony":
				chrono_buff_timer = m.duration
				miracle_triggered.emit(miracle_id, "3x Cosmic Speed (20s)!")

func get_pending_genesis_seeds() -> int:
	if not has_reached("wormhole_nexus"):
		return 0
	if current_run_quanta < 1.0e6:
		return 0
	var total_possible := int(floor(pow(current_run_quanta / 1.0e6, 0.35)))
	return max(1, total_possible)

func perform_big_bloom() -> bool:
	var earned := get_pending_genesis_seeds()
	if earned <= 0:
		return false
	genesis_seeds += earned
	quanta = 0.0
	current_run_quanta = 0.0
	goddess_stage = 0
	for gen in DB.GENERATORS:
		counts[gen.id] = 0
	chrono_buff_timer = 0.0
	for miracle_id: String in miracle_cooldowns:
		miracle_cooldowns[miracle_id] = 0.0
	save_game()
	state_updated.emit()
	return true

func buy_relic(relic_id: String) -> bool:
	for r in DB.RELICS:
		if r.id == relic_id:
			var lvl: int = relic_levels.get(relic_id, 0)
			if r.has("max_lvl") and lvl >= r.max_lvl:
				return false
			var cost: int = r.base_seed_cost * (lvl + 1)
			if genesis_seeds >= cost:
				genesis_seeds -= cost
				relic_levels[relic_id] = lvl + 1
				save_game()
				state_updated.emit()
				return true
	return false

func get_current_scale_title() -> String:
	var highest_scale: String = "No space. No time. Only hope."
	for gen in DB.GENERATORS:
		if counts.get(gen.id, 0) > 0:
			highest_scale = gen.scale
	return highest_scale

func get_creation_level() -> int:
	return CosmicProgression.level(counts, DB.GENERATORS)

func is_generator_discovered(id: String) -> bool:
	return CosmicProgression.can_discover(id, counts, DB.GENERATORS)

func get_resource_name() -> String:
	return "Creation Potential" if has_reached("quark_filament") else "Hope"

func has_reached(id: String) -> bool:
	var level: int = get_creation_level()
	for index: int in range(DB.GENERATORS.size()):
		if DB.GENERATORS[index].id == id:
			return level > index
	return false

func get_harmony_multiplier() -> float:
	# Balanced pairs and a complete elemental cycle matter more than one dominant fairy.
	var duality: int = mini(int(counts.get("yin", 0)), int(counts.get("yang", 0)))
	var elements: int = 100
	for id: String in ["element_wood", "element_fire", "element_earth", "element_metal", "element_water"]:
		elements = mini(elements, int(counts.get(id, 0)))
	return (1.0 + float(mini(duality, 100)) * 0.01) * (1.0 + float(elements) * 0.02)

func get_living_unity_multiplier() -> float:
	# Recognition strengthens the living beginning; it does not consume it.
	var complete_sets: int = 50
	for id: String in ["gaia_biosphere", "all_seeing", "omnipresence", "omnipotence"]:
		complete_sets = mini(complete_sets, maxi(0, int(counts.get(id, 0))))
	return 1.0 + float(complete_sets) * 0.02

func save_game() -> void:
	if not persistence_enabled:
		return
	var data := {
		"equipped_appearance": equipped_appearance,
		"completed_chapters": completed_chapters,
		"quanta": quanta,
		"lifetime_quanta": lifetime_quanta,
		"current_run_quanta": current_run_quanta,
		"genesis_seeds": genesis_seeds,
		"goddess_stage": goddess_stage,
		"counts": counts,
		"relic_levels": relic_levels,
		"timestamp": Time.get_unix_time_from_system()
	}
	var file := FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	if file:
		file.store_string(JSON.stringify(data))

func load_game() -> void:
	if not FileAccess.file_exists(SAVE_PATH):
		return
	var file := FileAccess.open(SAVE_PATH, FileAccess.READ)
	if not file:
		return
	var parsed = JSON.parse_string(file.get_as_text())
	if typeof(parsed) != TYPE_DICTIONARY:
		return
	
	quanta = float(parsed.get("quanta", 0.0))
	lifetime_quanta = float(parsed.get("lifetime_quanta", 0.0))
	current_run_quanta = float(parsed.get("current_run_quanta", 0.0))
	genesis_seeds = int(parsed.get("genesis_seeds", 0))
	goddess_stage = clampi(int(parsed.get("goddess_stage", 0)), 0, DB.GODDESS_FORMS.size() - 1)
	completed_chapters.clear()
	var saved_chapters: Array = parsed.get("completed_chapters", [])
	for chapter: Dictionary in CreationJourney.CHAPTERS:
		if chapter.id in saved_chapters:
			completed_chapters.append(chapter.id)
	restore_equipped_appearance(parsed)
	
	var saved_counts: Dictionary = parsed.get("counts", {})
	for k: String in counts:
		counts[k] = maxi(0, int(saved_counts.get(k, 0)))
	
	var saved_relics: Dictionary = parsed.get("relic_levels", {})
	for relic: Dictionary in DB.RELICS:
		relic_levels[relic.id] = clampi(int(saved_relics.get(relic.id, 0)), 0, int(relic.get("max_lvl", 1000000)))
	
	var last_time: float = float(parsed.get("timestamp", Time.get_unix_time_from_system()))
	var elapsed: float = clamp(Time.get_unix_time_from_system() - last_time, 0.0, 43200.0)
	if elapsed > 10.0:
		var offline_gain: float = get_total_qps() * elapsed * 0.5
		if offline_gain > 0.0:
			add_quanta(offline_gain)
			call_deferred("_emit_offline_popup", elapsed, offline_gain)

func _emit_offline_popup(elapsed: float, gain: float) -> void:
	offline_progress_calculated.emit(elapsed, gain)

func get_active_chapter() -> Dictionary:
	for chapter: Dictionary in CreationJourney.CHAPTERS:
		if not chapter.id in completed_chapters:
			return chapter
	return {}

func claim_creation_chapter() -> bool:
	var chapter: Dictionary = get_active_chapter()
	if chapter.is_empty():
		return false
	if CreationJourney.get_progress(chapter, current_run_quanta, counts) < float(chapter.target):
		return false
	completed_chapters.append(chapter.id)
	add_quanta(float(chapter.reward))
	save_game()
	chapter_completed.emit(chapter)
	state_updated.emit()
	return true

func _notification(what: int) -> void:
	if what in [NOTIFICATION_WM_CLOSE_REQUEST, NOTIFICATION_APPLICATION_PAUSED, NOTIFICATION_APPLICATION_FOCUS_OUT]:
		save_game()