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
- `E`: pick up a nearby item. Hold `E` near an unlocked machine to decode.
- `1`, `2`, `3`: select an inventory slot and the item in John Lemon's hand.
- `R`: insert the selected key while near the repair machine.
- `Esc`: quit the game (or stop Play Mode in the Editor).

The inventory holds three items. Find the three keys, insert one to unlock each decoding stage, and spend 15 seconds decoding that stage before inserting the next key. Decoding pauses when `E` is released or John Lemon leaves the machine. The exit opens after all three stages are complete. Being caught restarts the level and its progress.

The Chinese HUD uses Noto Sans SC from Google Fonts. Its SIL Open Font License is included at `Assets/Fonts/OFL-NotoSansSC.txt`.
