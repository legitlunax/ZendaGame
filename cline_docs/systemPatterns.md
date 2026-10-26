# System Patterns

## Architectural Patterns
1. **Autoload Singletons**:
   - `DB` (`Database.gd`): Invariant data definitions and formatting functions.
   - `Game` (`GameManager.gd`): Dynamic state, game loop, autosave, offline progression.
2. **Signal-Driven UI**:
   - `Main.gd` listens to `Game.state_updated`, `Game.goddess_evolved`, `Game.miracle_triggered`.
   - Decoupled view pattern: Visualizer and UI never mutate data directly; they dispatch through `Game`.
3. **AI Services**:
   - `AIService.gd`: Manages HTTP requests and callbacks for ChatGPT / MiniMax.
   - `AIConfig.gd`: Loads credentials securely without hardcoded secrets.