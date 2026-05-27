# GAME2401 2026 Lecture Code — Log

## Session 1 — 2026-05-21

**Goal**: Simplify the week-3 lecture code for students (remove the event-bus/service indirection they couldn't follow)

**Accomplished**:
- Scaffolded .project
- authored spec 01 (student simplification) + phase tasks 001-004 (all done)
- stripped EventBus/ServiceLocator/MonoBehaviourService/LevelService/InputPublisher
- ported Singleton<T>
- built LevelSingleton (reads LevelData SO, spawns prefab grid, walkability, grid collection), InputSingleton (C# event OnMove), PlayerController (composition + IMover, grid collection by cell), PacmanMover + StepMover
- restored over-deleted LevelData/LevelLegend SOs
- fixed layout-parse crash + facing
- wrote README

**Decisions**:
- Singletons over bus/locator
- C# events (+=) over UnityEvents
- PlayerController + IMover composition with swappable mover (Pacman/Stepped)
- two movers as separate classes, no toggles
- legend 'S' spawns the stepped player
- grid collection folded into PlayerController (no Collector component, no trigger colliders)
- comments only where non-obvious

**Blockers**: None

**Next**: Open Level-Pacman / Level-Stepped in Unity, let it import the new .meta files, confirm a clean build, and verify pellets collect on cell entry

---
No sessions yet.
