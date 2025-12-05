# Puff Daddy Final Boss - Sprite Sheet Specification

## Overview
This document outlines the sprite requirements for the Puff Daddy final boss fight.

## Recommended Sprite Sheet Layout
- **Sheet Size**: 2048x2048 pixels (or 4096x4096 for high detail)
- **Individual Sprite Size**: 128x128 or 256x256 pixels per frame
- **Format**: PNG with transparency
- **Style**: Match your existing game art style

---

## Animation States & Frame Counts

### 1. IDLE Animation (6-8 frames)
- Puff Daddy floating/standing in neutral pose
- Subtle breathing or hovering motion
- Could include cigar smoke puffing
- **Loop**: Yes

### 2. MOVE/FLOAT Animation (4-6 frames)
- Moving left/right across the arena
- Could show him gliding or walking
- Cape/coat flowing behind
- **Loop**: Yes

### 3. ATTACK 1 - Projectile Attack (8-10 frames)
- Wind-up: 2-3 frames (raising hand, gathering energy)
- Release: 2-3 frames (firing projectile)
- Recovery: 2-3 frames (returning to idle pose)
- **Loop**: No

### 4. ATTACK 2 - Shockwave/Slam (10-12 frames)
- Wind-up: 3-4 frames (raising arms/jumping)
- Impact: 2-3 frames (hitting ground, big impact pose)
- Shockwave: 3-4 frames (energy radiating out)
- Recovery: 2-3 frames
- **Loop**: No

### 5. ATTACK 3 - Special/Ultimate (12-15 frames)
- Dramatic charge-up: 4-5 frames
- Attack execution: 5-6 frames
- Recovery: 3-4 frames
- Could be music notes, champagne bottles, money rain, etc.
- **Loop**: No

### 6. HURT/DAMAGE (3-4 frames)
- Recoil from taking damage
- Flash white or red tint (can be done in code)
- Quick animation
- **Loop**: No

### 7. DEATH Animation (10-15 frames)
- Dramatic death sequence
- Falling, disintegrating, or exploding
- Final pose at the end
- **Loop**: No

### 8. PHASE TRANSITION (Optional, 8-10 frames)
- Animation when boss reaches 50% health
- Transformation or power-up effect
- Gets more aggressive/changes attacks
- **Loop**: No

### 9. TELEGRAPH Animations (4-6 frames each)
- Visual warnings before big attacks
- Flashing, glowing, or charging effects
- Helps player know what attack is coming
- **Loop**: Yes (until attack fires)

---

## Theme-Specific Attack Ideas

### Music/Hip-Hop Themed:
- 🎵 Musical note projectiles
- 🎤 Microphone slam attack
- 💿 Spinning vinyl disc projectiles
- 🔊 Sound wave shockwave attacks
- 💰 Money/dollar bill rain

### Business Mogul Themed:
- 💼 Briefcase throw
- 📱 Phone call summons minions
- 🍾 Champagne bottle explosion
- 💎 Diamond projectile barrage
- ⚡ "Bad Boy Records" lightning attack

---

## Sprite Sheet Organization Template

```
Row 1: IDLE (frames 1-8)
Row 2: MOVE (frames 1-6) + HURT (frames 1-4)
Row 3: ATTACK_1 (frames 1-10)
Row 4: ATTACK_2 (frames 1-12)
Row 5: ATTACK_3 (frames 1-15)
Row 6: DEATH (frames 1-15)
Row 7: PHASE_TRANSITION (frames 1-10)
Row 8: TELEGRAPH animations
```

---

## Visual Style Guidelines

### Character Design Elements:
- **Clothing**: Suit, sunglasses, jewelry/chains, signature style
- **Color Palette**: 
  - Primary: Blacks, whites, golds
  - Accent: Purple, blue (Bad Boy Records colors)
  - Effects: Bright cyan, yellow for attacks
- **Size**: Boss should be 2-3x larger than player character
- **Posture**: Confident, powerful stance

### Animation Principles:
- **Anticipation**: Wind-up before attacks
- **Exaggeration**: Big, readable movements
- **Weight**: Boss should feel heavy and powerful
- **Timing**: Slower, more deliberate than player

---

## Technical Specifications for Unity

### Import Settings (Unity):
1. Texture Type: **Sprite (2D and UI)**
2. Sprite Mode: **Multiple**
3. Pixels Per Unit: **100** (or match your game)
4. Filter Mode: **Point (no filter)** for pixel art, **Bilinear** for smooth art
5. Compression: **None** or **Low Quality** for best quality
6. Max Size: **2048** or **4096**

### Sprite Slicing:
- Use Unity's **Automatic** or **Grid By Cell Count** slicing
- Set cell size to match your frame size (128x128 or 256x256)
- Leave padding between frames if needed (2-4 pixels)

---

## Where to Get Sprites Created

### Option 1: Commission an Artist
- Fiverr, Upwork, or ArtStation
- Cost: $50-$300+ depending on complexity
- Provide this spec document + reference images

### Option 2: AI Generation + Editing
- Tools: Midjourney, Leonardo.ai, Stable Diffusion
- Generate base poses, then edit/combine in Photoshop/GIMP
- Requires manual cleanup and animation work

### Option 3: Modify Existing Assets
- Find similar character sprite sheet on OpenGameArt/Itch.io
- Modify colors, add accessories to make it unique
- Free but limited by available assets

### Option 4: Pixel Art Tools (DIY)
- **Aseprite** ($20, best for pixel art animation)
- **Piskel** (free, web-based)
- **GIMP** (free, general purpose)
- Time investment: 10-40 hours depending on skill

---

## Animation Timing Reference (60 FPS)

```
IDLE: 0.1s per frame (6 FPS feel)
MOVE: 0.08s per frame (smoother movement)
ATTACKS: 
  - Wind-up: 0.1-0.15s per frame (readable telegraph)
  - Execution: 0.05-0.08s per frame (quick, impactful)
  - Recovery: 0.1s per frame
HURT: 0.05s per frame (quick flash)
DEATH: 0.15-0.2s per frame (dramatic finale)
```

---

## Code Integration Checklist

Once you have the sprite sheet:
- [ ] Import into Unity with correct settings
- [ ] Slice sprite sheet into individual frames
- [ ] Create Animator Controller for boss
- [ ] Create animation clips for each state
- [ ] Set up animation transitions
- [ ] Add animation events for attack triggers
- [ ] Test timing with boss AI script
- [ ] Add particle effects for attacks
- [ ] Add sound effects for each animation
- [ ] Polish and iterate

---

## Next Steps

1. **Choose your approach** (commission, AI + edit, DIY, or modify existing)
2. **Gather reference images** (Puff Daddy photos, similar boss sprites)
3. **Create or commission the sprite sheet** using this spec
4. **Import into Unity** and set up animations
5. **Integrate with boss AI script**

Would you like help with any specific step, like setting up the boss AI script or animation controller?
