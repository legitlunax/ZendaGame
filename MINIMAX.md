# MiniMax AI Rules: Godot 4 & GenesisIdleRPG

You are an expert Godot 4 and GDScript 2.0 AI engineer interacting with the `GenesisIdleRPG` project.

## Context & Environment
- **Engine**: Godot Engine 4.7+ (x86_64 Windows)
- **Binary**: `E:\Godot\Godot_v4.7.2-stable_win64_console.exe`
- **Project Path**: `E:\Godot\GenesisIdleRPG`
- **MCP Server**: `E:\Godot\godot-mcp\build\index.js`
- **Verification Command**: `E:\Godot\check_project.bat`

## MiniMax Specific Guidelines
1. **Explicit Signatures**:
   - Always output full static types for variables, function parameters, and return types.
   - Do NOT use variant type inference `:=` on functions that return Variants. Write explicit types:
     `var max_val: float = floor(some_number)`
2. **Godot 4 Syntax Rules**:
   - Annotations: `@export`, `@onready`, `@tool`, `@rpc`.
   - Signals: `signal_name.emit()` and `signal_name.connect(callable)`.
   - Async: `await expression` (never use `yield`).
   - Clean file endings: Always finish scripts with a newline.
3. **MCP Usage with MiniMax**:
   - When asked to run or test scenes, call the `run_project` tool on `E:\Godot\GenesisIdleRPG`.
   - Inspect runtime errors using `get_debug_output`.
   - When modifying scenes, use `create_scene`, `add_node`, or `save_scene`.
4. **In-Engine MiniMax API Runtime**:
   - The game contains `scripts/ai/AIService.gd`, which supports calling MiniMax's Chat Completion V2 API (`abab6.5s-chat` / `MiniMax-Text-01`).
   - API keys are securely managed via `scripts/ai/AIConfig.gd` (`MINIMAX_API_KEY`).