class_name CreationAudio
extends Node

const SOUND_NAMES: Array[String] = ["tap", "purchase", "chapter", "evolution", "miracle"]
var players: Dictionary[String, AudioStreamPlayer] = {}
var tap_cooldown_msec: int = 0

func _ready() -> void:
	for sound_name: String in SOUND_NAMES:
		var player: AudioStreamPlayer = AudioStreamPlayer.new()
		player.stream = load("res://assets/generated/audio/%s.wav" % sound_name) as AudioStream
		player.volume_db = -12.0
		add_child(player)
		players[sound_name] = player
	Game.goddess_evolved.connect(_on_evolution)
	Game.chapter_completed.connect(_on_chapter)
	Game.miracle_triggered.connect(_on_miracle)

func play_sound(sound_name: String) -> void:
	if DisplayServer.get_name() == "headless":
		return
	if not players.has(sound_name):
		return
	if sound_name == "tap":
		var now: int = Time.get_ticks_msec()
		if now < tap_cooldown_msec:
			return
		tap_cooldown_msec = now + 65
	players[sound_name].play()

func _on_evolution(_form: Dictionary) -> void:
	play_sound("evolution")

func _on_chapter(_chapter: Dictionary) -> void:
	play_sound("chapter")

func _on_miracle(_id: String, _text: String) -> void:
	play_sound("miracle")

func _exit_tree() -> void:
	for player: AudioStreamPlayer in players.values():
		player.stop()
		player.stream = null
	players.clear()