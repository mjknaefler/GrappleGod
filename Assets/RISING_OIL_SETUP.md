# 🛢️ Rising Oil Level Setup Guide

## Overview
The oil level rises throughout the boss fight, forcing the player to move to higher platforms. The oil rises faster in Phase 2, adding urgency to the fight!

---

## Step 1: Create the Oil Visual

### A. Create the Oil GameObject
1. **Right-click in Hierarchy** → Create Empty
2. **Name it**: `RisingOil`
3. **Position**: X: 0, Y: -10 (below arena), Z: 0

### B. Add Sprite Renderer
1. **Add Component** → Sprite Renderer
2. **Sprite**: Use a **large rectangle sprite** (or create one)
   - Option A: Create in Assets → Right-click → Create → Sprites → Square
   - Option B: Use any large sprite and stretch it
3. **Scale**: Make it WIDE to cover entire arena
   - X: 50+ (should cover full arena width)
   - Y: 20 (tall enough to fill screen)
4. **Color**: Dark brown/black for oil look
   - R: 50, G: 40, B: 25, A: 200
5. **Sorting Layer**: Behind player but in front of background
6. **Material**: Sprites-Default

### C. Add Box Collider 2D (for damage detection)
1. **Add Component** → Box Collider 2D
2. **Is Trigger**: ✓ (checked)
3. **Size**: Match the sprite size (should be large)
4. **Offset**: Y: 10 (so trigger is at top of oil surface)

### D. Add the Rising Oil Script
1. **Add Component** → `RisingOilLevel`
2. **Configure Settings**:
   - **Rise Speed**: 0.5 (units per second)
   - **Start Y**: -10 (matches GameObject Y position)
   - **Max Y**: 5 (highest point oil can reach)
   - **Damage Per Second**: 2
   - **Damage Interval**: 1 second
   - **Oil Sprite Renderer**: Drag the Sprite Renderer component here
   - **Oil Color**: Dark brown (50, 40, 25, 200)
   - **Start Rising Immediately**: ❌ (unchecked - uses delay)
   - **Delay Before Rising**: 5 seconds

---

## Step 2: Connect to Boss

1. **Select Puff Daddy Boss** in Hierarchy
2. **Find PuffDaddyBoss component** in Inspector
3. **Scroll to Arena Settings**
4. **Rising Oil** field: Drag the `RisingOil` GameObject here

---

## Step 3: Design Your Arena with Platforms

The oil rising mechanic requires platforms at different heights!

### Recommended Layout:
```
         [Platform]     5 units high
   
   [Platform]   [Platform]  3 units high

[Platform]         [Platform]  1 unit high

================================ Ground (Y: 0)
```

### Platform Setup:
1. Create 4-6 platforms at varying heights
2. **Low platforms**: Y: 1-2 (flood first)
3. **Mid platforms**: Y: 3-4 (safe for Phase 1)
4. **High platforms**: Y: 5-7 (needed for Phase 2)
5. Add **grapple points** on each platform for mobility

---

## Step 4: Test the Oil Rising

1. **Play Mode**
2. **After 5 seconds**: Oil starts rising from Y: -10
3. **Watch it climb** at 0.5 units/second
4. **Touch the oil**: Player takes 2 damage per second
5. **Boss hits 50% HP**: Oil rises 2x faster (1 unit/second)
6. **At Y: 5**: Oil stops rising

---

## Customization Options

### Make it Rise Faster from Start
```
Start Rising Immediately: ✓
Delay Before Rising: 0
Rise Speed: 1.0
```

### Make it More/Less Dangerous
```
Damage Per Second: 3 (more dangerous)
Damage Interval: 0.5 (damage more frequently)
Max Y: 7 (reaches higher)
```

### Phase 2 Rise Speed Multiplier
In the boss script, you can change the multiplier:
```csharp
risingOil.IncreaseRiseSpeed(3f); // 3x faster instead of 2x
```

---

## Visual Enhancements (Optional)

### A. Animated Oil Surface
1. Add **Animator** to RisingOil
2. Create animation that:
   - Moves texture offset (looks like flowing)
   - Slightly scales Y up/down (looks like bubbling)

### B. Particle Effects
1. **Add Particle System** to RisingOil
2. **Settings**:
   - Start Size: 0.1-0.3
   - Start Color: Dark brown
   - Emission: 10 per second
   - Shape: Box (width matches oil width)
   - Gravity: -0.1 (bubbles float up)
   - Position: At top of oil surface

### C. Sound Effects
1. Add **AudioSource** to RisingOil
2. Loop a **bubbling/gurgling sound**
3. Play warning sound when oil starts rising

---

## Debug Gizmos

In Scene View (with RisingOil selected):
- **Brown line**: Starting Y position (-10)
- **Red line**: Maximum Y position (5)
- **Yellow box**: Current oil level

---

## Troubleshooting

**Oil doesn't rise:**
- ✓ Check `Start Rising Immediately` is checked OR wait for delay
- ✓ Check `Rise Speed` > 0
- ✓ Make sure GameObject Y position matches `Start Y` value

**Player doesn't take damage:**
- ✓ Check player has `Player` tag
- ✓ Check player has `Health` component
- ✓ Check Box Collider 2D is a trigger
- ✓ Check player is actually below oil level

**Oil doesn't rise faster in Phase 2:**
- ✓ Check boss script has `risingOil` reference assigned
- ✓ Check boss reaches 50% health (250 HP)
- ✓ Look for console message: "🛢️ The oil rises faster!"

**Oil is invisible:**
- ✓ Check Sprite Renderer has a sprite assigned
- ✓ Check sprite scale is large enough (X: 50+, Y: 20+)
- ✓ Check color alpha is not 0
- ✓ Check Sorting Layer is visible

---

## Gameplay Tips

**For Players:**
- Start on low platforms
- Move to higher ground as oil rises
- Phase 2 requires quick movement to highest platforms
- Oil adds time pressure - can't stay in one place forever!
- Creates natural difficulty curve - fight gets harder over time

**Balancing:**
- If too easy: Increase rise speed or reduce platform count
- If too hard: Decrease rise speed or add more high platforms
- Sweet spot: Oil reaches mid-platforms by Phase 2 transition

---

## Advanced: Multiple Oil Speeds

You can make oil rise in stages:

```csharp
// In boss script, call at different health thresholds:
if (currentHealth < 400) // 80% health
    risingOil.SetRiseSpeed(0.7f);
    
if (currentHealth < 300) // 60% health
    risingOil.SetRiseSpeed(1.0f);
    
// Phase 2 already does 2x multiplier at 50%
```

This creates a gradual difficulty increase!

---

Enjoy your rising oil mechanic! 🛢️✨
