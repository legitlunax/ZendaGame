extends Control

var lbl_quanta: Label
var lbl_qps: Label
var lbl_scale: Label
var lbl_seeds: Label
var lbl_goddess_title: Label
var lbl_goddess_lore: Label
var lbl_next_evolution: Label

var visualizer: CosmicVisualizer
var miracle_buttons: Dictionary = {}
var gen_rows: Dictionary = {}
var relic_rows: Dictionary = {}
var mult_buttons: Dictionary = {}

var lbl_prestige_info: Label
var btn_prestige: Button
var popup_banner: PanelContainer
var lbl_popup: Label
var journey_title: Label
var journey_goal: Label
var journey_progress: ProgressBar
var journey_claim: Button
var chronicle: Label
var bloom_confirmation: ConfirmationDialog
var banner_tween: Tween
var creation_audio: CreationAudio
var evolution_progress: ProgressBar
var demo_selector: OptionButton
var utility_menu: MenuButton
var help_dialog: AcceptDialog
var creation_tabs: TabContainer
var core_hint: Label
var page_scroll: ScrollContainer
var page_content: VBoxContainer
var body_layout: BoxContainer
var left_column: VBoxContainer
var right_column: VBoxContainer
var compact_layout: bool = false
var card_layouts: Array[BoxContainer] = []
var fairy_icons: Array[TextureRect] = []
var miracle_layout: BoxContainer

func _ready() -> void:
	theme = GenesisTheme.build()
	creation_audio = CreationAudio.new()
	add_child(creation_audio)
	_build_ui()
	resized.connect(_update_responsive_layout)
	_update_responsive_layout()
	Game.state_updated.connect(_refresh_ui)
	Game.goddess_evolved.connect(_on_goddess_evolved)
	Game.miracle_triggered.connect(_on_miracle_triggered)
	Game.offline_progress_calculated.connect(_on_offline_progress)
	Game.chapter_completed.connect(_on_chapter_completed)
	_refresh_ui()

func _make_panel_style(bg: Color, border: Color, radius: int = 8) -> StyleBoxFlat:
	var sb := StyleBoxFlat.new()
	sb.bg_color = bg
	sb.border_color = border
	sb.set_border_width_all(1)
	sb.set_corner_radius_all(radius)
	sb.set_content_margin_all(10)
	return sb

func _build_ui() -> void:
	var bg := ColorRect.new()
	bg.color = Color(0.05, 0.05, 0.10)
	bg.set_anchors_preset(Control.PRESET_FULL_RECT)
	add_child(bg)
	
	page_scroll = ScrollContainer.new()
	page_scroll.set_anchors_preset(Control.PRESET_FULL_RECT)
	page_scroll.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	add_child(page_scroll)
	var margin: MarginContainer = MarginContainer.new()
	margin.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	for side: String in ["left", "right", "top", "bottom"]:
		margin.add_theme_constant_override("margin_" + side, 12)
	page_scroll.add_child(margin)
	var root_vbox: VBoxContainer = VBoxContainer.new()
	page_content = root_vbox
	root_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	root_vbox.add_theme_constant_override("separation", 5)
	margin.add_child(root_vbox)
	_build_demo_toolbar(root_vbox)
	
	var header := PanelContainer.new()
	header.add_theme_stylebox_override("panel", _make_panel_style(Color(0.08, 0.08, 0.16), Color(0.85, 0.7, 0.35, 0.6), 0))
	root_vbox.add_child(header)
	
	var header_hbox: HFlowContainer = HFlowContainer.new()
	header_hbox.add_theme_constant_override("separation", 6)
	header.add_child(header_hbox)
	
	lbl_quanta = Label.new()
	lbl_quanta.add_theme_font_size_override("font_size", 16)
	lbl_quanta.add_theme_color_override("font_color", Color(1.0, 0.92, 0.55))
	header_hbox.add_child(lbl_quanta)
	
	lbl_qps = Label.new()
	lbl_qps.add_theme_font_size_override("font_size", 16)
	lbl_qps.add_theme_color_override("font_color", Color(0.6, 0.9, 1.0))
	header_hbox.add_child(lbl_qps)
	
	lbl_scale = Label.new()
	lbl_scale.add_theme_font_size_override("font_size", 15)
	lbl_scale.add_theme_color_override("font_color", Color(0.8, 0.75, 1.0))
	header_hbox.add_child(lbl_scale)
	
	lbl_seeds = Label.new()
	lbl_seeds.add_theme_font_size_override("font_size", 16)
	lbl_seeds.add_theme_color_override("font_color", Color(1.0, 0.6, 0.9))
	header_hbox.add_child(lbl_seeds)
	
	var body_hbox: BoxContainer = BoxContainer.new()
	body_layout = body_hbox
	body_hbox.size_flags_vertical = Control.SIZE_EXPAND_FILL
	body_hbox.add_theme_constant_override("separation", 12)
	root_vbox.add_child(body_hbox)
	
	var left_vbox := VBoxContainer.new()
	left_column = left_vbox
	left_vbox.custom_minimum_size = Vector2(480, 0)
	left_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	left_vbox.add_theme_constant_override("separation", 8)
	body_hbox.add_child(left_vbox)
	
	var goddess_header := PanelContainer.new()
	goddess_header.add_theme_stylebox_override("panel", _make_panel_style(Color(0.09, 0.08, 0.18), Color(0.5, 0.4, 0.8, 0.5)))
	left_vbox.add_child(goddess_header)
	
	var gh_vbox := VBoxContainer.new()
	goddess_header.add_child(gh_vbox)
	
	lbl_goddess_title = Label.new()
	lbl_goddess_title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl_goddess_title.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_goddess_title.add_theme_font_size_override("font_size", 18)
	lbl_goddess_title.add_theme_color_override("font_color", Color(1.0, 0.88, 0.55))
	gh_vbox.add_child(lbl_goddess_title)
	
	lbl_goddess_lore = Label.new()
	lbl_goddess_lore.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl_goddess_lore.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_goddess_lore.add_theme_font_size_override("font_size", 13)
	lbl_goddess_lore.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_goddess_lore.add_theme_color_override("font_color", Color(0.75, 0.8, 0.95))
	gh_vbox.add_child(lbl_goddess_lore)
	
	lbl_next_evolution = Label.new()
	lbl_next_evolution.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl_next_evolution.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_next_evolution.add_theme_font_size_override("font_size", 12)
	lbl_next_evolution.add_theme_color_override("font_color", Color(0.5, 0.95, 0.75))
	gh_vbox.add_child(lbl_next_evolution)
	evolution_progress = ProgressBar.new()
	evolution_progress.custom_minimum_size.y = 8
	evolution_progress.show_percentage = false
	gh_vbox.add_child(evolution_progress)
	
	visualizer = CosmicVisualizer.new()
	visualizer.core_tapped.connect(func() -> void: creation_audio.play_sound("tap"))
	visualizer.size_flags_vertical = Control.SIZE_EXPAND_FILL
	left_vbox.add_child(visualizer)
	left_vbox.move_child(visualizer, 0)
	core_hint = Label.new()
	core_hint.text = "TAP THE LIGHT / CREATE QUANTA"
	core_hint.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	core_hint.add_theme_color_override("font_color", Color(0.6, 0.85, 0.9))
	left_vbox.add_child(core_hint)
	_build_journey(left_vbox)
	
	var miracles_hbox: BoxContainer = BoxContainer.new()
	miracle_layout = miracles_hbox
	miracles_hbox.add_theme_constant_override("separation", 8)
	left_vbox.add_child(miracles_hbox)
	
	for m in DB.MIRACLES:
		var btn := Button.new()
		btn.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		btn.custom_minimum_size = Vector2(0, 44)
		btn.tooltip_text = m.desc
		btn.icon = load("res://assets/generated/icons/%s.svg" % m.id) as Texture2D
		btn.expand_icon = true
		btn.add_theme_constant_override("icon_max_width", 38)
		btn.pressed.connect(func(): Game.activate_miracle(m.id))
		miracles_hbox.add_child(btn)
		miracle_buttons[m.id] = btn
	
	var right_vbox := VBoxContainer.new()
	right_column = right_vbox
	right_vbox.custom_minimum_size = Vector2(600, 0)
	right_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	right_vbox.add_theme_constant_override("separation", 8)
	body_hbox.add_child(right_vbox)
	
	var mult_hbox: HFlowContainer = HFlowContainer.new()
	mult_hbox.add_theme_constant_override("separation", 6)
	right_vbox.add_child(mult_hbox)
	
	var mult_label := Label.new()
	mult_label.text = "Buy:"
	mult_hbox.add_child(mult_label)
	
	for mode in [1, 10, 100, -1]:
		var m_btn := Button.new()
		m_btn.text = "MAX" if mode == -1 else "x%d" % mode
		m_btn.custom_minimum_size = Vector2(48, 44)
		m_btn.pressed.connect(func():
			Game.buy_multiplier_mode = mode
			_refresh_ui()
		)
		mult_hbox.add_child(m_btn)
		mult_buttons[mode] = m_btn
	
	var tabs := TabContainer.new()
	tabs.clip_tabs = true
	creation_tabs = tabs
	tabs.size_flags_vertical = Control.SIZE_EXPAND_FILL
	right_vbox.add_child(tabs)
	
	var scroll_gen := ScrollContainer.new()
	scroll_gen.name = "Cosmic Creation"
	tabs.add_child(scroll_gen)
	
	var gen_vbox := VBoxContainer.new()
	gen_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	gen_vbox.add_theme_constant_override("separation", 6)
	scroll_gen.add_child(gen_vbox)
	
	for gen in DB.GENERATORS:
		var card := PanelContainer.new()
		card.add_theme_stylebox_override("panel", _make_panel_style(Color(0.09, 0.10, 0.18), gen.color.darkened(0.4)))
		gen_vbox.add_child(card)
		
		var row: BoxContainer = BoxContainer.new()
		card_layouts.append(row)
		row.add_theme_constant_override("separation", 12)
		card.add_child(row)
		_add_asset_icon(row, gen.id)
		
		var info_v := VBoxContainer.new()
		info_v.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		row.add_child(info_v)
		
		var title_lbl := Label.new()
		title_lbl.add_theme_font_size_override("font_size", 16)
		title_lbl.add_theme_color_override("font_color", gen.color.lightened(0.3))
		info_v.add_child(title_lbl)
		
		var desc_lbl := Label.new()
		desc_lbl.text = "%s — %s" % [gen.scale, gen.desc]
		desc_lbl.add_theme_font_size_override("font_size", 12)
		desc_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		desc_lbl.add_theme_color_override("font_color", Color(0.7, 0.75, 0.85))
		info_v.add_child(desc_lbl)
		
		var prod_lbl := Label.new()
		prod_lbl.add_theme_font_size_override("font_size", 12)
		prod_lbl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		prod_lbl.add_theme_color_override("font_color", Color(0.55, 0.95, 0.75))
		info_v.add_child(prod_lbl)
		
		var buy_btn := Button.new()
		buy_btn.custom_minimum_size = Vector2(108, 48)
		buy_btn.pressed.connect(func() -> void:
			if Game.buy_generator(gen.id):
				creation_audio.play_sound("purchase")
		)
		row.add_child(buy_btn)
		
		gen_rows[gen.id] = {
			"card": card,
			"title": title_lbl,
			"prod": prod_lbl,
			"btn": buy_btn,
			"desc": desc_lbl,
			"data": gen
		}
	
	var scroll_relics := ScrollContainer.new()
	scroll_relics.name = "Divine Relics"
	tabs.add_child(scroll_relics)
	
	var relic_vbox := VBoxContainer.new()
	relic_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	relic_vbox.add_theme_constant_override("separation", 8)
	scroll_relics.add_child(relic_vbox)
	
	for r in DB.RELICS:
		var r_card := PanelContainer.new()
		r_card.add_theme_stylebox_override("panel", _make_panel_style(Color(0.12, 0.08, 0.18), Color(0.9, 0.6, 1.0, 0.5)))
		relic_vbox.add_child(r_card)
		
		var r_row: BoxContainer = BoxContainer.new()
		card_layouts.append(r_row)
		r_card.add_child(r_row)
		_add_asset_icon(r_row, r.id)
		
		var r_info := VBoxContainer.new()
		r_info.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		r_row.add_child(r_info)
		
		var r_title := Label.new()
		r_title.add_theme_font_size_override("font_size", 16)
		r_title.add_theme_color_override("font_color", Color(1.0, 0.8, 0.95))
		r_info.add_child(r_title)
		
		var r_desc := Label.new()
		r_desc.text = r.desc
		r_desc.add_theme_font_size_override("font_size", 13)
		r_desc.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		r_info.add_child(r_desc)
		
		var r_btn := Button.new()
		r_btn.custom_minimum_size = Vector2(170, 48)
		r_btn.pressed.connect(func() -> void:
			if Game.buy_relic(r.id):
				creation_audio.play_sound("purchase")
		)
		r_row.add_child(r_btn)
		
		relic_rows[r.id] = {
			"title": r_title,
			"btn": r_btn,
			"data": r
		}
	
	var prestige_panel := PanelContainer.new()
	prestige_panel.name = "The Big Bloom"
	prestige_panel.add_theme_stylebox_override("panel", _make_panel_style(Color(0.10, 0.06, 0.16), Color(1.0, 0.8, 0.4, 0.6)))
	tabs.add_child(prestige_panel)
	
	var p_vbox := VBoxContainer.new()
	p_vbox.add_theme_constant_override("separation", 16)
	prestige_panel.add_child(p_vbox)
	
	lbl_prestige_info = Label.new()
	lbl_prestige_info.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_prestige_info.add_theme_font_size_override("font_size", 15)
	p_vbox.add_child(lbl_prestige_info)
	
	btn_prestige = Button.new()
	btn_prestige.custom_minimum_size = Vector2(260, 56)
	btn_prestige.pressed.connect(func():
		bloom_confirmation.dialog_text = "Begin a new universe for +%d Genesis Seeds?\n\nQuanta, generators, goddess form, and active miracles reset.\nSeeds, relics, lifetime creation, and your chronicle remain." % Game.get_pending_genesis_seeds()
		_popup_fitted(bloom_confirmation)
	)
	p_vbox.add_child(btn_prestige)
	bloom_confirmation = ConfirmationDialog.new()
	bloom_confirmation.title = "Plant a new universe"
	bloom_confirmation.get_label().autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	bloom_confirmation.confirmed.connect(func() -> void:
		if Game.perform_big_bloom():
			_show_banner("THE BIG BLOOM COMPLETE! A higher reality awakens.")
	)
	add_child(bloom_confirmation)
	var chronicle_scroll: ScrollContainer = ScrollContainer.new()
	chronicle_scroll.name = "Chronicle"
	tabs.add_child(chronicle_scroll)
	chronicle = Label.new()
	chronicle.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	chronicle.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	chronicle.add_theme_font_size_override("font_size", 16)
	chronicle_scroll.add_child(chronicle)
	
	popup_banner = PanelContainer.new()
	popup_banner.visible = false
	popup_banner.add_theme_stylebox_override("panel", _make_panel_style(Color(0.18, 0.12, 0.32, 0.95), Color(1.0, 0.85, 0.4)))
	popup_banner.set_anchors_preset(Control.PRESET_CENTER_TOP)
	popup_banner.position = Vector2(-220, 62)
	popup_banner.custom_minimum_size = Vector2(440, 48)
	add_child(popup_banner)
	
	lbl_popup = Label.new()
	lbl_popup.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	lbl_popup.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	lbl_popup.add_theme_color_override("font_color", Color(1.0, 0.95, 0.7))
	popup_banner.add_child(lbl_popup)

func _refresh_ui() -> void:
	_refresh_journey()
	var creation_level: int = Game.get_creation_level()
	var resource_name: String = Game.get_resource_name()
	creation_tabs.set_tab_hidden(1, Game.genesis_seeds == 0)
	creation_tabs.set_tab_hidden(2, not Game.has_reached("wormhole_nexus"))
	core_hint.text = "HOLD ON TO HOPE / TAP THE VOID" if creation_level == 0 else "%02d / %s" % [creation_level, CosmicProgression.TITLES[creation_level].to_upper()]
	core_hint.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	if demo_selector != null:
		demo_selector.select(Game.demo_era)
	lbl_quanta.text = "%s %s" % [DB.format_num(Game.quanta), resource_name]
	lbl_qps.text = "(+%s / sec)" % DB.format_num(Game.get_total_qps())
	lbl_scale.text = "Scale: %s" % Game.get_current_scale_title()
	lbl_seeds.text = "❀ Genesis Seeds: %d (+%d%%)" % [Game.genesis_seeds, Game.genesis_seeds * 25]
	lbl_seeds.visible = Game.genesis_seeds > 0
	
	var form: Dictionary = DB.GODDESS_FORMS[Game.goddess_stage]
	lbl_goddess_title.text = "★ %s (x%.1f Aura)" % [form.title, form.mult_bonus]
	lbl_goddess_lore.text = '"%s"' % form.lore
	if not Game.has_reached("hadron_forge"):
		lbl_goddess_title.text = "ONLY HOPE" if creation_level == 0 else CosmicProgression.TITLES[creation_level].to_upper()
		lbl_goddess_lore.text = "No stars. No matter. Not even time. But the possibility of something remains." if creation_level == 0 else "A speculative beginning: possibility is slowly becoming structure."
	
	if Game.goddess_stage < DB.GODDESS_FORMS.size() - 1:
		var next_f: Dictionary = DB.GODDESS_FORMS[Game.goddess_stage + 1]
		lbl_next_evolution.text = "Next Ascension at %s Run Quanta" % DB.format_num(next_f.req_quanta)
		evolution_progress.max_value = float(next_f.req_quanta)
		evolution_progress.value = Game.current_run_quanta
	else:
		lbl_next_evolution.text = "Maximum Omniversal Divinity Reached"
		evolution_progress.value = evolution_progress.max_value
	if creation_level < DB.GENERATORS.size():
		var next_layer: Dictionary = DB.GENERATORS[creation_level]
		lbl_next_evolution.text = "Next discovery: %s / %s %s" % [next_layer.name, DB.format_num(next_layer.base_cost), resource_name]
		evolution_progress.max_value = float(next_layer.base_cost)
		evolution_progress.value = Game.quanta
	
	for m in mult_buttons.keys():
		mult_buttons[m].modulate = Color(1.0, 0.9, 0.4) if Game.buy_multiplier_mode == m else Color(1, 1, 1)
	
	for m in DB.MIRACLES:
		var btn: Button = miracle_buttons[m.id]
		btn.visible = Game.has_reached("hadron_forge")
		var cd: float = Game.miracle_cooldowns.get(m.id, 0.0)
		if cd > 0.0:
			btn.disabled = true
			btn.text = "%s (%.0fs)" % [m.name, cd]
		else:
			btn.disabled = false
			btn.text = "✦ %s" % m.name
	
	for gen_id in gen_rows.keys():
		var row: Dictionary = gen_rows[gen_id]
		var gen: Dictionary = row.data
		row.card.visible = Game.is_generator_discovered(gen_id)
		var owned: int = Game.counts.get(gen_id, 0)
		var qps: float = Game.get_generator_qps(gen)
		var buy_info: Dictionary = Game.get_bulk_buy_info(gen)
		
		row.title.text = "%s / %s  [%d]" % [DB.FAIRY_NAMES.get(gen_id, gen.name), gen.name, owned]
		row.title.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		row.prod.text = "Nurturing: %s /s (Next x2 at %d)" % [DB.format_num(qps), (int(owned / 25) + 1) * 25]
		if not str(gen.dimension).is_empty():
			row.prod.text += "\nDimension built" if owned > 0 else "\nDimension foundation / ready to build"
		if gen_id in ["yin", "yang", "element_wood", "element_fire", "element_earth", "element_metal", "element_water"]:
			row.prod.text += "\nShared harmony: x%.2f" % Game.get_harmony_multiplier()
		if gen_id in ["gaia_biosphere", "all_seeing", "omnipresence", "omnipotence", "divine_life", "omniall", "hope_return"]:
			row.prod.text += "\nLiving Unity: x%.2f" % Game.get_living_unity_multiplier()
		var action: String = "Nurture"
		if owned == 0:
			action = "Build" if not str(gen.dimension).is_empty() else "Discover"
		row.btn.text = "%s +%d\n%s" % [action, buy_info.amount, DB.format_num(buy_info.cost)]
		row.btn.tooltip_text = "%s %s" % [DB.format_num(buy_info.cost), resource_name]
		row.card.tooltip_text = str(gen.desc)
		row.desc.visible = not compact_layout or owned == 0
		row.btn.disabled = not buy_info.can_afford
	
	for r_id in relic_rows.keys():
		var r_row: Dictionary = relic_rows[r_id]
		var r: Dictionary = r_row.data
		var lvl: int = Game.relic_levels.get(r_id, 0)
		var maxed: bool = r.has("max_lvl") and lvl >= r.max_lvl
		var cost: int = r.base_seed_cost * (lvl + 1)
		
		r_row.title.text = "%s / %s (Lv. %d)" % [DB.FAIRY_NAMES.get(r_id, r.name), r.name, lvl]
		r_row.title.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		if maxed:
			r_row.btn.text = "MAXED"
			r_row.btn.disabled = true
		else:
			r_row.btn.text = "Upgrade\n%d Genesis Seeds" % cost
			r_row.btn.disabled = Game.genesis_seeds < cost
	
	var pending: int = Game.get_pending_genesis_seeds()
	lbl_prestige_info.text = (
		"THE BIG BLOOM (COSMIC RE-GENESIS)\n\n" +
		"After building a Living Universe Dimension and 1.00 M+ Creation Potential, you may fold this universe " +
		"into immortal Genesis Seeds.\n\n" +
		"• Each Genesis Seed grants +25% permanent production speed AND unlocks Divine Relics.\n" +
		"• Current Run Quanta: %s\n" % DB.format_num(Game.current_run_quanta) +
		"• Genesis Seeds upon Ascension: +%d" % pending
	)
	btn_prestige.text = "Trigger The Big Bloom (+%d Seeds)" % pending
	btn_prestige.disabled = pending <= 0

func _on_goddess_evolved(new_stage: Dictionary) -> void:
	_show_banner("DIVINE ASCENSION! Zenda evolved into %s!" % new_stage.title)

func _on_miracle_triggered(_id: String, text_popup: String) -> void:
	visualizer.spawn_floating_text(visualizer.size * 0.5, text_popup, Color(0.5, 1.0, 0.9))

func _on_offline_progress(seconds: float, gain: float) -> void:
	var mins := int(seconds / 60.0)
	_show_banner("Welcome back! Zenda wove +%s Quanta over %d min." % [DB.format_num(gain), max(1, mins)])

func _show_banner(msg: String) -> void:
	lbl_popup.text = msg
	popup_banner.visible = true
	if banner_tween != null and banner_tween.is_valid():
		banner_tween.kill()
	banner_tween = create_tween()
	banner_tween.tween_interval(4.5)
	banner_tween.tween_callback(func() -> void:
		popup_banner.visible = false
	)

func _build_journey(parent: VBoxContainer) -> void:
	var panel: PanelContainer = PanelContainer.new()
	panel.add_theme_stylebox_override("panel", _make_panel_style(Color(0.06, 0.13, 0.17), Color(0.3, 0.8, 0.75, 0.5)))
	parent.add_child(panel)
	var box: VBoxContainer = VBoxContainer.new()
	panel.add_child(box)
	journey_title = Label.new()
	journey_title.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	journey_title.add_theme_color_override("font_color", Color(0.65, 1.0, 0.85))
	box.add_child(journey_title)
	journey_goal = Label.new()
	journey_goal.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	journey_goal.add_theme_font_size_override("font_size", 13)
	box.add_child(journey_goal)
	journey_progress = ProgressBar.new()
	journey_progress.custom_minimum_size.y = 10
	journey_progress.show_percentage = false
	box.add_child(journey_progress)
	journey_claim = Button.new()
	journey_claim.custom_minimum_size.y = 36
	journey_claim.pressed.connect(func() -> void: Game.claim_creation_chapter())
	box.add_child(journey_claim)

func _refresh_journey() -> void:
	var chapter: Dictionary = Game.get_active_chapter()
	if chapter.is_empty():
		var final_chapter: Dictionary = CreationJourney.CHAPTERS.back()
		journey_title.text = "CREATION JOURNEY / %s built" % str(final_chapter.dimension)
		journey_goal.text = "Existence rests on interwoven dimensions. This was your building sequence, not a universal hierarchy. Life recognizes its shared divine nature; keep nurturing every life."
		journey_progress.value = journey_progress.max_value
		journey_claim.text = "All chapters remembered"
		journey_claim.disabled = true
	else:
		var progress: float = CreationJourney.get_progress(chapter, Game.current_run_quanta, Game.counts)
		journey_title.text = chapter.title
		journey_goal.text = chapter.goal
		journey_progress.max_value = float(chapter.target)
		journey_progress.value = progress
		journey_claim.text = "Remember / +%s %s" % [DB.format_num(float(chapter.reward)), Game.get_resource_name()]
		journey_claim.disabled = progress < float(chapter.target)
	var entries: PackedStringArray = ["THE CREATION CHRONICLE", "Existence is built on many interwoven dimensions. This chronicle records your sequence, not a mandatory order for all creation. What you nurture is remembered across every Big Bloom."]
	for entry: Dictionary in CreationJourney.CHAPTERS:
		if entry.id in Game.completed_chapters:
			entries.append("%s\n%s" % [entry.title, entry.lore])
	if Game.completed_chapters.is_empty():
		entries.append("An unwritten universe. Tap the cosmic core to write its first chapter.")
	chronicle.text = "\n\n".join(entries)

func _on_chapter_completed(chapter: Dictionary) -> void:
	var unlocked_state: String = ""
	for state: String in Game.EMOTION_CHAPTERS:
		if "discover_" + str(Game.EMOTION_CHAPTERS[state]) == str(chapter.get("id", "")):
			unlocked_state = state
	if not unlocked_state.is_empty():
		visualizer.play_unlocked_emotion(unlocked_state, 6.0)
	else:
		visualizer.play_unlocked_emotion("Dance", 5.0)
	_show_banner(chapter.lore)
	if not unlocked_state.is_empty():
		_show_banner("ANIMATION UNLOCKED: %s\n%s" % [unlocked_state, chapter.lore])
	var completion: String = "DIMENSION BUILT" if not str(chapter.get("dimension", "")).is_empty() else "CREATION REMEMBERED"
	visualizer.spawn_floating_text(visualizer.size * 0.5, "+%s / %s" % [DB.format_num(float(chapter.reward)), completion], Color(0.6, 1.0, 0.8))

func _add_asset_icon(parent: BoxContainer, asset_id: String) -> void:
	var icon: TextureRect = TextureRect.new()
	icon.texture = load("res://assets/generated/icons/%s.svg" % asset_id) as Texture2D
	icon.custom_minimum_size = Vector2(80, 96)
	icon.tooltip_text = "%s / Creation Fairy" % DB.FAIRY_NAMES.get(asset_id, asset_id)
	icon.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	icon.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	icon.mouse_filter = Control.MOUSE_FILTER_IGNORE
	fairy_icons.append(icon)
	parent.add_child(icon)

func _build_demo_toolbar(parent: VBoxContainer) -> void:
	var row: HBoxContainer = HBoxContainer.new()
	row.add_theme_constant_override("separation", 6)
	parent.add_child(row)
	var title: Label = Label.new()
	title.text = "GENESIS"
	title.add_theme_font_size_override("font_size", 16)
	title.add_theme_color_override("font_color", Color("f5dc9c"))
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	title.tooltip_text = "Phone preview / no save" if Game.demo_mode else "Your universe / autosave"
	row.add_child(title)
	if Game.demo_mode:
		demo_selector = OptionButton.new()
		demo_selector.custom_minimum_size = Vector2(0, 44)
		demo_selector.fit_to_longest_item = false
		demo_selector.tooltip_text = "Preview an era / no save"
		for preset: Dictionary in Game.DEMO_ERAS:
			demo_selector.add_item(str(preset.name))
		demo_selector.item_selected.connect(_select_demo_era)
		row.add_child(demo_selector)
	utility_menu = MenuButton.new()
	utility_menu.text = "Menu"
	utility_menu.custom_minimum_size.y = 44
	row.add_child(utility_menu)
	var popup: PopupMenu = utility_menu.get_popup()
	popup.add_check_item("Mute sound", 0)
	popup.set_item_checked(0, AudioServer.is_bus_mute(0))
	popup.add_item("How to play", 1)
	popup.id_pressed.connect(func(id: int) -> void:
		if id == 0:
			var muted: bool = not AudioServer.is_bus_mute(0)
			AudioServer.set_bus_mute(0, muted)
			popup.set_item_checked(0, muted)
		else:
			_popup_fitted(help_dialog)
	)
	help_dialog = AcceptDialog.new()
	help_dialog.title = "Build the dimensions of existence"
	help_dialog.get_label().autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	help_dialog.dialog_text = "Begin with nothing but hope. Tap the void.\nAwaken Elpis for 5 Hope, then explore {discoveries} fairy discoveries.\nMajor stages conclude by building a dimension. The universe and everything in existence rest on many interwoven dimensions.\nBuild elemental, time, space, quantum, living-universe, Dao-like, reality, and living-divinity domains.\nAny dimension could precede or follow another in the story. This journey uses one sequential building path, not a ranking of dimensions.\nEach discovery reveals the next; claim chapter rewards to keep growing.\nBalanced Yin/Yang pairs add 1% production each, up to 100 pairs.\nComplete five-element sets add 2% each, up to 100 sets.\nLiving Unity sets add 2% each, up to 50 sets.\n\nOmnielle shelters infinite superposed universes without a shared clock, distance, or lawbook.\nOriginal fantasy cosmology, not science or a teaching of a spiritual tradition.\nDemo previews never load or write your saved universe.".replace("{discoveries}", str(DB.GENERATORS.size()))
	add_child(help_dialog)

func _select_demo_era(index: int) -> void:
	if Game.load_demo_era(index):
		demo_selector.select(index)
		for player: AudioStreamPlayer in creation_audio.players.values():
			player.stop()
		visualizer.floating_texts.clear()
		visualizer.creation_sparks.clear()
		visualizer.pulse_scale = 1.0 if index == 0 else 1.22
		_show_banner("%s / Play, weave, and explore. Your saved universe is untouched." % Game.DEMO_ERAS[index].name)

func _popup_fitted(dialog: AcceptDialog) -> void:
	var available: Vector2i = Vector2i(get_viewport_rect().size) - Vector2i(24, 48)
	dialog.size = Vector2i(mini(560, available.x), mini(400, available.y))
	dialog.popup_centered(dialog.size)

func _update_responsive_layout() -> void:
	if body_layout == null:
		return
	compact_layout = size.x < 1000.0
	miracle_layout.vertical = compact_layout
	body_layout.vertical = compact_layout
	left_column.custom_minimum_size.x = 0.0 if compact_layout else 440.0
	right_column.custom_minimum_size.x = 0.0 if compact_layout else 500.0
	page_content.custom_minimum_size.y = maxf(0.0, size.y - 24.0)
	visualizer.custom_minimum_size.y = 230.0 if compact_layout else 260.0
	lbl_goddess_lore.visible = not compact_layout
	lbl_goddess_title.tooltip_text = lbl_goddess_lore.text
	lbl_goddess_title.add_theme_font_size_override("font_size", 14 if compact_layout else 18)
	creation_tabs.custom_minimum_size.y = 520.0 if compact_layout else 360.0
	for card: BoxContainer in card_layouts:
		card.vertical = false
	for icon: TextureRect in fairy_icons:
		icon.custom_minimum_size = Vector2(40, 56) if compact_layout else Vector2(80, 96)
		icon.size_flags_horizontal = Control.SIZE_FILL
	for row: Dictionary in gen_rows.values():
		row.title.add_theme_font_size_override("font_size", 13 if compact_layout else 16)
		row.btn.add_theme_font_size_override("font_size", 12 if compact_layout else 14)
		row.desc.visible = not compact_layout or int(Game.counts.get(row.data.id, 0)) == 0
	lbl_scale.visible = not compact_layout
	popup_banner.custom_minimum_size.x = minf(440.0, size.x - 32.0)
	popup_banner.size.x = popup_banner.custom_minimum_size.x
	popup_banner.position = Vector2(-popup_banner.size.x * 0.5, 16.0)
	journey_claim.custom_minimum_size.y = 48.0
