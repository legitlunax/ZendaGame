# GitHub Copilot & OpenAI Codex Instructions for Godot 4

## Project & Framework Context
- Project: `GenesisIdleRPG`
- Engine: Godot 4.7+
- Language: GDScript 2.0 (strict typing)
- Console executable: `E:\Godot\Godot_v4.7.2-stable_win64_console.exe`
- Verification script: `E:\Godot\check_project.bat`

## GDScript 4 Conventions
- **Static Typing**: Always annotate function parameters, return types, and variables:
  ```gdscript
  func add_quanta(amount: float) -> void:
      var total: float = quanta + amount
  ```
- **Avoid Variant Inference Trap**: Do NOT use `:=` with `floor()`, `log()`, `Dictionary.get()`, or functions returning Variant. Always use explicit type `var val: float = ...` or `var val: int = int(...)`.
- **Annotations**: Use `@export`, `@onready`, `@tool`, `@rpc`.
- **Signals**: Use `my_signal.emit()` and `my_signal.connect(my_callable)`. Never use Godot 3 `emit_signal(...)` or `connect("name", self, "func")`.
- **Coroutines**: Use `await get_tree().create_timer(1.0).timeout`. Never use `yield`.
- **Lambdas**: Use typed lambdas `func() -> void: ...`.
- **Node Access**: Prefer `@onready var node: Type = $Path` or `%UniqueName`.
- **Scene Files (.tscn)**: Maintain valid Godot 4 `.tscn` format. Do not alter node references or external resource IDs arbitrarily.

## Project Structure
- `scripts/Database.gd` (Autoload `DB`): Static configs for generators, relics, and miracles.
- `scripts/GameManager.gd` (Autoload `Game`): Main logic, currency, progression, and saving.
- `scripts/CosmicVisualizer.gd`: Procedural 2D particle & cosmetic drawing.
- `scripts/Main.gd`: UI logic and event listeners.
- `scripts/ai/AIService.gd`: Integrated runtime AI client for ChatGPT and MiniMax.
- `scripts/ai/AIConfig.gd`: Safe API key loader from environment variables.