# Week 3 — Grid Game

A small grid-based game (Pac-Man-ish) kept deliberately simple. The level is text, shared things
are singletons, input is a C# event, and the player is built from small components.

## Patterns used

- **Singleton** (`Core/Patterns/Singleton.cs`) — a base class that gives one shared instance via
  `Instance`. `LevelSingleton` and `InputSingleton` use it. This replaces a service locator: there's
  one obvious way to reach the level or the input.
- **C# event for input** — `InputSingleton.OnMove` is an `event Action<Vector2Int>`. Anything that
  cares subscribes with `+=` and unsubscribes with `-=`. No central event bus.
- **Composition + interface** — the player is `PlayerController` + a mover. `PlayerController` reads
  input and forwards it to whichever mover is attached. Movers implement `IMover`, so the controller
  doesn't care if it's `PacmanMover` or `StepMover`.
- **ScriptableObject for data** — a level is a `LevelData` asset (a text maze + a `LevelLegend`).
  Editing levels never touches code.

## Code map

```
Core/Patterns/Singleton.cs           generic single-instance base
Gameplay/Input/InputSingleton.cs     reads input, raises OnMove
Gameplay/Level/LevelSingleton.cs     builds the grid from LevelData, spawns tiles, tracks collectables
Gameplay/Level/LevelData.cs          ScriptableObject: the maze text + legend
Gameplay/Level/LevelLegend.cs        ScriptableObject: symbol -> prefab + walkable
Gameplay/Player/PlayerController.cs  forwards input to the mover; collects by grid cell
Gameplay/Player/IMover.cs            contract: OnMoveInput(direction)
Gameplay/Player/PacmanMover.cs       glide until a wall, buffered turns
Gameplay/Player/StepMover.cs         one cell per press
```

## How a level is built

`LevelSingleton` reads its assigned `LevelData`, splits the text into rows, and for each character:
- marks the cell walkable or not (from the legend), and
- spawns the legend's prefab for that character at the cell's world position.

Cell -> world is `GridToWorld`: `x` goes along +X, rows go into -Z (so the world matches the text).

## Legend symbols

Edit `ScriptableObjects/GameData/Legend.asset`. Each entry maps a character to a prefab and a
walkable flag. Current symbols:

| Symbol  | Spawns          | Walkable |
|---------|-----------------|----------|
| `#`     | wall (Block)    | no       |
| `P`     | Player (Pacman) | yes      |
| `S`     | PlayerStepped   | yes      |
| `G`     | Ghost           | yes      |
| `.`     | Collectable     | yes      |
| (space) | nothing (floor) | yes      |

## P vs S (the two movement styles)

The two movers are separate components, not a toggle:
- **Pac-Man** — `Player.prefab` = `PlayerController` + `PacmanMover`. Keeps gliding until a wall;
  a new direction takes effect at the next cell.
- **Stepped** — `PlayerStepped.prefab` = `PlayerController` + `StepMover`. One cell per key press.

Which one spawns is just which letter is in the maze: `P` spawns the Pac-Man player, `S` spawns the
stepped player. `Level-Pacman` uses `P`; `Level-Stepped` uses `S`.

## Collection (grid-based, no trigger colliders)

`LevelSingleton` remembers every `.` it spawned, keyed by cell. `PlayerController` checks which cell
it's on each frame; when it moves onto a new cell it calls `LevelSingleton.TryCollect(cell)`, which
removes the collectable there. Pure grid coordinates — no physics triggers.

## Setting things up

**Run a demo:** open `Scenes/Levels/Level-Pacman` or `Level-Stepped` and press play. Each scene has a
`LevelSingleton` (with its level assigned) and an `InputSingleton`.

**Make a new level:**
1. Create -> `Game/Level Data`. Type the maze in the layout box (one line per row).
2. Assign the `Legend` asset to it.
3. In a scene, set the `LevelSingleton` component's `Level Data` field to your new asset. Add an
   `InputSingleton` to the scene too (or it auto-creates on first use).
4. Put one `P` **or** one `S` in the maze for the player, `#` for walls, `.` for pellets, `G` for ghosts.

**Change cell spacing:** `LevelSingleton._cellSize` (world distance between cells).

**Add a new tile type:** make a prefab, add a legend entry mapping a character to it, then use that
character in a layout.
