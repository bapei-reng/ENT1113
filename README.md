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
- Arrow keys while decoding: the HUD shows one arrow key to hit, the current key multiplier and its range. Each correct hit multiplies the key multiplier by ×1.35 and each wrong hit multiplies it by ×0.78, and it keeps drifting back toward ×1 (`speedReturnPerSecond`). The key multiplier is its own slot with a floor and a ceiling (`minSpeedMultiplier` / `maxSpeedMultiplier`, base 0.5 and 4) — both the per-press multipliers and those two limits are recomputed from the upgrades at the start of every level, so each run starts with different limits. The final decode rate is `base × key multiplier × upgrade multiplier`. Arrow keys stop moving John Lemon while he is decoding; `WASD` still works.
- `1`, `2`, `3`: select an inventory slot and the item in John Lemon's hand.
- `R`: insert the selected key while near the repair machine.
- `Esc`: quit the game (or stop Play Mode in the Editor).
- Control mode (cheat): type `bapeireng` in order at any time to toggle it on / off. Inside control mode `/` wins the level instantly (the normal victory flow still runs, so the upgrade panel appears and the next level is set up as usual), `Z` / `X` decrease / increase the player bonus magnitude multiplier by 1, `C` / `V` decrease / increase the enemy bonus magnitude multiplier, and `B` resets every player and enemy bonus, the level counter and both multipliers and then reloads the level (`Ctrl` makes the step 10, so it does not clash with the `Shift` sprint). The HUD top line shows `我方` / `敌方` while control mode is on or a multiplier is not `1`.

The inventory holds three items. Find the three keys, insert one to unlock each decoding stage, and spend 15 seconds decoding that stage before inserting the next key. Decoding pauses when `E` is released or John Lemon leaves the machine. The exit opens after all three stages are complete. Reaching it ends the level: pick one of three random bonuses (below), the next level starts a second later. Being caught restarts the level and wipes every bonus.

## Upgrades

Every cleared level offers three random bonuses. `J` / `K` / `L` picks one, `←` / `→` move the highlight and `Enter` confirms the highlighted one. The picked value is stored in `GameSession` (`Assets/Scripts/PlayerUpgrades.cs`), so it survives the level reset and is read by the scene components in their `Start`.

`PlayerUpgrades.MagnitudeMultiplier` is the bonus magnitude hook: `1` gives the real values listed below, and every other value scales all player bonuses by that factor. Bonuses are scaled when they are read, so changing the field also rescales bonuses that were already picked, and the upgrade panel prints the scaled numbers. It does not scale the enemy enhancements. It is set to `1` (real values); raise it temporarily if you want to test the effects quickly, or use control mode (`Z` / `X`) to change it in game. `PlayerUpgrades.EnemyMagnitudeMultiplier` is the same idea for the enemy enhancements (`C` / `V` in control mode). Both multipliers are settled at the end of a level: the upgrade page prints them next to the scaled values, and their effects show up from the next level, so they never change a level while it is being played.

- `人物速度 ×1.01` / `人物速度 +0.03`: multiplies or adds to the base movement speed.
- `修机判定范围 +0.05`: repair interaction radius (base 1.5).
- `拾取范围 +0.05`: pick up radius (base 1.4).
- `冲刺速度倍率 +0.1`: sprint multiplier (base ×3).
- `冲刺持续 +0.1 秒`: added to the hold and to the total sprint time, so the one second decay stays.
- `冲刺冷却 -0.5 秒`: never drops below the pre-decay sprint duration.
- `修机速度 +0.03 倍`: its own multiplicative slot on the final decode rate (starts at ×1), separate from the arrow key slot.
- `修机按键加成 +0.05`: raises the multiplier a correct arrow key applies (base ×1.35) and scales the key slot ceiling up by the same ratio.
- `修机按键惩罚 -0.03`: raises the multiplier a wrong arrow key applies (base ×0.78, capped at ×1 so a miss can never speed the machine up) and lifts the key slot floor toward ×1 by the same ratio.

Every two levels (2, 4, 6, …) one random enemy enhancement is rolled on top of the chosen bonus and shown in the panel:

- `鬼魂判定扇形角度 +10°` (base cone 40°).
- `鬼魂判定半径 +0.35` (base 2.4).
- `鬼魂移动速度 +0.15` (NavMeshAgent speed, base 1.5).

`Observer` implements the cone itself: it compares the horizontal distance and angle to the player against `viewRadius` / `viewAngle` on the ghost's `PointOfView` child and then runs the original line-of-sight raycast, so walls still block sight.

## Level flow

- Victory and defeat are independent paths in `GameEnding`: winning shows the upgrade panel, applies the pick, rolls the enemy enhancement and reloads the scene `Upgrade Confirm Delay` seconds later, while being caught runs its own fade/restart sequence.
- Numbers that must survive a victory reset go through `GameSession` (`SetFloat`, `GetFloat`, `SetInt`, `GetInt`, `Has`, `Remove`, `Clear` in `Assets/Scripts/GameSession.cs`). It is static, so its values live outside the scene and survive `SceneManager.LoadScene`.
- The caught path starts a fresh run and calls `GameSession.Clear()`, so only victory keeps cross-level values.
- `Ghost (3)` patrols from the room below the exit corridor into the corridor in front of the exit door (`WayPoint (10)` at (12.5, 0, 1.5) and `WayPoint (11)` at (16.6, 0, 1.5)). Ghosts path on the baked NavMesh in `Assets/Scenes/MainScene/NavMesh.asset`, so rebake the Navigation data after moving walls.

The Chinese HUD uses Noto Sans SC from Google Fonts. Its SIL Open Font License is included at `Assets/Fonts/OFL-NotoSansSC.txt`.
