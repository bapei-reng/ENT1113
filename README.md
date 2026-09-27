# ENT1113 — John Lemon's Haunted Jaunt

Unity course project for team development.

## Open the project

1. Clone this repository.
2. In Unity Hub, add the cloned repository folder as a project.
3. Open it with Unity **2022.3.8f1**.

The `com.unity.cinemachine@2.5.0` and `com.unity.postprocessing@2.1.2` folders are local packages referenced by `Packages/manifest.json`; keep them beside `Assets`, `Packages`, and `ProjectSettings`.

Unity generates `Library`, `Temp`, build outputs, and editor-specific files locally. They are excluded from Git.

## Gameplay

- `WASD` / arrow keys: move.
- `Shift`: sprint burst — movement speed triples for the first second, eases back to normal over the next second (15 second cooldown, shown in the HUD).
- `E`: pick up a nearby item. Hold `E` near an unlocked machine to decode.
- Arrow keys while decoding: the HUD shows one arrow key to hit. Hitting it speeds decoding up (×1.35 per hit, capped at ×4); hitting another arrow key slows it down (×0.78 per miss, floored at ×0.5). Either way the speed keeps drifting back toward ×1. Arrow keys stop moving John Lemon while he is decoding; `WASD` still works.
- `1`, `2`, `3`: select an inventory slot and the item in John Lemon's hand.
- `R`: insert the selected key while near the repair machine.
- `Esc`: quit the game (or stop Play Mode in the Editor).

The inventory holds three items. Find the three keys, insert one to unlock each decoding stage, and spend 15 seconds decoding that stage before inserting the next key. Decoding pauses when `E` is released or John Lemon leaves the machine. The exit opens after all three stages are complete; reaching it resets the level after 3 seconds, or immediately when you press `Enter`. Being caught restarts the level and its progress.

## Level flow

- Victory and defeat are independent paths in `GameEnding`: winning runs the victory sequence (`Victory Reset Delay` = 3 seconds, `Enter` skips it) and resets the level, while being caught runs its own fade/restart sequence.
- Numbers that must survive a victory reset go through `GameSession` (`SetFloat`, `GetFloat`, `SetInt`, `GetInt`, `Has`, `Remove`, `Clear` in `Assets/Scripts/GameSession.cs`). It is static, so its values live outside the scene and survive `SceneManager.LoadScene`.
- The caught path starts a fresh run and calls `GameSession.Clear()`, so only victory keeps cross-level values.
- `Ghost (3)` patrols from the room below the exit corridor into the corridor in front of the exit door (`WayPoint (10)` at (12.5, 0, 1.5) and `WayPoint (11)` at (16.6, 0, 1.5)). Ghosts path on the baked NavMesh in `Assets/Scenes/MainScene/NavMesh.asset`, so rebake the Navigation data after moving walls.

The Chinese HUD uses Noto Sans SC from Google Fonts. Its SIL Open Font License is included at `Assets/Fonts/OFL-NotoSansSC.txt`.
