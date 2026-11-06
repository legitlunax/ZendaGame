# World storyline gates

Before the first creation of a new world can be purchased, all chapters in earlier worlds must be fully read. Existing worlds and their upgrades are unaffected. The first world has no reading prerequisite.

The Gather button displays `Read story / Before unlock` when blocked, even when the purchase is unaffordable. Selecting it opens the earliest unread required chapter. The reader requires its normal final-page `Finish their story` confirmation; opening a chapter or reaching its last page does not complete it. Chapters continue in order, then `Return / unlock next world` returns to Gather. Unlocking remains a separate purchase. Leaving early never bypasses the gate.

If a required chapter's objective is not yet met, the Storyline view identifies it and offers a return to gameplay. Gifts need not be claimed. Reading completion is stored as the optional `read_stories` field in the existing save format. Legacy claimed chapters count as read; legacy unclaimed chapters must be read. Imported progress is not revoked, but future world formation checks all preceding histories.

Enforcement is in `GenesisGame.Buy`, not just the UI. The Sanctum wrapper adds navigation without charging or purchasing on a blocked attempt.

## Gathering follows the current world

Gather shows the newest formed world's discovered fairies, not the accumulated fairies of all earlier worlds. Once available, the following world's first fairy appears as the next-world unlock entry. A successful unlock switches Gather to the new world and resets its scroll position. The header shows that world's name, icon and owned-fairy progress. Returning from required reading focuses the unlock entry using its position in this world-specific list.

Earlier-world levels and automatic production are preserved off-page. Browsing old worlds in Worlds, choosing a journey origin, or changing tabs does not move the active gathering list backward. The current gathering world is derived from owned creations, so save restoration needs no additional active-world field.

Validation scripts:

- `E:\ZendaGame\Genesis\Tools\MCP\world-story-gate-smoke.json`: domain gates, save/restore, compatibility and gift independence.
- `E:\ZendaGame\Genesis\Tools\MCP\world-story-navigation-smoke.json`: reader ordering, explicit finish, redirects, return and purchase across all 29 transitions.
- `E:\ZendaGame\Genesis\Tools\MCP\active-world-gathering-smoke.json`: run in Play mode; validates all 29 transitions, world filtering, local unlock scroll, automatic switching, old-world income, map/tab isolation and restoration.

Reports are written under `E:\ZendaGame\Genesis\Builds`. Automated checks do not replace visual or physical-device acceptance.