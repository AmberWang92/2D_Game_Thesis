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

- Enemies, spawner, score, HUD, game over → Milestones 2 and 3.
- Layers `Enemy` / `EnemyBullet` are reserved but unused in M1.
