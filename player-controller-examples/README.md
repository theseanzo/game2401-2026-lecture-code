# Player Controller Examples

A library of player-controller archetypes for GAME2401. Each one is a single `MonoBehaviour` plus a `MovementProfile` ScriptableObject, kept deliberately small so you can read it end to end, run it, and fork it for your own game. There is no service locator, no event bus, and no state machine — a controller subscribes to input, drives its physics, and that is the whole story.

The controllers live under `Assets/_Project/Code/Gameplay/PlayerControllers/`, one folder per archetype. Their tunable values live as `.asset` files under `Assets/_Project/ScriptableObjects/`.

## Project setup

- **Unity 6000.3.15f1**, Input System package, Cinemachine 3.x.
- Tag your rendering camera `MainCamera` and put a `CinemachineBrain` on it. Several controllers read `Camera.main` to move relative to the view; without the tag, camera-relative movement has no reference.
- `InputSingleton` bootstraps itself. The first time any controller asks for it, it spawns a hidden GameObject and enables the input actions, so there is nothing to drop in the scene by hand.

## Demo scenes

Open a scene from `Assets/_Project/Scenes/Demos/` and press play. Each archetype's player prefab lives in `Assets/_Project/Prefabs/Players/`.

| Archetype | Demo scene | Player prefab |
|---|---|---|
| ThirdPerson | `3rdPerson-Scene` | `ThirdPersonPlayer` |
| PlayerAiming (first person) | `FPS-Scene` | `FPSPlayer` |
| TopDown | `TopDown-Scene` | `TopDownPlayer` |
| Tank | `Tank-Scene` | `TankPlayer` |
| SideScroller | `SideScroll-Scene` | `SideScrollPlayer` |
| PointAndClick | `Point-Click-Scene` | `PointClickPlayer` |
| RollingBall | `Rolling-Scene` | `RollingPlayer` |
| Stage-tilt (Monkey Ball) | `MonkeyBall-Scene` | `MonkeyBall` |
| Grid | code only — no demo scene yet | — |
| Vehicle | code only — no demo scene yet | — |

## Shared architecture

Three pieces are common to every example.

**`InputSingleton`** (`Code/Gameplay/Input/InputSingleton.cs`) reads the generated `PlayerInputActions` and raises one event per intent: `OnMove`, `OnLook`, `OnJump`, `OnJumpReleased`, `OnSprint`, `OnDodge`, `OnInteract`, `OnFire`, `OnLockOn`. A controller subscribes in `OnEnable` and unsubscribes in `OnDisable`:

```csharp
private void OnEnable()
{
    _input = InputSingleton.Instance;
    _input.OnMove += HandleMove;
}
```

**`MovementProfile`** (`Code/Gameplay/PlayerControllers/Profiles/`) is a ScriptableObject of tuning values. Each archetype has its own subclass (e.g. `ThirdPersonMovementProfile`) with the fields that archetype needs. Swap the asset to retune a controller without touching code.

**`GroundCheck`** (`Code/Gameplay/PlayerControllers/Base/GroundCheck.cs`) answers "am I on the ground" for the controllers that need it. The CharacterController archetypes expect one on the player.

## Input map

`PlayerInputActions.inputactions` drives `InputSingleton`. Defaults:

| Intent | Keyboard / Mouse | Gamepad |
|---|---|---|
| Move | WASD | Left stick |
| Look / Aim | Mouse | Right stick |
| Jump | Space | South |
| Sprint | Left Shift | Left shoulder |
| Dodge | Left Ctrl | East |
| Interact | E | West |
| Fire | Left Mouse | Right trigger |
| Lock On | Right Mouse | Right stick press |

## The archetypes

Whether a controller moves relative to the camera matters for how you frame the scene. The table is the quick reference; each section below has the detail.

| Archetype | Physics | Movement frame |
|---|---|---|
| ThirdPerson | CharacterController | Camera-relative |
| PlayerAiming (first person) | CharacterController | Body-relative (controller owns the look) |
| TopDown | CharacterController | Camera-relative, or world-space by flag |
| Tank | CharacterController | Body-relative |
| SideScroller | Rigidbody | Fixed 2.5D plane |
| Grid | Transform (coroutine) | Camera-relative cardinal snap |
| PointAndClick | NavMeshAgent | Click ray through the camera |
| Vehicle | Rigidbody | Body-relative |
| RollingBall | Rigidbody | Camera-relative force |

### ThirdPerson

Orbiting third-person character. Moves relative to the camera and turns to face the direction of travel; can strafe and snap-face a locked target instead.

- **Input:** `OnMove`, `OnJump`, `OnJumpReleased`, `OnSprint`, `OnLockOn`.
- **Needs:** a `GroundCheck` and a `PlayerAnimationController` on the player, and `Camera.main` (or assign `_cameraTransform`).
- **Camera:** a **FreeLook Camera** (GameObject > Cinemachine > FreeLook Camera), Tracking Target = player. It ships with Orbital Follow, Rotation Composer, and the mouse-orbit input wired. The controller reads the camera for movement, so no extra hookup.
- **Lock-on (optional):** add a second `CinemachineCamera` at higher Priority that looks at the target, enabled while locked.

### PlayerAiming (first person)

First-person strafe. The controller drives the look itself: it yaws the body and pitches a camera pivot, so movement is always relative to the body's own forward.

- **Input:** `OnMove`, `OnLook`, `OnJump`, `OnJumpReleased`, `OnSprint`.
- **Needs:** a `GroundCheck`; an eye-height child assigned to `_cameraPivot` for pitch (it runs without one, but you lose vertical look). Cursor is locked on start.
- **Camera:** simplest is to make the **Main Camera a child of `_cameraPivot`** — no Cinemachine camera needed, the body's yaw and the pivot's pitch move the view. Cinemachine alternative: a `CinemachineCamera` Tracking the pivot with Position = Hard Lock To Target, Rotation = Same As Follow Target, and no Input Axis Controller (the controller already reads the look).

### TopDown

Twin-stick mover on the ground plane. The body rotates independently of travel through one of three modes: mouse-cursor aim, twin-stick aim, or auto-face-movement.

- **Input:** `OnMove`, `OnLook`.
- **Needs:** a `GroundCheck`. Uses `Camera.main` only when `UseWorldSpaceMovement` is off; with it on, no camera is needed for control.
- **Camera:** overhead `CinemachineCamera`, Tracking Target = player. Position = Position Composer with a high-Y, slightly-back offset. Rotation = **none**: set the camera transform to look straight down (X = 90) and leave Rotation Control empty. Do not use a look-at on a near-straight-down camera — looking down is a gimbal singularity and the view will spin.
- **Mouse-cursor aim:** the controller rays the cursor through `Camera.main` onto the ground plane, so the Main Camera must be tagged and looking down.

### Tank

Tank controls. The vertical stick drives forward and back along the body's own axis; the horizontal stick rotates the body in place. No camera-relative input, so the camera is free.

- **Input:** `OnMove` only.
- **Needs:** a `GroundCheck`. Backward is slower than forward; a flag controls whether it can turn while driving.
- **Camera:** `CinemachineCamera` Tracking the tank — a chase rig (Position Composer or Third Person Follow with a behind/above offset, Rotation Composer) or a fixed isometric angle. Any framing works.

### SideScroller

2.5D platformer on the X axis. The most feature-rich example: variable jump height, double jump, air dash, and wall slide with wall jump.

- **Input:** `OnMove`, `OnJump`, `OnJumpReleased`, `OnSprint`, `OnDodge`.
- **Needs:** a Rigidbody (the controller freezes its rotation and turns gravity off, driving velocity itself in `FixedUpdate`). Wall mechanics raycast against the layers set in the profile's `WallLayers`, so set those up.
- **Camera:** side-on `CinemachineCamera`, Tracking Target = player. Position = Position Composer following X and Y with a fixed Z standoff. Rotation = fixed, facing the play plane. Set the lens to Orthographic for a flat look.

### Grid

Tile-based movement. Each press steps exactly one cell over a fixed duration; analog input snaps to the four cardinal directions, and a new move is refused until the current step finishes.

- **Input:** `OnMove` only.
- **Needs:** `Camera.main` — the camera's yaw defines which world directions the cardinal steps map to. No Rigidbody or CharacterController. Collision checking is optional via a profile flag.
- **Camera:** fixed isometric `CinemachineCamera`, Tracking Target = player, Position Composer with an isometric offset, Rotation = a fixed angle. Keep the yaw fixed, or the stick directions rotate with the camera (a 45-degree isometric reads cleanly).

### PointAndClick

Click-to-move pathfinding. A mouse click raycasts from the camera onto the clickable layers, and a `NavMeshAgent` paths to the hit point, slowing as it arrives.

- **Input:** reads `Mouse.current` directly through the Input System — it does **not** use `InputSingleton`.
- **Needs:** a **baked NavMesh** in the scene and a `NavMeshAgent` on the player; the ground must be NavMesh-walkable. `Camera.main` must be tagged for the click ray.
- **Camera:** isometric or overhead `CinemachineCamera`, Tracking Target = player, Position Composer with an isometric/overhead offset, Rotation fixed or look-at.

### Vehicle

Arcade car. Throttle accelerates toward a max speed; steering only bites while moving and scales with speed; grip cancels sideways slide, and a handbrake drops grip to let the back end drift.

- **Input:** `OnMove` (Y = throttle/brake, X = steer), `OnJump`/`OnJumpReleased`, `OnDodge` (handbrake).
- **Needs:** a Rigidbody (driven with forces and `MoveRotation` in `FixedUpdate`). An optional ground ray; with its distance at 0 the car is treated as always grounded.
- **Camera:** chase `CinemachineCamera`, Tracking Target = car, Third Person Follow with a behind/above offset, Rotation Composer looking at the car.

### RollingBall

Physics sphere pushed with camera-relative force. The arcade take on a rolling ball: it adds force and rolling torque, clamps top speed, and jumps with an impulse when grounded. For the authentic Super Monkey Ball model, see the next section.

- **Input:** `OnMove`, `OnJump`.
- **Needs:** a Rigidbody and a Sphere Collider. Grounding is a `SphereCast` against the profile's `GroundLayers`. Reads `Camera.main` (or an assigned `_cameraTransform`) for the push direction.
- **Camera:** a **FreeLook Camera** Tracking the ball gives the orbiting Monkey-Ball feel. Set the follow **Binding Mode = World Space** (or Lazy Follow) so the ball's spin does not whip the camera around.

## Stage-tilt (authentic Super Monkey Ball)

`RollingBall` pushes the ball directly. Real Super Monkey Ball never touches the ball — it tilts the floor and lets gravity roll the ball downhill. This example is that model, and it is a *level* controller, not a player controller. It is three decoupled pieces:

- **Stage** — a kinematic Rigidbody + `StageTiltController` (`Code/Gameplay/Stage/`) on a root that parents all the level geometry, pivot at the stage center. It reads the stick and rotates the whole stage, camera-relative and clamped, via `MoveRotation` so the moving floor carries the resting ball. Tuning lives on a `StageTiltProfile` asset.
- **Ball** — a plain dynamic Rigidbody sphere with a collider and gravity. **No script.** It rolls down whatever slope the stage gives it.
- **Monkey** — a `MonkeyVisual` on a separate object (not a child of the ball). It copies the ball's position, stays upright, and turns to face the steer direction it gets from the stage's `OnSteer` event. Keeping it off the ball's rotation is what stops it tumbling.

**Setup:**

1. Parent the level under a `Stage` object (pivot centered). Add a Rigidbody and `StageTiltController`, and assign a `StageTiltProfile` (Create > ScriptableObjects > Stage).
2. Add a sphere `Ball` with a dynamic Rigidbody (gravity on), Sphere Collider, Collision Detection = Continuous. No script. Hide its mesh if you only want to see the monkey.
3. Add the character as its own `Monkey` object with `MonkeyVisual`; assign the ball and the stage, and an offset that sits it on top of the ball.
4. Camera: a `CinemachineCamera` Tracking the **ball** (it settles in physics, so no jitter), Binding Mode = World Space. Do not parent it under the stage, or the horizon tilts with the floor.

Push the stick: the stage leans, the ball rolls, the monkey rides upright.

## Adding your own

The recipe every example follows:

1. One `MonoBehaviour` for the controller. Subscribe to the `InputSingleton` events you need in `OnEnable`, unsubscribe in `OnDisable`.
2. One `MovementProfile` subclass for the tunables, exposed as a serialized field.
3. Drive whichever physics backing fits the archetype (CharacterController, Rigidbody, NavMeshAgent, or transform), and frame a Cinemachine camera to match the movement frame.
