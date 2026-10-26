class_name FairyAppearance
extends Resource

# Visual configuration only: never grants progression or emotion unlocks.
@export var appearance_id: String = "ivory_queen"
@export var outfit_color: Color = Color("eee3fa")
@export var accent_color: Color = Color("58327c")
@export var hair_color: Color = Color("7962ab")
@export var wing_color: Color = Color("ddcfee")
@export_range(0.5, 1.5) var wing_scale: float = 1.0
@export var wings_visible: bool = true
@export var cosmetics_visible: bool = true