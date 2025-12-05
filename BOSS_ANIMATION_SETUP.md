# Boss Animation Setup Guide - Unity Animator

## Overview
This guide shows how to set up the Animator Controller for the Puff Daddy boss with all states, transitions, and triggers.

---

## Step 1: Create Animator Controller

1. **Right-click in Project window** → Create → Animator Controller
2. Name it: **"PuffDaddyAnimator"**
3. Save in `Assets/Animations/` folder (create if needed)

---

## Step 2: Create Animation Clips

For each animation state, create an animation clip:

### A. Create the Animation Clips

1. Right-click in Project → Create → Animation
2. Create these clips:

**Required Animations:**
- `PuffDaddy_Idle` - Default standing/floating pose
- `PuffDaddy_Move` - Moving/flying animation
- `PuffDaddy_Attack1` - Musical note barrage
- `PuffDaddy_Attack2` - Champagne bottle throw
- `PuffDaddy_Attack3` - Money rain (Phase 2)
- `PuffDaddy_SpecialAttack` - Microphone slam
- `PuffDaddy_Hurt` - Taking damage
- `PuffDaddy_PhaseTransition` - Phase 1 → Phase 2
- `PuffDaddy_Death` - Death sequence

---

## Step 3: Setup Animator Controller

### A. Assign Animator to Boss

1. Select **PuffDaddy GameObject** in Hierarchy
2. In Animator component:
   - Controller: Drag **PuffDaddyAnimator**
3. Avatar: None (for 2D)
4. Apply Root Motion: **Unchecked**

### B. Open Animator Window

1. Double-click **PuffDaddyAnimator** in Project
2. Opens Animator window
3. You'll see a grid with "Entry" node

---

## Step 4: Create States

### A. Add Animation States

In the Animator window:

1. **Right-click** → Create State → Empty
2. Name it based on the list below
3. In Inspector, assign the corresponding animation clip
4. Repeat for all states

**States to Create:**

```
├── Idle (default state - orange)
├── Move
├── Attack1
├── Attack2  
├── Attack3
├── SpecialAttack
├── Hurt
├── PhaseTransition
└── Death
```

### B. Set Default State

1. **Right-click "Idle"** → Set as Layer Default State
2. Idle should turn **orange**

---

## Step 5: Create Parameters

Parameters are used to trigger transitions between states.

### In Animator Window:

1. Click **"Parameters"** tab (left side)
2. Click **"+"** button
3. Add these parameters:

**Triggers** (one-shot, auto-resets):
- `Attack` (Trigger)
- `Attack2` (Trigger)
- `Attack3` (Trigger)
- `SpecialAttack` (Trigger)
- `Hurt` (Trigger)
- `PhaseTransition` (Trigger)
- `Death` (Trigger)

**Bool** (stays true/false):
- `IsMoving` (Bool)

**Float** (optional, for future):
- `Speed` (Float)

---

## Step 6: Create Transitions

Transitions connect states and define when to switch animations.

### Core Transitions:

#### From Idle:
1. **Idle → Move**
   - Click Idle → Right-click → Make Transition → Click Move
   - Condition: `IsMoving` is `true`
   - Settings:
     - Has Exit Time: ✓
     - Exit Time: 0.9
     - Transition Duration: 0.1

2. **Idle → Attack1**
   - Condition: `Attack` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0.1

3. **Idle → Attack2**
   - Condition: `Attack2` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0.1

4. **Idle → Attack3**
   - Condition: `Attack3` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0.1

5. **Idle → SpecialAttack**
   - Condition: `SpecialAttack` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0.1

6. **Idle → Hurt**
   - Condition: `Hurt` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0
   - Can Transition To Self: ✓

7. **Idle → PhaseTransition**
   - Condition: `PhaseTransition` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0.2

8. **Idle → Death**
   - Condition: `Death` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0.1

---

#### From Move:
1. **Move → Idle**
   - Condition: `IsMoving` is `false`
   - Has Exit Time: ✓
   - Exit Time: 0.8
   - Transition Duration: 0.2

2. **Move → Attack1**
   - Condition: `Attack` (trigger)
   - Has Exit Time: ✗

3. **Move → Attack2**
   - Condition: `Attack2` (trigger)
   - Has Exit Time: ✗

4. **Move → SpecialAttack**
   - Condition: `SpecialAttack` (trigger)
   - Has Exit Time: ✗

5. **Move → Hurt**
   - Condition: `Hurt` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0

---

#### From Attack States (All attacks return to Idle):

For **each** of Attack1, Attack2, Attack3, SpecialAttack:

1. **Attack → Idle**
   - Condition: None (automatic)
   - Has Exit Time: ✓
   - Exit Time: 0.95 (let animation almost finish)
   - Transition Duration: 0.1

2. **Attack → Hurt** (can be interrupted by damage)
   - Condition: `Hurt` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0

3. **Attack → Death**
   - Condition: `Death` (trigger)
   - Has Exit Time: ✗
   - Transition Duration: 0

---

#### From Hurt:
1. **Hurt → Idle**
   - Condition: None
   - Has Exit Time: ✓
   - Exit Time: 1.0 (play full animation)
   - Transition Duration: 0.1

2. **Hurt → Death**
   - Condition: `Death` (trigger)
   - Has Exit Time: ✗

---

#### From PhaseTransition:
1. **PhaseTransition → Idle**
   - Condition: None
   - Has Exit Time: ✓
   - Exit Time: 1.0 (play full animation)
   - Transition Duration: 0.2

---

#### From Death:
**NO transitions** - Death is final state

---

## Step 7: Animation Clips Setup

### If You Have Sprite Sheets:

1. **Select sprite sheet** in Project
2. **Sprite Mode**: Multiple
3. **Pixels Per Unit**: 100 (or your game's setting)
4. Click **Sprite Editor** → Slice → Apply
5. **Select boss GameObject** in Hierarchy
6. Open Animation window: Window → Animation → Animation
7. Click **Create** → Save as animation name
8. **Drag sprite frames** into Animation timeline
9. Set frame rate (6-12 FPS for pixel art, 24-30 for smooth)
10. **Repeat for each animation**

### If Using Single Sprites (Placeholder):

Each animation can just be:
- **Idle**: Single sprite, or sprite bobbing up/down
- **Attack**: Single sprite with color flash
- **Hurt**: Sprite with red tint
- **Death**: Sprite fading out

---

## Step 8: Configure Boss Script

The PuffDaddyBoss script already has the correct trigger calls:

```csharp
// In code, it calls:
anim.SetTrigger("Attack");          // For Attack1
anim.SetTrigger("Attack2");         // For Attack2
anim.SetTrigger("Attack3");         // For Attack3
anim.SetTrigger("SpecialAttack");   // For Microphone Slam
anim.SetTrigger("Hurt");            // When taking damage
anim.SetTrigger("PhaseTransition"); // At 50% health
anim.SetTrigger("Death");           // At 0 health
```

**No changes needed** - the code already matches!

---

## Animator State Diagram (Visual Reference)

```
        ┌─────────┐
        │  ENTRY  │
        └────┬────┘
             │
             ▼
        ┌─────────┐ ◄─────────────────────┐
    ┌──►│  IDLE   │                       │
    │   └────┬────┘                       │
    │        │                            │
    │        ├─► Attack1 ──────────────┐  │
    │        ├─► Attack2 ──────────────┤  │
    │        ├─► Attack3 ──────────────┤  │
    │        ├─► SpecialAttack ────────┤  │
    │        │                         │  │
    │        ├─► Move ◄────────────────┘  │
    │        │     │                      │
    │        │     └──────────────────────┘
    │        │
    │        ├─► PhaseTransition ─────────┘
    │        │
    │        ├─► Hurt ───────────────────┐
    │        │                           │
    │        └─► Death (END)             │
    │                                    │
    └────────────────────────────────────┘
```

---

## Step 9: Testing

### Test Each State:

1. **Press Play** in Unity
2. **Open Animator window** while playing
3. Watch states transition
4. Check Console for logs from boss script

### Manual Testing:

In a test script, you can manually trigger:
```csharp
// Temporary test script
GetComponent<Animator>().SetTrigger("Attack");
```

---

## Quick Setup (Using Placeholder Animations)

If you don't have sprite sheets yet:

### Create Simple Animations:

1. **Idle Animation**:
   - Single frame
   - Or two frames: normal → slight scale change

2. **Attack Animations**:
   - Flash white for 0.1s
   - Return to normal

3. **Hurt Animation**:
   - Flash red for 0.2s

4. **Death Animation**:
   - Fade alpha from 1 → 0 over 2 seconds
   - Scale shrink

### How to Create Placeholder Animations:

1. Select Boss GameObject
2. Window → Animation → Animation
3. Click **Create** → Name: "PuffDaddy_Idle"
4. Click **Record** button (red circle)
5. **Move timeline** to 0:30 (half second)
6. **Change a property** (e.g., position Y += 0.2)
7. Timeline to 1:00
8. Change property back (position Y -= 0.2)
9. Stop recording
10. **Loop**: Check loop in Animation window

Repeat for other animations!

---

## Common Issues

### Animations Not Playing:
- Check Animator is assigned to boss
- Check animation clips are assigned to states
- Check parameters are spelled correctly (case-sensitive!)
- Check transitions have correct conditions

### Boss Stuck in One State:
- Check "Has Exit Time" settings
- Make sure transitions exist back to Idle
- Check if trigger is being called in code

### Animations Too Fast/Slow:
- Adjust animation clip speed in Animation window
- Or adjust in Animator state (Speed multiplier)

### Transitions Too Jerky:
- Reduce Transition Duration (0.1-0.2 is good)
- Check Exit Time isn't too early

---

## Animation Timing Reference

Good timing for each animation:

- **Idle**: 1-2 seconds loop
- **Move**: 0.5-1 second loop
- **Attack1**: 0.5-0.8 seconds
- **Attack2**: 0.6-1.0 seconds
- **Attack3**: 0.8-1.2 seconds
- **SpecialAttack**: 1.0-1.5 seconds
- **Hurt**: 0.2-0.3 seconds (quick!)
- **PhaseTransition**: 2-3 seconds (dramatic!)
- **Death**: 2-4 seconds (epic finale!)

---

## Next Steps

1. ✅ Create Animator Controller
2. ✅ Add all states
3. ✅ Create parameters
4. ✅ Connect transitions
5. ✅ Create animation clips (even simple placeholders)
6. ✅ Assign to boss GameObject
7. ✅ Test in Play mode
8. ✅ Replace with real animations later

---

You can start with **super simple placeholder animations** (just color flashes or scale changes) and the boss will work! Replace with real sprite animations later when you have the art! 🎮

Want me to help with any specific part?
