# Puff Daddy Boss Scene Setup Guide

## Scene Structure Overview

```
BOSS2 Scene
├── Arena (Environment)
│   ├── Ground/Floor
│   ├── Walls/Boundaries
│   ├── Platforms (grapple points)
│   └── Cover Objects
├── Boss
│   └── PuffDaddy (with PuffDaddyBoss script)
├── Player
│   └── GrappleGod (your player prefab)
├── Camera
│   └── Main Camera
└── UI
    ├── Canvas
    │   ├── BossHealthBar
    │   └── PlayerHUD
    └── EventSystem
```

---

## Step-by-Step Setup

### 1. Create/Open the Boss Scene

1. **Open** `Assets/Scenes/USED IN GAME/BOSS2.unity`
2. Or create a new scene: File → New Scene → Save as "BOSS2"

---

### 2. Setup the Arena Environment

#### A. Create Arena Center Point
1. Create Empty GameObject: **"ArenaCenter"**
2. Position: (0, 0, 0)
3. This marks the center of the boss fight area

#### B. Create Ground/Floor
1. Create Sprite → 2D Object → Sprite (or use Tilemap)
2. Name: **"Ground"**
3. Scale to desired size (e.g., 20 units wide x 2 units tall)
4. Position at bottom of arena
5. Add Component → Box Collider 2D (not trigger)
6. Layer: **"Ground"**

#### C. Create Boundaries/Walls
1. Create 4 empty GameObjects: **"WallLeft", "WallRight", "WallTop", "WallBottom"**
2. Add Box Collider 2D to each
3. Position them around the arena edges
4. Make them invisible or use wall sprites
5. These prevent player/boss from leaving the arena

#### D. Create Platforms (for grappling)
1. Create 4-6 platforms around the arena
2. Position at various heights (y: 2, 4, 6 units)
3. Add sprites/graphics
4. Add Collider2D (if needed for player to stand on)
5. **Important**: Add grapple points to these platforms:
   - Create child GameObject on each platform
   - Add your GrappleTarget component
   - Tag as "GrapplePoint" (or whatever your grapple system uses)

Example platform positions:
```
Platform1: (-8, 4)    Platform2: (8, 4)
Platform3: (-10, 2)   Platform4: (10, 2)
Platform5: (0, 6)     Platform6: (0, 3)
```

#### E. Create Cover Objects (Optional)
1. Create 2-4 pillars or obstacles
2. Position between player and boss
3. Add Collider2D
4. Players can hide behind these from projectiles

---

### 3. Setup the Boss

#### A. Create Boss GameObject
1. Right-click Hierarchy → Create Empty → Name: **"PuffDaddy"**
2. Position: (0, 3, 0) (centered, floating above ground)
3. Add Component → **Rigidbody2D**
   - Body Type: Dynamic
   - Gravity Scale: **0** (boss floats)
   - Constraints: Freeze Rotation Z ✓
4. Add Component → **Sprite Renderer**
   - Assign temporary sprite (circle, square, or boss sprite)
   - Color: Purple/Gold (Bad Boy Records colors)
5. Add Component → **Collider2D** (Circle or Box)
   - Adjust size to fit sprite
6. Add Component → **Animator** (if you have animations)
7. Add Component → **Audio Source**
8. Add Component → **PuffDaddyBoss** script

#### B. Configure PuffDaddyBoss Script

**Boss Stats:**
- Max Health: 500
- Phase 2 Health Threshold: 0.5

**Movement:**
- Move Speed: 3
- Float Height: 2
- Float Speed: 1

**Attack Settings - Phase 1:**
- Attack Cooldown: 2
- Musical Note Projectile: (create prefab, assign later)
- Champagne Bottle Prefab: (create prefab, assign later)

**Attack Settings - Phase 2:**
- Phase 2 Attack Cooldown: 1.5
- Money Rain Prefab: (create prefab, assign later)
- Soundwave Projectile: (create prefab, assign later)
- Phase 2 Projectile Count: 8

**Special Attacks:**
- Microphone Slam Effect: **Assign your GroundSlamEffect prefab**
- Shockwave Radius: 8
- Shockwave Damage: 2

**Arena Settings:**
- Aggro Radius: 15
- Arena Center: **Drag the ArenaCenter GameObject here**
- Arena Radius: 10

**Audio:**
- Phase 1 Music: (assign music clip)
- Phase 2 Music: (assign music clip)
- Assign other sound effects as needed

#### C. Set Boss Layer
- Select PuffDaddy GameObject
- Set Layer to: **"Enemy"** (important for homing projectiles!)

---

### 4. Setup the Player

#### A. Add Player to Scene
1. **Drag your player prefab** into the scene
2. Position: (-10, 1, 0) (starting position, away from boss)
3. Make sure player has:
   - Health component
   - PlayerFocus component
   - PlayerAttackFreeAim component
   - Grapple hook system

#### B. Test Player Movement
- Platforms should have grapple points
- Player should be able to move around arena
- Grapple should work on platforms

---

### 5. Setup Camera

#### A. Configure Main Camera
1. Select Main Camera
2. Position: (0, 3, -10)
3. Size (Orthographic): 8-12 (adjust to fit arena)
4. **Add Camera Follow Script** (if you have one):
   - Follow Target: Drag Player GameObject
   - Or use Cinemachine for smooth camera

#### B. Optional: Camera Boundaries
- Add a script to keep camera within arena bounds
- Prevents camera from showing outside the arena

---

### 6. Setup UI

#### A. Boss Health Bar
1. Right-click Hierarchy → UI → Canvas (if not exists)
2. Inside Canvas, create UI → Image: **"BossHealthBar"**
3. Position at top of screen
4. Add child Image: **"HealthFill"** (the colored bar)
5. Set HealthFill Image Type to "Filled"
6. Create script or use existing health bar script
7. Reference PuffDaddy's health in script

Example Health Bar Script:
```csharp
public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private PuffDaddyBoss boss;
    [SerializeField] private Image fillImage;
    
    void Update()
    {
        fillImage.fillAmount = boss.CurrentHealth / boss.MaxHealth;
    }
}
```

#### B. Boss Name/Phase Display
1. Add TextMeshProUGUI above health bar
2. Text: "PUFF DADDY"
3. Font size: 32-48
4. Color: Gold

#### C. Phase Indicator
1. Add TextMeshProUGUI below health bar
2. Text: "PHASE 1" (update when phase changes)
3. Listen to boss phase change event

---

### 7. Create Arena Decoration (Optional)

#### Background:
- Add background sprites (stage, crowd, lights)
- Set sorting layer behind gameplay

#### Lighting:
- Add 2D Lights (URP) for dramatic effect
- Spotlight on boss
- Stage lights

#### Particle Effects:
- Smoke/fog at ground level
- Sparkles or confetti
- Stage effects

---

### 8. Setup Boss Projectile Prefabs (Placeholder)

For now, create simple placeholder projectiles:

#### Musical Note Projectile:
1. Create Empty GameObject: "MusicalNote"
2. Add Sprite Renderer (use music note sprite or circle)
3. Add Rigidbody2D (Gravity Scale: 0)
4. Add Circle Collider 2D (Is Trigger: ✓)
5. Add Projectile script:
   - Damage: 1
   - Speed: 10
   - Lifetime: 4
   - **Owner: Enemy** ← IMPORTANT!
6. Save as prefab

Repeat for other projectiles (champagne, money, soundwave).

---

### 9. Test the Scene

#### Checklist:
- [ ] Boss spawns at arena center
- [ ] Boss is on "Enemy" layer
- [ ] Player can move around
- [ ] Grapple points work on platforms
- [ ] Camera follows player or stays centered
- [ ] Boss health bar appears
- [ ] Walk toward boss until aggro triggers
- [ ] Boss says: "You can't stop... WON'T STOP!"
- [ ] Boss attacks in sequence (Note → Bottle → Slam)
- [ ] Ground slam effect spawns and expands
- [ ] Player takes damage from attacks
- [ ] Player projectiles damage boss
- [ ] Boss projectiles don't damage boss
- [ ] Boss enters Phase 2 at 50% health
- [ ] Phase 2 music changes
- [ ] Boss attacks faster in Phase 2
- [ ] Boss dies at 0 health

---

### 10. Polish (After Basic Setup Works)

#### Add:
- Victory screen when boss dies
- Defeat screen if player dies
- Background music transitions
- Screen shake on slam attacks
- Particle effects on attacks
- Boss intro cutscene/dialogue
- Arena decorations
- Lighting effects

---

## Quick Setup (Minimal - 5 minutes)

If you just want to test quickly:

1. **Arena**: Create large ground sprite, add colliders
2. **Boss**: Create GameObject at (0, 3), add PuffDaddyBoss script
   - Set layer to "Enemy"
   - Assign GroundSlamEffect prefab
   - Set Arena Center to (0,0)
   - Set aggro radius to 15, arena radius to 10
3. **Player**: Drop player prefab at (-10, 1)
4. **Camera**: Position at (0, 3, -10), size 10
5. **Test**: Play and walk toward boss

---

## Common Issues

**Boss doesn't activate:**
- Check aggro radius (default 15 units)
- Make sure player is moving toward boss
- Check console for aggro message

**Boss falls through floor:**
- Rigidbody2D Gravity Scale must be 0

**Projectiles don't work:**
- Make sure Owner is set correctly (Player/Enemy)
- Check layer masks

**Grapple doesn't work:**
- Add grapple points to platforms
- Check player's grapple script settings

**Camera too close/far:**
- Adjust Orthographic Size (8-12 range)

---

## Scene Hierarchy Example

```
BOSS2
├── Arena
│   ├── Ground
│   ├── WallLeft
│   ├── WallRight  
│   ├── Platform1 (with GrappleTarget child)
│   ├── Platform2 (with GrappleTarget child)
│   ├── Platform3 (with GrappleTarget child)
│   └── ArenaCenter (Empty, marks center)
├── PuffDaddy
│   └── (Rigidbody2D, SpriteRenderer, Collider, PuffDaddyBoss script)
├── Player
│   └── (Your player prefab)
├── Main Camera
│   └── (Following player or centered on arena)
└── Canvas
    ├── BossHealthBar
    │   ├── Background
    │   └── HealthFill
    ├── BossNameText
    └── PhaseIndicator
```

---

Ready to build! Start with the minimal setup, test it works, then add polish! 🎤💎

Need help with any specific part?
