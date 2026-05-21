# Interview state — 01-student-simplification

## Asked & answered
- Q: Architecture removal scope? A: Strip EventBus + ServiceLocator + service layer. LevelSingleton.Instance is the access pattern. Input via UnityEvents. Movers reference LevelSingleton.Instance directly. (No global bus, no locator, no MonoBehaviourService/GameInitializer/InputPublisher.)
- Q: Grid-build approaches? A: TWO ways: (1) string layout parsed into grid data (maze text), (2) code that spawns a grid of prefab tiles via nested loops with clearly-labeled positioning/size/spacing math (take a simple prefab, lay it out). This covers point 4's "grid positioning, size" logic too.
- Q: Second mover class? A: StepMover — one cell per press, then stop. No momentum. Paired with PacmanMover (glide until wall, turn when able). Two separate classes, no toggles. No rhythm in scope.
- Q: Input wiring? A: InputSingleton (same Singleton<T> pattern) reads PlayerInputActions and BROADCASTS UnityEvents (UnityEvent<Vector2Int> OnMove). Movers subscribe via InputSingleton.Instance.OnMove.AddListener(...). Input is an instantiated singleton broadcaster, parallel to LevelSingleton. Not a separate inspector-wired component, not direct-read, not C# Action.

## DRAFTABLE — all unknowns resolved.

## Uncertainty map (ranked by leverage)
1. Architecture removal scope — strip EventBus + ServiceLocator + MonoBehaviourService/service layer entirely, or keep some? SURFACED FIRST.
2. "Multiple ways of showing a grid being made" — which approaches to demonstrate (hardcoded 2D array / string layout / inspector-authored / procedural)?
3. Alternative movement form — what is the non-Pacman class (stop-on-arrival tile-by-tile? snap step?).
4. Rhythm/Conductor — keep for students or drop entirely for simplicity?
5. Input wiring — UnityEvent wired in inspector vs player reads input directly.

## Next question
Architecture removal scope.

## Notes
- Project: game2401-2026-lecture-code/week-3 (Unity). Same heavy arch as week-3-example: EventBus, ServiceLocator, MonoBehaviourService, LevelService, InputPublisher.
- Existing lecture PlayerController.cs already has AltMoveInput stub using UnityEvent<MoveEvent> + PlayerInputActions (the simple-input direction).
- Port Singleton<T> from bvc-ggj (Assets/_Project/Code/Core/Patterns/Singleton.cs) → LevelSingleton.
- Requirements given: (1) simple Unity events input, (2) no LevelService → LevelSingleton, (3) singleton holds grid, (4) multiple grid-build approaches clearly labeled, (5) Pacman mover + alternative mover as separate classes (no toggles), (6) all code commented.
