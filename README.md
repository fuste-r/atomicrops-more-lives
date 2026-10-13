# More Lives - Atomicrops Mod

A mod for **Atomicrops** that adds an extra lives and revivals system, configurable bonus starting health, an on-screen lives counter, and emergency revive protections.

---

## Features

- **Extra Revive Lives**:
  - Start each run with extra lives (default: **3**, fully configurable).
  - When you take fatal damage, instead of game over, an extra life is consumed.
  - Instantly revives on the spot:
    - Restores health (default: **100%**, configurable).
    - Grants **invincibility frames** and character blinking (default: **3.0s**).
    - Clears nearby enemy projectiles and knocks back surrounding enemies (`BreakBulletsAndPushEnemies`).
    - Displays a green floating notification: `★ EXTRA LIFE USED! (X remaining) ★`.
    - Plays a revival sound effect.
- **Bonus Max Health / Hearts**:
  - Configure additional starting hearts (`BonusMaxHealth`, default: 0). E.g. set to `2` to start with 6 hearts instead of 4.
  - Native integration with the Atomicrops heart HUD.
- **On-Screen Lives Counter (HUD)**:
  - Displays remaining lives in the upper-left HUD: `❤ Lives: 3` (or `❤ Lives: ∞` when infinite lives is active).
  - Configurable position, font size, and visibility.
- **Corpse-a-Copia Compatibility**:
  - By default, mod extra lives trigger before the vanilla Corpse-a-Copia flower (`UseExtraLivesBeforeCorpseACopia = true`). This protects your max HP from being reduced to 1 by the vanilla flower. If desired, you can toggle this off in the config.
- **In-Game Hotkeys**:
  - **`F7`**: Add +1 extra life immediately.
  - **`F8`**: Full heal (restores current health to max).
  - **`F9`**: Toggle infinite lives (invulnerability cheat) on/off.

---

## Installation

### Prerequisites
- [BepInEx 5 (x86)](https://github.com/BepInEx/BepInEx/releases) installed in your Atomicrops game directory.

### Quick Install
1. Download `MoreLives.dll` from the latest release.
2. Place `MoreLives.dll` into your `Atomicrops/BepInEx/plugins/` directory.

### Linux / Steam Deck / Proton Users (Important!)
Since Atomicrops runs through Wine/Proton on Linux, you must configure Proton to load the BepInEx `winhttp.dll` hook:

1. Open your **Steam** library.
2. Right-click **Atomicrops** -> **Properties...**
3. In the **General** tab, scroll down to **Launch Options**.
4. Set the Launch Options to:
   ```bash
   WINEDLLOVERRIDES="winhttp=n,b" %command%
   ```
5. Launch the game! BepInEx and More Lives will automatically load.

---

## Configuration

After launching the game once with the mod, a configuration file is generated at:
`Atomicrops/BepInEx/config/com.fuster.atomicrops.morelives.cfg`

You can edit this file to tweak any settings:

| Section | Setting | Default | Description |
|---|---|---|---|
| `[Lives]` | `StartingLives` | `3` | Number of extra revive lives at the start of each run. |
| `[Health]` | `BonusMaxHealth` | `0` | Additional max hearts given at start (0 = vanilla default). |
| `[Revive]` | `ReviveHealthPercent` | `100` | Percentage of max HP restored upon revive (10 to 100). |
| `[Revive]` | `InvincibilityDuration` | `3.0` | Seconds of invulnerability and blinking after revive. |
| `[Revive]` | `UseExtraLivesBeforeCorpseACopia` | `true` | Use extra lives before consuming Corpse-a-Copia flower. |
| `[Cheats]` | `InfiniteLives` | `false` | Enable infinite lives (cannot die). |
| `[HUD]` | `ShowLivesHUD` | `true` | Show or hide the lives counter on the HUD. |
| `[HUD]` | `HUDPositionX` | `20` | X position (pixels from left). |
| `[HUD]` | `HUDPositionY` | `85` | Y position (pixels from top). |
| `[HUD]` | `HUDFontSize` | `16` | Font size of the lives HUD label. |
| `[Hotkeys]` | `AddLifeHotkey` | `F7` | Key to add +1 extra life during gameplay. |
| `[Hotkeys]` | `RestoreHealthHotkey` | `F8` | Key to heal to full during gameplay. |
| `[Hotkeys]` | `ToggleInfiniteHotkey` | `F9` | Key to toggle infinite lives on/off. |

---

## Building from Source

To compile changes:
```bash
./build.sh
```
The script compiles the mod in `Release` configuration using `dotnet` and automatically deploys `MoreLives.dll` to your Atomicrops BepInEx plugins directory.
