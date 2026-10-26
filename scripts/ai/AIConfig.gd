class_name AIConfig
extends RefCounted

const CONFIG_FILE_PATH := "user://ai_credentials.json"

static func get_openai_api_key() -> String:
	var env_key := OS.get_environment("OPENAI_API_KEY")
	if not env_key.is_empty():
		return env_key
	return _load_key_from_file("openai_api_key")

static func get_minimax_api_key() -> String:
	var env_key := OS.get_environment("MINIMAX_API_KEY")
	if not env_key.is_empty():
		return env_key
	return _load_key_from_file("minimax_api_key")

static func get_minimax_group_id() -> String:
	var env_id := OS.get_environment("MINIMAX_GROUP_ID")
	if not env_id.is_empty():
		return env_id
	return _load_key_from_file("minimax_group_id")

static func save_credentials(openai_key: String = "", minimax_key: String = "", minimax_group: String = "") -> void:
	var data: Dictionary = {}
	if FileAccess.file_exists(CONFIG_FILE_PATH):
		var read_file := FileAccess.open(CONFIG_FILE_PATH, FileAccess.READ)
		if read_file:
			var text := read_file.get_as_text()
			var parsed: Variant = JSON.parse_string(text)
			if parsed is Dictionary:
				data = parsed
			read_file.close()

	if not openai_key.is_empty():
		data["openai_api_key"] = openai_key
	if not minimax_key.is_empty():
		data["minimax_api_key"] = minimax_key
	if not minimax_group.is_empty():
		data["minimax_group_id"] = minimax_group

	var write_file := FileAccess.open(CONFIG_FILE_PATH, FileAccess.WRITE)
	if write_file:
		write_file.store_string(JSON.stringify(data, "\t"))
		write_file.close()

static func _load_key_from_file(key_name: String) -> String:
	if not FileAccess.file_exists(CONFIG_FILE_PATH):
		return ""
	var file := FileAccess.open(CONFIG_FILE_PATH, FileAccess.READ)
	if not file:
		return ""
	var text := file.get_as_text()
	file.close()
	var parsed: Variant = JSON.parse_string(text)
	if parsed is Dictionary and parsed.has(key_name):
		return str(parsed[key_name])
	return ""
