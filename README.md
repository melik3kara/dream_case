# Dream Games - Case Study

A simple, level-based mobile puzzle game built with Unity and C#. The goal is to clear all obstacles by blasting cubes and using rockets.

## Setup

1. **Open Unity Hub** → click "Open" → select this `dream_case` folder
2. Unity will import all assets and scripts automatically
3. **Create two scenes** in `Assets/Scenes/`:
   - `MainScene` — add an empty GameObject, attach the **MainMenuManager** script
   - `LevelScene` — add an empty GameObject, attach the **LevelSceneManager** script
4. **Add both scenes to Build Settings** (File → Build Settings → Add Open Scenes). Make sure **MainScene is index 0** (first in the list)
5. **Verify sprites**: Select any sprite in `Assets/Resources/Sprites/`, ensure Texture Type is set to "Sprite (2D and UI)" in the Inspector (this is the default for 2D projects)
6. **Play** — open MainScene and hit the Play button

## Editor Tool

Use **Dream Games → Set Level Number** in the Unity menu bar to manually set the current level (useful for testing).

## Project Structure

```
Assets/
  Scripts/
    Core/         GameManager.cs, SpriteLoader.cs
    Data/         LevelData.cs
    Board/        Board.cs, ItemBase.cs, CubeItem.cs, RocketItem.cs, ObstacleItem.cs
    UI/           MainMenuManager.cs, LevelSceneManager.cs, FailPopup.cs, CelebrationManager.cs
    Editor/       LevelEditorMenu.cs
  Resources/
    Levels/       level_01.json — level_10.json
    Sprites/      All game sprites organized by type
  Scenes/         MainScene, LevelScene
ProjectSettings/
Packages/
```

## Gameplay

- **Tap** a group of 2+ same-color cubes to blast them
- Blasting **4+ cubes** creates a **Rocket** at the tapped position
- **Tap a Rocket** to explode it — it splits into two parts that damage everything in their path
- Adjacent rockets combine into a **Combo** (3x3 cross explosion)
- Clear all **obstacles** (boxes, stones, vases) within the given move count to win
