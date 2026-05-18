# Milestone 1 — Editor setup

All gameplay code is in place. Unity needs you to author the prefabs/scene wiring once;
re-runs after that are pure code edits. Steps below assume Unity 6000.3, URP 2D, and the
existing `Assets/InputSystem_Actions.inputactions`.

## 1. Layers

`Edit > Project Settings > Tags and Layers` — add user layers:
- `Player`
- `PlayerBullet`
- `Enemy` (used in M2)
- `EnemyBullet` (used in M2)

`Edit > Project Settings > Physics 2D` collision matrix:
- Uncheck `PlayerBullet` × `Player` (a player's own bullet shouldn't hit them).
- Uncheck `PlayerBullet` × `PlayerBullet`.
- Leave `PlayerBullet` × default checked so the projectile collider events fire.

## 2. ScriptableObject assets

Right-click `Assets/Settings/Data/` (create if missing) and use `Create > TopDownShooter`:

1. **Projectile Definition** → `PlayerBulletDef.asset`
   - `damage = 1`, `lifetime = 1.5`, `hitMask = Enemy` (set after M2; for M1 leave `Everything` minus Player layer).
   - `tint = yellow`.
2. **Weapon Definition** → `Pistol.asset`
   - `projectile = PlayerBulletDef`, `fireRate = 6`, `muzzleSpeed = 18`, `projectilesPerShot = 1`, `spreadDegrees = 2`, `poolPrewarm = 32`.
3. **Player Stats** → `PlayerStats.asset`
   - `moveSpeed = 6`, `maxHP = 10`, `primaryWeapon = Pistol`.
4. **Void Event Channel** → `PlayerDiedChannel.asset` (for M3, but create now).

Leave `PlayerBulletDef.prefab` field empty until step 3 below; come back and assign.

## 3. Prefabs

Create these under `Assets/Prefabs/` (drag from Hierarchy):

### PlayerBullet.prefab
- Empty GameObject, layer = `PlayerBullet`.
- Components:
  - `SpriteRenderer` (sprite empty — `PlaceholderSpriteFactory` fills it).
  - `Rigidbody2D`: BodyType `Dynamic`, GravityScale 0, Interpolate `Interpolate`, CollisionDetection `Continuous`, FreezeRotation Z.
  - `CircleCollider2D`: `Is Trigger = true`, radius ~0.1.
  - `PlaceholderSpriteFactory`: shape `Circle`, color yellow, size 16.
  - `Projectile`: `definition = PlayerBulletDef`.
- Save as prefab. Then go back to `PlayerBulletDef.asset` and set `prefab = PlayerBullet`.

### Player.prefab
- Empty GameObject named `Player`, layer = `Player`.
- Children:
  - `Visual` — `SpriteRenderer` empty + `PlaceholderSpriteFactory` (shape `Circle`, color cyan, size 64).
  - `AimPivot` — empty transform; child `Muzzle` offset `(0.6, 0, 0)`.
- Components on root:
  - `Rigidbody2D`: Dynamic, GravityScale 0, FreezeRotation Z, Interpolate.
  - `CircleCollider2D` (radius ~0.4).
  - `MovementComponent2D` (auto).
  - `HealthComponent` (initial Max overridden by stats; assign `diedChannel = PlayerDiedChannel`).
  - `WeaponHolder`: `definition = Pistol`, `muzzle = AimPivot/Muzzle`.
  - `PlayerInputReader`:
    - `moveAction` → `InputSystem_Actions/Player/Move`
    - `lookAction` → `InputSystem_Actions/Player/Look`
    - `attackAction` → `InputSystem_Actions/Player/Attack`
    - `worldCamera` = leave empty (uses Camera.main).
  - `PlayerController`: `stats = PlayerStats`, `input = PlayerInputReader`, `weapon = WeaponHolder`, `aimPivot = AimPivot`.
- Save as prefab.

## 4. Scene wiring (`SampleScene.unity`)

1. Drop `Player.prefab` at origin.
2. On `Main Camera`:
   - Set Projection `Orthographic`, Size ~7.
   - Add `CameraFollow2D`, set `target = Player` transform.
3. Press Play:
   - WASD / left stick moves the player.
   - Mouse / right stick aims (`Look` action).
   - LMB / gamepad south fires bullets toward the cursor.
   - Bullets fly, despawn after 1.5s, return to pool.

## 5. Quick smoke checks

- Bullets visible (yellow circles)? If not, check `PlaceholderSpriteFactory` is on the bullet prefab and `SpriteRenderer.sprite` is empty.
- Player not falling? `Rigidbody2D.GravityScale` must be 0 (also enforced in code on Awake).
- Aim model: `PlayerInputReader` reads `Mouse.current.position` directly for pointer aim (the existing `Look` action is bound to pointer *delta*, not absolute position, so it can't be used for cursor projection). For gamepad it samples `Gamepad.current.rightStick` and uses it as a direction when above the deadzone. Stick aim takes priority when present; otherwise the player aims at the cursor.

## 6. What's intentionally NOT here yet

- Spawner, score, HUD, game over → Milestone 3.

---

# Milestone 2 — Enemies

Adds Chaser + Shooter enemy archetypes, a perception sensor, contact damage, and an `EnemyKilled` channel.

## M2.1 Layers + collision matrix

Make sure both `Enemy` and `EnemyBullet` user layers exist.

Physics 2D collision matrix:
- `EnemyBullet` × `Player` → enabled (enemy bullets must hit player).
- `EnemyBullet` × `Enemy` → disabled (don't friendly-fire).
- `EnemyBullet` × `EnemyBullet` → disabled.
- `PlayerBullet` × `Enemy` → enabled.
- `PlayerBullet` × `Player` → disabled (already set in M1).
- `Enemy` × `Player` → enabled (chasers need physical contact to damage).
- `Enemy` × `Enemy` → enabled (they bump; if you want overlap, disable).

Update `PlayerBulletDef.hitMask` to `Enemy` only.

## M2.2 SO assets (`Assets/Settings/Data/`)

1. **Projectile Definition** → `EnemyBulletDef.asset`
   - `damage = 1`, `lifetime = 2`, `hitMask = Player`, `tint = magenta`.
2. **Weapon Definition** → `EnemyPistol.asset`
   - `projectile = EnemyBulletDef`, `fireRate = 1.5`, `muzzleSpeed = 9`, `spreadDegrees = 4`, `poolPrewarm = 8`.
3. **Void Event Channel** → `EnemyKilledChannel.asset` (used by `ScoreService` in M3).
4. **Enemy Definition** → `ChaserDef.asset`
   - `behavior = Chaser`, `moveSpeed = 3`, `maxHP = 3`, `detectionRadius = 12`,
     `contactDamage = 1`, `contactInterval = 0.6`, `scoreValue = 10`, `tint = red`.
5. **Enemy Definition** → `ShooterDef.asset`
   - `behavior = Shooter`, `moveSpeed = 2`, `maxHP = 2`, `detectionRadius = 14`,
     `attackRange = 7`, `weapon = EnemyPistol`, `scoreValue = 25`, `tint = magenta`.

Assign prefabs (next step) back into `ChaserDef.prefab` / `ShooterDef.prefab` after they exist.

## M2.3 Prefabs (`Assets/Prefabs/Enemies/`, `Assets/Prefabs/Projectiles/`)

### EnemyBullet.prefab
Same recipe as `PlayerBullet.prefab` but:
- Layer = `EnemyBullet`.
- `PlaceholderSpriteFactory` color = magenta, size 16.
- `Projectile.definition = EnemyBulletDef`.

### Chaser.prefab
- Root GameObject `Chaser`, layer = `Enemy`.
- Components on root:
  - `Rigidbody2D` (Dynamic, GravityScale 0, FreezeRotation Z, Interpolate, CollisionDetection Continuous).
  - `CircleCollider2D` (radius ~0.4, **not** trigger — chasers physically touch the player).
  - `SpriteRenderer` + `PlaceholderSpriteFactory` (shape Circle, size 56). Tint is overridden at runtime from the definition.
  - `MovementComponent2D`, `HealthComponent` (assign `diedChannel` empty — we use the `killedChannel` on the controller).
  - `EnemySensor`: `targetMask = Player`, leave `radius` (overwritten from definition at runtime).
  - `ContactDamageDealer`: `targetMask = Player` (values overwritten from definition).
  - `EnemyController`: `definition = ChaserDef`, `sensor`, `contact` references; leave `weapon` empty; `killedChannel = EnemyKilledChannel`.
- Save prefab → set `ChaserDef.prefab = Chaser`.

### Shooter.prefab
- Root GameObject `Shooter`, layer = `Enemy`.
- Children:
  - `Visual` (SpriteRenderer + PlaceholderSpriteFactory, square, size 56).
  - `AimPivot` empty → child `Muzzle` at `(0.5, 0, 0)`.
- Components on root:
  - `Rigidbody2D`, `CircleCollider2D` (non-trigger, radius ~0.4).
  - `MovementComponent2D`, `HealthComponent`.
  - `EnemySensor` (`targetMask = Player`).
  - `WeaponHolder`: `definition = EnemyPistol`, `muzzle = AimPivot/Muzzle`.
  - `EnemyController`: `definition = ShooterDef`, `sensor`, `weapon`, `aimPivot = AimPivot`; leave `contact` empty (shooters don't melee); `killedChannel = EnemyKilledChannel`.
- Save prefab → set `ShooterDef.prefab = Shooter`.

## M2.4 Scene smoke test

1. Drop 2–3 `Chaser.prefab` and 1 `Shooter.prefab` into `SampleScene` around the player.
2. Press Play. Expected:
   - Chasers detect the player within 12 units and run at them; touching the player ticks HP down every ~0.6s.
   - Shooter stops at ~7 units and fires magenta bullets; bullets damage the player on hit.
   - Player bullets damage enemies; HP reaches 0 → enemy disables colliders, briefly lingers, then deactivates.
   - `EnemyKilledChannel` raises on each death (M3 will subscribe).
   - Player death (HP 0) puts the player into `PlayerDeadState`; the scene continues to run (game-over flow is M3).

## M2.5 Notes on design

- **Chaser vs Shooter selection**: behaviour switch lives in the SO (`EnemyBehavior`). `EnemyController` reads it; states branch on it. Adding a new archetype = new `EnemyDefinitionSO` + (if needed) new `IState<EnemyController>` implementation. No edits to the controller.
- **Pool reuse readiness**: `EnemyController.OnEnable` resets HP, re-enables colliders, and resets the FSM to Idle so the same instance can be recycled by the M3 spawner without extra glue.
- **Damage routing**: enemies don't reference the player. Their `ContactDamageDealer` only knows the `Player` layer mask, and `DamageDealer.TryApply` resolves any `IDamageable` it finds. No coupling between Enemy and PlayerController types.
- **Aim hysteresis**: `EnemyAttackState` uses 1.15× attackRange as the exit threshold to prevent jitter between Chase and Attack at the boundary.

---

# Milestone 3 — Survival Loop

Adds the run lifecycle (GameManager FSM), endless time-scaled spawning, score, HUD, and a Game Over panel with restart.

## M3.1 SO assets (`Assets/Settings/Data/`)

1. **Void Event Channel** → `GameStartedChannel.asset`.
2. **Void Event Channel** → `GameOverChannel.asset`.
3. **Int Event Channel** → `EnemyScoreChannel.asset` (per-kill score deltas).
4. **Int Event Channel** → `ScoreChangedChannel.asset` (running total for the HUD).

Re-open the existing **enemy prefabs** (`Chaser`, `Shooter`) and on `EnemyController`:
- `killedChannel` → `EnemyKilledChannel` (unchanged from M2; SpawnDirector does not need it).
- `scoreChannel` → `EnemyScoreChannel` *(new field added in M3)*.

Re-open the **player prefab** (`Player`) and on `HealthComponent`:
- `diedChannel` is already `PlayerDiedChannel` from M1. Leave it.

## M3.2 Scene wiring

In `SampleScene`, create a single empty GameObject `GameSystems` and add three components:

### `GameManager`
- `playerDiedChannel` → `PlayerDiedChannel`.
- `gameStartedChannel` → `GameStartedChannel`.
- `gameOverChannel` → `GameOverChannel`.
- `autoStart` = true.

### `ScoreService`
- `enemyScoreChannel` → `EnemyScoreChannel`.
- `gameStartedChannel` → `GameStartedChannel` (resets on each run).
- `scoreChangedChannel` → `ScoreChangedChannel`.

### `SpawnDirector`
- `player` → Player transform; `worldCamera` → Main Camera; `game` → `GameSystems` (the GameManager).
- `enemyPool` → array containing `ChaserDef` and `ShooterDef`.
- `prewarmPerArchetype` = 4.
- `spawnIntervalOverTime` curve: starts ~2.0 at t=0, drops to ~0.4 by t=120.
- `concurrentCapOverTime` curve: starts at 4 at t=0, rises to ~24 by t=120.
- `spawnRingMargin` = 2; `warmupDelay` = 1.5.

Remove any hand-placed enemies from the scene — the director is now the sole source of enemies.

## M3.3 UI (Canvas)

Create a UI Canvas (Screen Space — Overlay) with an EventSystem.

### HUD anchored at the top
Three `TextMeshProUGUI` elements (`HPText`, `TimeText`, `ScoreText`) plus a `HUDController` component on the Canvas (or any child) wired to:
- `playerHealth` → Player's `HealthComponent`.
- `game` → `GameSystems` GameManager.
- `scoreChangedChannel` → `ScoreChangedChannel`.
- `hpText`, `scoreText`, `timeText` → the three TMP labels.

### GameOverPanel (child Panel of the Canvas)
- A `Panel` GameObject `GameOverPanel` (Image dimmer) containing:
  - `FinalScoreText` (TMP), `SurvivedText` (TMP), and a `RestartButton` (UI Button).
- Add `GameOverPanel` component to the panel and wire:
  - `gameOverChannel` → `GameOverChannel`.
  - `game` → `GameSystems`; `score` → `GameSystems` (ScoreService).
  - `root` → the panel itself (will be hidden in Awake).
  - `finalScoreText`, `survivedText`, `restartButton` → corresponding UI nodes.

The panel will be hidden at start and shown automatically when `GameOverChannel` raises; the button reloads the active scene via `GameManager.RestartScene()`.

## M3.4 Smoke test

1. Press Play. After the 1.5s warmup, enemies start streaming in from just outside the camera.
2. HUD shows HP, score climbing on kills, and a `m:ss` timer.
3. Difficulty curve increases the cap and decreases the interval as time progresses.
4. When the player's HP hits 0:
   - `HealthComponent` raises `PlayerDiedChannel`.
   - `GameManager` transitions to `GameOverState` and raises `GameOverChannel`.
   - `SpawnDirector` stops spawning (gated on `game.IsRunning`).
   - `GameOverPanel` appears with final score + duration; clicking Restart reloads `SampleScene`.

## M3.5 Notes on design

- **One direction of data flow**: enemies raise `EnemyScoreChannel`; `ScoreService` accumulates and re-broadcasts on `ScoreChangedChannel`; HUD only reads. No system polls another system's state.
- **Spawner uses the same generic pool as projectiles**: one `ObjectPool<EnemyController>` per archetype, keyed by `EnemyDefinitionSO`. `EnemyController.Despawned` is what returns the instance and decrements `_aliveCount` — the void `EnemyKilledChannel` is intentionally NOT used for that (it would conflate death with deactivation, which are separated by `despawnDelay`).
- **No parenting footgun for enemies**: `SpawnDirector` instantiates enemy prefabs with no parent, mirroring the constraint enforced on `WeaponHolder.projectileParent` after the M2 slow-bullet bug.
- **Restart by scene reload**: the run is stateless across reloads — every SO event channel is intact, but all MonoBehaviour subscribers are torn down by Unity on unload and freshly re-added on load, so no manual unsubscribe gymnastics are required.
- **Difficulty curves over code constants**: `spawnIntervalOverTime` and `concurrentCapOverTime` are `AnimationCurve`s on the spawner, so tuning is a designer task (Inspector), not a programmer task.
