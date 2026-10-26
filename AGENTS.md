# Godot 4 / GenesisIdleRPG — Principal Engineering & AI Rules

## 1. Environment & Fast-Path Verification
Always use explicit binary paths:
- **Godot Console Executable**: `E:\Godot\Godot_v4.7.2-stable_win64_console.exe`
- **Godot GUI Executable**: `E:\Godot\Godot_v4.7.2-stable_win64.exe`
- **MCP Server**: `E:\Godot\godot-mcp\build\index.js` (Run with `node` and `GODOT_PATH` set)
- **Fast Syntax & Scene Verification**:
  ```cmd
  E:\Godot\check_project.bat
  ```
  Or in PowerShell:
  ```powershell
  & "E:\Godot\Godot_v4.7.2-stable_win64_console.exe" --headless --path "E:\Godot\GenesisIdleRPG" --editor --quit
  ```
  Run this after modifying any `.gd`, `.tscn`, or `.godot` file. It must exit cleanly with code `0`.

## 2. Module & Architecture Map
| Feature / Layer | Primary Script / Scene | Description | Fast Check |
| :--- | :--- | :--- | :--- |
| **Project Config** | `GenesisIdleRPG/project.godot` | Godot 4.7 config, viewport (1280x720), autoloads | Check headless |
| **Static Data** | `GenesisIdleRPG/scripts/Database.gd` | Autoload `DB`. Generators, Relics, Miracles, number formatting | `check_project.bat` |
| **Game State & Logic**| `GenesisIdleRPG/scripts/GameManager.gd` | Autoload `Game`. Quanta economy, offline earnings, ticks | `check_project.bat` |
| **Canvas FX** | `GenesisIdleRPG/scripts/CosmicVisualizer.gd` | 2D canvas drawing procedural stars, black holes, effects | `check_project.bat` |
| **Main UI Controller**| `GenesisIdleRPG/scripts/Main.gd` | HUD controller, UI buttons, popup notifications, tabs | `check_project.bat` |
| **Main Scene** | `GenesisIdleRPG/scenes/Main.tscn` | Root `Control` node linking UI and `Main.gd` | `check_project.bat` |
| **AI Runtime Integration** | `GenesisIdleRPG/scripts/ai/AIService.gd` | In-game client for OpenAI ChatGPT and MiniMax APIs | `check_project.bat` |
| **AI Credentials** | `GenesisIdleRPG/scripts/ai/AIConfig.gd` | Environment variable & local credential manager | `check_project.bat` |

## 3. GDScript 4 Invariants & Coding Standards
1. **Static Typing & Warning Enforcement**:
   - Explicit types for all declarations (`var count: int = 0`, `func foo() -> void:`).
   - In Godot 4, variant inference warnings are treated as errors. Avoid `:=` when the RHS evaluates to Variant (e.g. `floor()`, `log()`, dictionary `.get()`). Use `var n: float = ...` or `var n: int = int(...)`.
2. **Modern GDScript 4 Keywords**:
   - Use `@export`, `@onready`, `@rpc`, `@tool` (Never Godot 3 `export var`, `onready var`).
   - Use `signal.emit()` and `signal.connect()` (Never `emit_signal` or string-based `connect`).
   - Use `await` for coroutines and timers (Never `yield`).
   - Use `super._ready()` (Never `._ready()`).
3. **No BOM Artifacts**:
   - All text files (`.gd`, `.godot`, `.tscn`) must be clean UTF-8 without BOM (`EF BB BF`).
4. **Node Tree & Memory Management**:
   - Always free dynamic nodes with `.queue_free()`.
   - Use `@onready var btn: Button = $VBoxContainer/Button` or Unique Names `%Button`.

## 4. MCP Tools
When connected via Model Context Protocol (`godot-mcp`), use the following tools:
- `launch_editor`: Open Godot editor targeting `E:\Godot\GenesisIdleRPG`.
- `run_project`: Run Godot project to test features and collect runtime errors.
- `get_debug_output`: Inspect debug and crash logs.
- `stop_project`: Safely close the game process.
- `create_scene`, `add_node`, `save_scene`: Automated scene creation.