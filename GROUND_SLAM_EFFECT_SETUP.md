# Ground Slam Effect - Setup Guide

## Quick Setup (5 minutes)

### Method 1: Simple Circle Sprite (Easiest)

1. **Create the Prefab:**
   - Right-click in Hierarchy → Create Empty → Name it "GroundSlamEffect"
   - Add Component → Sprite Renderer
   - Add Component → GroundSlamEffect script

2. **Create a Circle Sprite:**
   - In Unity, right-click in Project → Create → Sprites → Circle
   - Assign this to the Sprite Renderer

3. **Configure Sprite Renderer:**
   - Color: Red or Orange (RGB: 255, 100, 50)
   - Sorting Layer: Make sure it's above ground but below player
   - Material: Default (or use a glowing material)

4. **Configure GroundSlamEffect Script:**
   - Expansion Speed: 10
   - Max Radius: 8
   - Duration: 0.8
   - Damage: 2
   - Target Layers: Select "Player" layer
   - Damage Once: Check ✓
   - Shockwave Renderer: Drag the Sprite Renderer component

5. **Add Audio Source (Optional):**
   - Add Component → Audio Source
   - Uncheck "Play On Awake"

6. **Save as Prefab:**
   - Drag from Hierarchy to Project folder
   - Delete from Hierarchy
   - Assign to PuffDaddyBoss script's "Microphone Slam Effect" field

---

## Method 2: With Particle Effects (Better Visual)

### Step 1: Create Base GameObject
```
GroundSlamEffect (Empty GameObject)
├── ShockwaveSprite (Sprite Renderer - expanding circle)
├── ImpactParticles (Particle System - explosion)
└── DustParticles (Particle System - ground dust)
```

### Step 2: Setup Shockwave Sprite
- Add Sprite Renderer to child GameObject
- Use Circle or Ring sprite
- Color: Orange/Red with transparency
- Set to expand from scale (0,0,0) to full size

### Step 3: Create Impact Particles
1. Create Particle System called "ImpactParticles"
2. **Main Module:**
   - Duration: 0.5
   - Looping: OFF
   - Start Lifetime: 0.3-0.5
   - Start Speed: 5-10
   - Start Size: 0.5-1.0
   - Start Color: Orange to Yellow gradient
   - Play On Awake: OFF

3. **Emission:**
   - Rate over Time: 0
   - Bursts: 1 burst, Count: 20-30 particles

4. **Shape:**
   - Shape: Circle
   - Radius: 1
   - Emit from Edge

5. **Color over Lifetime:**
   - Gradient: Start bright → Fade to transparent

### Step 4: Create Dust Particles
1. Create Particle System called "DustParticles"
2. **Main Module:**
   - Duration: 0.8
   - Start Lifetime: 0.5-1.0
   - Start Speed: 2-5
   - Start Size: 0.8-1.5
   - Start Color: Brown/Grey
   - Play On Awake: OFF

3. **Emission:**
   - Bursts: 1 burst, Count: 15-20 particles

4. **Shape:**
   - Shape: Circle
   - Radius: 2

### Step 5: Assign References
In GroundSlamEffect script:
- Shockwave Renderer: Drag ShockwaveSprite's Sprite Renderer
- Impact Particles: Drag ImpactParticles
- Dust Particles: Drag DustParticles

---

## Method 3: Using Animation (Most Control)

### Create Animated Sprite Sheet:
1. **Create sprite sheet** with 6-8 frames of shockwave expanding
2. **Import to Unity** and slice
3. **Create Animation:**
   - Select GroundSlamEffect GameObject
   - Window → Animation → Animation
   - Create New Animation: "Shockwave_Expand"
   - Add sprite frames (0.1s per frame)
   
4. **Animation Settings:**
   - Loop: OFF
   - Play on Awake: ON
   - Destroy object when done

---

## Quick Placeholder (30 seconds)

If you just want to test the effect quickly:

1. Create Empty GameObject: "GroundSlamEffect"
2. Add Sprite Renderer
3. Set Sprite to Circle (built-in)
4. Set Color to Red (255, 0, 0, 100)
5. Add GroundSlamEffect script
6. Set Max Radius: 8
7. Set Target Layers to "Player"
8. Save as prefab

Done! The script handles the expansion and fading automatically.

---

## Pro Tips

### Make it Look Better:
1. **Use a glowing material:**
   - Create Material → Shader: Sprites/Default
   - Or use Shader Graph for custom glow

2. **Add screen shake:**
   - Call CameraShake when effect spawns
   - Small intensity (0.3-0.5), short duration (0.3s)

3. **Layer multiple effects:**
   - Fast inner ring (bright)
   - Slow outer ring (faded)
   - Ground cracks sprite

4. **Sound design:**
   - Low frequency "BOOM" for impact
   - Rumbling for shockwave
   - Slight bass boost

### Performance:
- Keep particle count low (< 50 particles)
- Use simple shaders
- Destroy effect after it's done (auto-handled)

---

## Color Schemes

### Powerful Impact:
- Bright Orange → Red → Black
- Use for heavy attacks

### Energy Wave:
- Cyan → Blue → Transparent
- Use for magic/energy attacks

### Earthquake:
- Brown → Grey → Transparent
- Use for ground-based attacks

### Boss Themed (Puff Daddy):
- Gold → Purple → Black
- Matches "Bad Boy Records" colors

---

## Testing

1. **In Scene:**
   - Drag prefab into Hierarchy
   - Press Play
   - Should expand and fade automatically

2. **With Boss:**
   - Assign to PuffDaddyBoss "Microphone Slam Effect"
   - Trigger boss slam attack
   - Check Console for "Ground Slam Effect spawned!" and damage logs

3. **Debugging:**
   - Gizmos show max radius (red circle)
   - During play, yellow circle shows current radius
   - Enable Gizmos in Scene view to see

---

## Integration with Boss

The PuffDaddyBoss script already has this code:
```csharp
if (microphoneSlamEffect != null)
{
    Instantiate(microphoneSlamEffect, transform.position, Quaternion.identity);
}
```

Just assign your GroundSlamEffect prefab to the "Microphone Slam Effect" field in the Inspector!

---

## Common Issues

**Effect not visible:**
- Check Sorting Layer (should render above ground)
- Check Sprite Renderer is enabled
- Check alpha isn't 0

**No damage dealt:**
- Verify Target Layers includes "Player"
- Check player has Health component
- Look for damage logs in Console

**Effect too fast/slow:**
- Adjust Duration (0.5-1.0 seconds works well)
- Adjust Expansion Speed (5-15 range)

**Effect too small/large:**
- Adjust Max Radius (match boss script's shockwaveRadius)
- Default is 8 units

---

Ready to use! The simplest method (Method 1) will work perfectly for testing, and you can always upgrade to fancier effects later! 💥
