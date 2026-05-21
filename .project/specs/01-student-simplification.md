---
milestone_ref: ""
notion_id: ""
---

# Spec 01 — Student simplification

## Overview

The week-3 lecture code carries the full studio architecture — `EventBus`, `ServiceLocator`, `MonoBehaviourService`, `GameInitializer`, `LevelService`, `InputPublisher`. In class, students couldn't follow the event bus or the service indirection. This spec rebuilds the same gameplay (grid, input, movement) with a flat, readable structure students can hold in their heads, every file heavily commented.

The replacement architecture has three ideas only:
1. **Singletons** for shared access — `LevelSingleton` and `InputSingleton` (ported `Singleton<T>` base from bvc-ggj). `.Instance` replaces the service locator.
2. **Unity events** for messaging — `InputSingleton` broadcasts `UnityEvent`s; movers subscribe. No code event bus.
3. **Plain MonoBehaviours** for movers — two separate classes, no inheritance gymnastics, no mode toggles.

Project: `game2401-2026-lecture-code/week-3` (Unity). Current scripts under `Assets/_Project/Code/`.

## Decisions

1. **Strip the bus + service layer** — delete `EventBus`, `EventBusSubscriber`, `IEvent`/`IEventBus`, `ServiceLocator`, `IService`/`IMonoBehaviourService`, `MonoBehaviourService`, `GameInitializer`, `PersistentServiceInitializer`, `LevelService`/`ILevelService`, `GameSceneInitializer`, `InputPublisher`, `LevelLoadedEvent`. `.Instance` on singletons is the only global access pattern.
2. **Port `Singleton<T>`** from bvc-ggj (`Assets/_Project/Code/Core/Patterns/Singleton.cs`) — generic `MonoBehaviour` base with a static `Instance`. Used by both `LevelSingleton` and `InputSingleton`.
3. **Input is a broadcasting singleton** — `InputSingleton` reads `PlayerInputActions` and raises `UnityEvent<Vector2Int> OnMove`. Movers subscribe in code: `InputSingleton.Instance.OnMove.AddListener(...)`. The deliberate teaching contrast to the old code-level event bus.
4. **Two grid-build approaches, both labeled** — (a) parse a multi-line string maze into grid data; (b) spawn a grid of prefab tiles in code with clearly-labeled positioning/size/spacing math. Both driven by `LevelSingleton`.
5. **Two movers, separate classes, no toggles** — `PacmanMover` (glide in current direction until a wall, turn when able) and `StepMover` (one cell per press, then stop). Each is a self-contained MonoBehaviour that subscribes to `InputSingleton.OnMove` and queries `LevelSingleton`.
6. **No rhythm/Conductor** — out of scope; keep it simple.
7. **Comment everything** — every class and non-trivial method gets a plain-language comment for a student reading it cold. Inline notes on the grid math and the walkability check especially.

## Phase A: Singleton base + strip old architecture

1. Add `Assets/_Project/Code/Core/Patterns/Singleton.cs` — port the generic `Singleton<T>` from bvc-ggj. Comment how `Instance` works (lazy find/create) and what `Awake` does (claim or destroy duplicate). Default `PersistBetweenScenes` to `false` for a single-scene teaching project, with a comment on the trade-off.
2. Delete the stripped files listed in Decision 1. Confirm nothing else references them (movers/input/level are rewritten in later phases).
3. Keep `PlayerInputActions.cs` (generated Input System wrapper). Keep `LevelData`/`LevelLegend`/`Tile`/`LegendEntry` only if reused by the new `LevelSingleton`; otherwise fold the needed bits into the singleton to cut file count. Decide during implementation; bias toward fewer files.
4. `MoveEvent` (`InputEvents.cs`): replace with a plain `Vector2Int` carried by the UnityEvent — no event struct. Remove `InputEvents.cs` if nothing else uses it.

## Phase B: LevelSingleton

1. Create `Assets/_Project/Code/Gameplay/Level/LevelSingleton.cs` — `LevelSingleton : Singleton<LevelSingleton>`. Holds the grid (`bool[,]` walkable or `char[,]` symbols) and exposes `bool IsWalkable(int x, int y)` and `Vector3 GridToWorld(Vector2Int cell)` with bounds checks.
2. **Build approach 1 — string layout.** A serialized multi-line string maze parsed into the grid. Clearly-labeled method `BuildFromLayout(string)`. Comment the row/column-to-cell mapping.
3. **Build approach 2 — prefab grid in code.** A method `SpawnTileGrid()` taking a simple tile prefab + width/height + cell size, instantiating a grid via nested `for` loops. Heavily comment the positioning math: `worldPos = origin + new Vector3(x * cellSize, 0, -y * cellSize)`, and how size/spacing are derived. This is the "grid positioning, size" teaching piece.
4. Keep both approaches visible and switchable by a simple serialized enum or two clearly-named public methods — NOT a hidden toggle; the point is students read both.

## Phase C: InputSingleton

1. Create `Assets/_Project/Code/Gameplay/Input/InputSingleton.cs` — `InputSingleton : Singleton<InputSingleton>`. Holds a `PlayerInputActions` instance; enable in `OnEnable`, disable in `OnDisable`.
2. Expose `public UnityEvent<Vector2Int> OnMove;`. On the Move action's `performed`, convert the raw `Vector2` to a cardinal `Vector2Int` (reuse the `ToCardinal` logic from the lecture `AltMoveInput` stub, with the corrected y-mapping) and `OnMove.Invoke(direction)`.
3. Comment the broadcast model: "anything can subscribe with `InputSingleton.Instance.OnMove.AddListener(...)`; no central bus."

## Phase D: PacmanMover + StepMover

1. Create `Assets/_Project/Code/Gameplay/Player/PacmanMover.cs` — subscribes to `InputSingleton.Instance.OnMove` in `Start`, stores a desired direction, glides cell-to-cell (lerp), and at each cell turns to the desired direction if walkable, else continues straight until blocked. Queries `LevelSingleton.Instance.IsWalkable`. Commented step by step.
2. Create `Assets/_Project/Code/Gameplay/Player/StepMover.cs` — subscribes to `OnMove`; on each input, if the adjacent cell is walkable, move exactly one cell (lerp or snap) and stop. No momentum, no buffering. Commented step by step.
3. Both are independent MonoBehaviours on the player prefab (enable one to switch styles). No shared base, no toggle field — the two classes ARE the two styles.

## Edge Cases

- **Singleton access order** — a mover's `Start` calls `LevelSingleton.Instance` / `InputSingleton.Instance`. The lazy `Instance` getter creates the singleton if absent, so order is safe, but prefer placing both singletons in the scene so students see them. Comment this.
- **Duplicate singletons in scene** — `Awake` destroys the extra; comment it so students aren't surprised when a second one vanishes.
- **Layout string vs prefab grid mismatch** — if both build approaches run, they must agree on dimensions/origin. Pick one active per scene; document that running both is for comparison only.
- **Input not enabled** — if `PlayerInputActions` isn't enabled, `OnMove` never fires; the `OnEnable`/`OnDisable` pair must be present. Comment it.
- **Walkability bounds** — `IsWalkable` must return false outside the grid, not throw. Guard and comment (same bug class that bit the parent project).
