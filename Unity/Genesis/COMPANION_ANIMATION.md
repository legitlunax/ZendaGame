# Companion emotion animation

October 26, 2026. Energy means the existing 60-second Inspiration meter.

## Behavior

- Invoking Zenda resets the social clock and gives a 2.5-second happy welcome.
- 90 seconds without invocation on the focused Goddess view: sad.
- 180 seconds: playful angry/pouting expression.
- Reading, menus and app-unfocused time do not advance this social clock.
- Full Inspiration starts a four-second kiss celebration; the next refill gives
  a hug. Celebrations re-arm only below 80% Inspiration to avoid tap spam.
- Fairy motion uses the existing icon: hover, bounce, squash/stretch, tilt,
  expression badge, and hearts travelling toward Zenda. This is not a facial rig
  or a new sprite-sheet animation. Zenda uses existing VRM expressions and
  procedural arm poses. Exact hand-to-mouth placement varies with the avatar.
- Reactions are cosmetic. No neglect penalty, lost progress or bonus rewards.
- Companion tap opens history without Hope; a drag cancels selection.
- Selection and emotion state remain session-only.

## Verification

- E:\ZendaGame\Genesis\Tools\MCP\companion-emotion-smoke.json:
  1,938 checks passed, latest report 2026-10-26T17:57:21.8944514Z.
  Includes mood timing/synchronization, alternating affection, menu pause,
  animated bounds, synthetic pointer drag/tap and cosmetic-only checks.
- Story dialogue: 653 checks passed; illustrated-reader integration: 10 passed.
- E:\ZendaGame\Genesis\Builds\zenda-companion-affection.png visually reviewed:
  smiling badge, fairy and hearts visible beside Zenda.
- Removed story camera rendering inside OnGUI after URP re-entry exceptions.
  The remaining LateUpdate refresh produced no new render exceptions in the
  checked run. Console retained an unrelated Unity AI Account API warning.

## Open issues (not counted as passed)

- Existing companion suite failed its individual Death history assertion:
  the earlier Life-continuity addition mentions Animelle in Morthelia's history.
  This was not changed as part of companion animation.
- Latest story screenshot has an empty Zenda portrait panel in the mixed
  preview-scene setup, although the Lady and dialogue display. Reader geometry
  and navigation checks do not prove portrait rendering. Further investigation
  is needed; do not describe visual reader validation as fully passed.
- Physical-phone gestures, full animation playback acceptance, and avatar
  kiss/hug hand placement still need manual testing.
- Preserve the dirty preview scene; no scene was saved during these changes.