# Storyline navigation

October 26, 2026.

- Story library is now Storyline: one current chapter, in catalog order, rather
  than a scrollable archive of every chapter.
- Current means the first chapter without a claimed reward. Reading does not
  advance it until the gift is received, preserving existing reward gating.
- Locked current chapters show their goal and link to Idle RPG.
- Completed storylines show a completion message and gallery guidance.
- Previous Lady histories remain at Lady gallery > select Lady > Full story.
  The reader's back button returns to that selected Lady; stories opened from
  Storyline return to Storyline. Replay does not grant duplicate rewards.
- Story content, unlock rules and companion behavior are unchanged.

## Verification

- E:\ZendaGame\Genesis\Tools\MCP\storyline-smoke.json: 229 checks passed;
  report E:\ZendaGame\Genesis\Builds\storyline-validation.txt timestamp
  2026-10-26T18:02:53.9009578Z. Includes chapter ordering, locked access,
  reward gating, replay, gallery return and all-chapters-complete state.
- Existing chronicle smoke: 18 checks passed in a separate fresh Play session.
- Console query after regression returned zero warning/error entries.
- Play stopped after testing. Dirty preview scene preserved without saving.
- These checks validate logic/navigation, not physical-phone layout or the
  previously documented blank reader portrait in the mixed preview scene.