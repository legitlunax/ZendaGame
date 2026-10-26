# ChatGPT & Cursor Project Instructions: Godot 4 (GenesisIdleRPG)

## Overview
This is a Godot 4.7+ Idle RPG project named `Genesis: Goddess of Creation` located at `E:\Godot\GenesisIdleRPG`.
Use GDScript 2.0 with strict static typing.

## Key Directives for ChatGPT / Cursor
1. **Never use Godot 3 Syntax**:
   - `export var` -> `@export var`
   - `onready var` -> `@onready var`
   - `yield(...)` -> `await ...`
   - `emit_signal("name", ...)` -> `signal_name.emit(...)`
   - `connect("name", self, "func")` -> `signal_name.connect(_on_func)`
   - `._ready()` -> `super._ready()`
2. **Type Safety & Warnings Treated as Errors**:
   - All code in this project treats warnings as errors.
   - Using variant inference `:=` on math functions (`floor`, `log`) or variant lookups causes parse failure!
   - ALWAYS use explicit typing:
     `var max_count: int = int(floor(val))`
     `var cost: float = float(data.get("cost", 0.0))`
3. **Architecture**:
   - `Database.gd` is registered as Autoload `DB`.
   - `GameManager.gd` is registered as Autoload `Game`.
   - `AIService.gd` and `AIConfig.gd` provide runtime calling of ChatGPT (`gpt-4o`, `gpt-4o-mini`) and MiniMax (`abab6.5s-chat`).
4. **Verification**:
   - To verify scripts without opening the full editor, run:
     `E:\Godot\check_project.bat`
   - It runs Godot in headless mode and reports any syntax or resource errors.
5. **Model Context Protocol (MCP)**:
   - The project is equipped with `godot-mcp` located at `E:\Godot\godot-mcp`.
   - When using Cursor or MCP-enabled tools, you can control the engine via `godot` MCP tools (`launch_editor`, `run_project`, `get_debug_output`, `create_scene`, `add_node`, etc.).