# 2D Top-Down Survivor Shooter

## Project Overview

This project is a small 2D top-down survivor shooter developed in Unity for the Game Programming module.

The game is inspired by the core loop of survivor-style games: the player moves around an arena, automatically shoots toward the mouse direction, defeats enemies, collects experience gems, chooses upgrades, and tries to survive a sequence of enemy waves.

The aim of the project is to create a focused vertical slice rather than a large unfinished game. The final result demonstrates a complete playable loop with movement, combat, enemy behaviour, experience progression, upgrades, wave-based pacing, player feedback, camera movement, and map boundaries.

## Game Goal

The player must survive 10 waves of enemies.

During each wave, enemies spawn around the arena and chase the player. Defeating enemies drops experience gems. Collecting enough experience causes the player to level up and choose one of three random upgrades. The player wins after surviving the final wave and clearing the remaining enemies.

## Controls

| Action | Control |
|---|---|
| Move | `WASD` or Arrow Keys |
| Aim | Mouse position |
| Shoot | Automatic, no mouse click required |
| Choose upgrade | Click one of the upgrade buttons |

## How to Run

1. Clone or download this repository.
2. Open the project in Unity `2022.3.62f3c1` or a compatible Unity 2022.3 LTS version.
3. Open the scene:
   - `Assets/Scenes/SampleScene.unity`
4. Press Play in the Unity Editor.

## Main Features

### Player Movement and Shooting

- The player moves with keyboard input.
- Shooting is automatic.
- The mouse controls the shooting direction.
- Bullets are fired from a `FirePoint` object and keep their direction after spawning.
- A small aim assist system can slightly adjust shots toward nearby enemies.

### Enemy Behaviour

- Enemies use simple chase behaviour and move toward the player.
- Enemies deal contact damage with a cooldown so the player does not lose all health instantly.
- Three enemy tiers are supported:
  - `EnemyTier1`
  - `EnemyTier2`
  - `EnemyTier3`
- Later waves introduce stronger enemy types.

### Experience and Upgrades

- Enemies drop experience gems when defeated.
- The player collects experience by touching gems or by using pickup radius.
- Level-up pauses the game and displays three upgrade choices.
- Implemented upgrades include:
  - Fire rate increase
  - Multishot
  - Giant bullets
  - Experience pickup range
  - Max health increase

### Wave System

- The game contains 10 waves.
- Each wave has a time limit.
- Enemy types and spawn pressure increase over time.
- Spawn frequency also scales gently with player level.
- Enemy count is capped so the game does not become an uncontrolled full-screen swarm.
- At the start of a new wave, the player's health is restored.

### Camera and Map Boundaries

- The camera follows the player with a zoomed-in view.
- The player sees only part of the map at one time.
- Player movement, enemy movement, and enemy spawn positions are limited to the playable map area.
- The camera is intentionally not clamped to the map edge, so the view may show outside the map near the boundary.

### Player Feedback

- The player flashes red briefly when taking damage.
- The HUD displays health, player level, experience, wave number, and wave timer.
- Level-up and wave-start states are displayed through UI prompts.

## Key Scripts

| Script | Purpose |
|---|---|
| `PlayerController.cs` | Handles player movement, aiming, shooting, health, experience, level-up, upgrades, and hit feedback. |
| `Enemy.cs` | Handles enemy movement, contact damage, health, death, and experience drops. |
| `WaveManager.cs` | Controls wave timing, enemy spawning, enemy tiers, wave UI, health restoration, difficulty scaling, and victory. |
| `UpgradeManager.cs` | Listens for level-up events, pauses the game, shows the upgrade UI, applies upgrades, and resumes gameplay. |
| `UpgradePopupUI.cs` | Creates and displays the three-choice upgrade popup. |
| `ExperienceGem.cs` | Handles experience gem visuals, pickup range detection, collision pickup, and experience reward. |
| `GameBounds2D.cs` | Provides shared map boundary clamping for the player, enemies, and spawns. |
| `CameraFollow2D.cs` | Makes the main camera follow the player and applies the zoomed-in orthographic view. |
| `DirectionalSprite2D.cs` | Reusable left/right sprite switching for player and enemies. |
| `Bullet.cs` | Handles bullet movement, damage, collision with enemies, and lifetime. |

## Design Decisions

### Realistic Scope

The project focuses on a small but complete vertical slice. Instead of attempting a large game with many unfinished systems, the goal was to create one coherent gameplay loop that can be played, understood, and demonstrated clearly.

### Automatic Shooting

Automatic shooting was chosen to match the survivor-shooter style. This lets the player focus on movement, positioning, aiming direction, and upgrade choices rather than repeatedly clicking.

### Separate Wave and Level Progression

Player level and wave number are separate systems:

- Player level is based on collected experience.
- Wave progression is based on time survived.

This separation makes the game easier to understand and avoids confusing level-ups with wave transitions.

### Simple Enemy AI

Enemies use direct chase behaviour instead of complex pathfinding. This is appropriate for the project scope and creates clear pressure on the player without adding unnecessary complexity.

### Controlled Difficulty Scaling

Spawn rate increases gently with player level, but the maximum number of enemies is capped. This keeps the game from becoming too difficult or visually overcrowded, especially in later waves.

## Development and Testing Notes

During development, several issues were identified and improved through testing:

- Bullet direction was corrected so shots use the `FirePoint` direction and keep a fixed direction after spawning.
- Player and enemy left/right sprite switching was adjusted.
- Wave UI and player level UI were separated to avoid confusion.
- Player health restoration at the start of new waves was simplified and centralized through `WaveManager`.
- Early spawn pressure was reduced so level 1 is less overwhelming.
- Map boundaries were added so the player and enemies cannot leave the playable area.
- Player hit feedback was added to make damage more readable.

GitHub Issues and labels were also used to organise completed work and show the current project management state.

## Assets and Credits

### Image Assets

The following core image assets were generated using Google Gemini:

- `Assets/Art/player1.png` - player character sprite sheet
- `Assets/Art/enemy_basic1.png` - enemy sprite sheet
- `Assets/Art/ground_tile.png` - map / ground image
- `Assets/Art/bullet1.png` - bullet image

These images were generated specifically for this student project and then imported, sliced, configured, and integrated in Unity.

### Runtime-Generated Asset

- The experience gem visual is generated at runtime in `ExperienceGem.cs` as a simple circular sprite.

### Unity Assets

The project uses Unity built-in components and standard Unity 2D systems, including:

- `Rigidbody2D`
- `Collider2D`
- `SpriteRenderer`
- `Camera`
- Unity UI components

## AI and Tool Use Disclosure

Google Gemini was used to generate the main 2D image assets listed above.

AI coding assistance was also used during development for debugging support, code suggestions, and project organisation. The game design choices, feature selection, Unity integration, playtesting, balancing decisions, and final implementation decisions were reviewed and directed by myself.

## Legal, Ethical, Social, Accessibility, and Security Considerations

- The core image assets were AI-generated for this project rather than copied from commercial games.
- The project does not collect player data or use online services during gameplay.
- The game uses simple keyboard and mouse controls.
- The current prototype does not yet include full accessibility options such as remappable controls, colour-blind modes, or audio/visual accessibility settings.
- Future improvements could include clearer UI scaling, remappable controls, more readable colour choices, and sound volume controls.

## Known Limitations

- The game currently uses simple prototype UI.
- Audio feedback is not yet implemented.
- Enemy AI is intentionally simple and does not use pathfinding.
- Some balance values are still stored directly in script/Inspector fields rather than ScriptableObjects.
- The camera can show outside the map near the boundary by design.

## Future Improvements

- Add sound effects for shooting, enemy hits, experience pickup, level-up, and victory.
- Add enemy hit flash and death effects.
- Improve upgrade UI presentation.
- Add a main menu, pause menu, and clearer victory/defeat screens.
- Move balance data into ScriptableObjects.
- Add more enemy behaviours and upgrade types.
- Add more accessibility options.

## Repository

GitHub repository:

https://github.com/lolollop/GameProgramming
