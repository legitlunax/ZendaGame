class_name GenesisTheme
extends RefCounted

static func panel(fill: Color, edge: Color, radius: int = 10) -> StyleBoxFlat:
	var style: StyleBoxFlat = StyleBoxFlat.new()
	style.bg_color = fill
	style.border_color = edge
	style.set_border_width_all(1)
	style.set_corner_radius_all(radius)
	style.content_margin_left = 14.0
	style.content_margin_right = 14.0
	style.content_margin_top = 9.0
	style.content_margin_bottom = 9.0
	return style

static func build() -> Theme:
	var result: Theme = Theme.new()
	result.default_font_size = 14
	result.set_color("font_color", "Label", Color("d7e8ef"))
	result.set_color("font_color", "Button", Color("d0f6ee"))
	result.set_color("font_disabled_color", "Button", Color("607886"))
	result.set_stylebox("normal", "Button", panel(Color("142c36"), Color("315663")))
	result.set_stylebox("hover", "Button", panel(Color("20454b"), Color("78dcca")))
	result.set_stylebox("pressed", "Button", panel(Color("28545a"), Color("edce86")))
	result.set_stylebox("disabled", "Button", panel(Color("101d2a"), Color("243644")))
	var focus: StyleBoxFlat = panel(Color(0, 0, 0, 0), Color("edce86"))
	result.set_stylebox("focus", "Button", focus)
	result.set_stylebox("panel", "TabContainer", panel(Color("0d1828"), Color("2a4055")))
	result.set_stylebox("tab_selected", "TabContainer", panel(Color("1a3540"), Color("507a7d"), 6))
	result.set_stylebox("tab_unselected", "TabContainer", panel(Color("101d2a"), Color("263b4c"), 6))
	result.set_color("font_selected_color", "TabContainer", Color("ffe4a4"))
	result.set_color("font_unselected_color", "TabContainer", Color("8ca9b9"))
	result.set_stylebox("background", "ProgressBar", panel(Color("101e30"), Color("26394a"), 4))
	result.set_stylebox("fill", "ProgressBar", panel(Color("58bca7"), Color("82e6cd"), 4))
	return result