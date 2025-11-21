# Infinite Focus Powerup - Setup Guide

## What Was Created

### 1. **PlayerFocus.cs** (Updated)
Added infinite focus mode support:
- `HasInfiniteFocus` property - Check if infinite focus is active
- `ActivateInfiniteFocus(duration)` method - Call this to grant infinite focus
- `OnInfiniteFocusChanged` event - Subscribe to know when infinite focus starts/stops
- Automatic timer that counts down and disables infinite focus

### 2. **InfiniteFocusPowerup.cs** (New)
The actual pickup item:
- Grants infinite focus for a set duration (default: 10 seconds)
- Optional respawn system
- Optional particle effects and sound
- Auto-rotating/floating animation

### 3. **InfiniteFocusIndicator.cs** (New - Optional)
UI element to show infinite focus is active:
- Shows/hides panel when infinite focus activates/deactivates
- Optional countdown timer text
- Optional progress bar
- Optional pulsing glow effect

### 4. **SimpleFocusBar.cs** (Updated)
Now shows special visual when infinite focus is active:
- Changes to yellow color (configurable)
- Pulses to make it obvious
- Bar stays full during infinite mode

## How to Set Up the Powerup

### Basic Setup (In Unity):

1. **Create the Powerup GameObject:**
   - Create a new GameObject (Right-click in Hierarchy > Create Empty)
   - Name it "InfiniteFocusPowerup"
   - Add a Sprite Renderer (give it a cool icon sprite)
   - Add a Circle Collider 2D (or Box Collider 2D)
   - Make sure "Is Trigger" is checked on the collider
   - Add the `InfiniteFocusPowerup` script

2. **Configure the Script:**
   - Duration: 10 seconds (how long infinite focus lasts)
   - Respawns: Check if you want it to respawn
   - Respawn Time: 30 seconds (time until it comes back)

3. **Optional - Add Visual Effects:**
   - Pickup Effect: Drag a particle system prefab here
   - Pickup Sound: Drag an audio clip here

4. **Place in Your Level:**
   - Drag the powerup into your scene
   - Position it where you want players to find it

### Optional UI Setup:

**To add the "Infinite Focus Active" indicator:**

1. Create a UI Panel:
   - Right-click in Hierarchy > UI > Panel
   - Name it "InfiniteFocusIndicator"
   - Style it (make it glow, add a border, etc.)
   - Position it somewhere visible (top of screen works well)

2. Optional - Add Timer Text:
   - Right-click on the panel > UI > Text - TextMeshPro
   - Name it "TimerText"

3. Add the Script:
   - Add `InfiniteFocusIndicator` component to the panel
   - Assign the panel itself to "Indicator Panel"
   - Assign the timer text if you made one

## How It Works

### When Player Picks It Up:
1. Player touches the powerup
2. `ActivateInfiniteFocus(10)` is called on PlayerFocus
3. `HasInfiniteFocus` becomes true
4. All focus consumption returns true without draining focus
5. UI shows special visual (yellow pulsing bar)
6. Optional indicator appears showing countdown

### During Infinite Focus:
- Charge attacks don't consume focus
- Launch attacks don't consume focus
- Focus bar stays at 100% and pulses yellow
- Timer counts down each frame

### When Time Expires:
- `HasInfiniteFocus` becomes false
- Normal focus consumption resumes
- UI returns to normal colors
- Optional indicator hides

### If Respawn is Enabled:
- Powerup hides and waits
- After respawn time, it reappears
- Can be collected again

## Customization Tips

### Change Duration:
In `InfiniteFocusPowerup.cs`, adjust the `duration` field (default: 10 seconds)

### Make it One-Time Only:
Uncheck "Respawns" on the InfiniteFocusPowerup component

### Change UI Colors:
- In `SimpleFocusBar`: Change "Infinite Focus Color" (default: yellow)
- In `InfiniteFocusIndicator`: Change "Pulse Color 1" and "Pulse Color 2"

### Test It:
Run your game, walk over the powerup, and spam charge attacks - you'll never run out of focus!

## Quick Test Code (Optional)
If you want to test it with a key press, add this to any script:

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.I))
    {
        FindAnyObjectByType<PlayerFocus>()?.ActivateInfiniteFocus(10f);
    }
}
```

Press 'I' to activate infinite focus for 10 seconds!
