# Homing Projectile Powerup - Setup Guide

## Overview
The homing projectile powerup system is now fully implemented! This system allows the player to collect a powerup that temporarily makes their projectiles home in on enemies.

## Files Created/Modified

### New Files:
1. **HomingProjectilePowerup.cs** - The pickup that activates homing
2. **HomingProjectileIndicator.cs** - UI indicator showing powerup is active
3. **HomingProjectile.cs** - Modified to add EnableHoming() method

### Modified Files:
1. **PlayerFocus.cs** - Added homing projectile tracking system
2. **PlayerAttackFreeAim.cs** - Integrated homing activation on projectile spawn

## Unity Setup Steps

### 1. Setup Your Projectile Prefabs
Add the `HomingProjectile` component to your projectile prefabs:
- Select your normal projectile prefab
- Add Component → HomingProjectile
- Configure settings:
  - **Speed**: 15 (projectile movement speed)
  - **Rotate Speed**: 200 (how fast it turns toward target)
  - **Detection Radius**: 10 (how far it can detect enemies)
  - **Enemy Layer**: Set to your boss/enemy layer (e.g., "Enemy")
  - **Life Time**: 4 (seconds before auto-destroy)

Repeat for secondary projectile prefab if you have one.

### 2. Create the Powerup GameObject
1. Create a new GameObject in your scene: `HomingPowerup`
2. Add a SpriteRenderer:
   - Assign a sprite (e.g., a glowing projectile icon)
3. Add a Collider2D (e.g., CircleCollider2D):
   - Set "Is Trigger" to true
   - Adjust size to match sprite
4. Add Component → HomingProjectilePowerup script
5. Configure settings:
   - **Duration**: 15 seconds (how long homing lasts)
   - **Respawns**: Check if you want it to respawn
   - **Respawn Time**: 45 seconds (if respawning)
   - **Rotation Speed**: 100 (visual rotation)
   - **Bob Speed**: 2 (up/down bobbing)
   - **Bob Height**: 0.3 (bobbing distance)
   - Optional: Add particle effects and collect sound

### 3. Setup UI Indicator
1. In your Canvas, create a new UI GameObject: `HomingIndicator`
2. Add an Image component:
   - Set sprite to a projectile icon
3. Add a TextMeshProUGUI child for the timer:
   - Name it "TimerText"
4. Add a CanvasGroup component to the parent
5. Add Component → HomingProjectileIndicator script
6. Configure references:
   - **Player Focus**: Drag your player's PlayerFocus component
   - **Icon Image**: Drag the Image component
   - **Timer Text**: Drag the TextMeshProUGUI
   - **Canvas Group**: Should auto-assign
7. Configure colors:
   - **Active Color**: Cyan (or any bright color)
   - **Inactive Color**: Gray
   - **Pulse Speed**: 2

### 4. Verify Player Setup
Make sure your player GameObject has:
- `PlayerFocus` component
- `PlayerAttackFreeAim` component with PlayerFocus reference assigned

### 5. Layer Setup
Ensure your boss/enemies are on the correct layer:
1. Select your boss GameObject
2. Set Layer to "Enemy" (or whatever you named it)
3. Go to HomingProjectile component on projectile prefabs
4. Set Enemy Layer mask to match

## How It Works

### System Flow:
1. Player touches the `HomingPowerup` GameObject
2. `HomingProjectilePowerup` calls `playerFocus.ActivateHomingProjectiles(15f)`
3. `PlayerFocus` sets `HasHomingProjectiles = true` and starts a 15-second timer
4. `HomingProjectileIndicator` UI appears with timer
5. When player fires, `PlayerAttackFreeAim` checks `HasHomingProjectiles`
6. If true, calls `homingProjectile.EnableHoming()` on spawned projectile
7. Projectile now tracks nearest enemy using `Physics2D.OverlapCircleAll`
8. After 15 seconds, timer expires and homing deactivates
9. If respawn enabled, powerup reappears after 45 seconds

### Key Features:
- ✅ Projectiles smoothly track nearest enemy
- ✅ Works with both normal and secondary attacks
- ✅ Timer-based duration system
- ✅ Visual UI feedback with countdown
- ✅ Event-driven updates (no constant polling)
- ✅ Respawnable powerup option
- ✅ Debug logging for troubleshooting

## Testing

1. **Test Powerup Collection:**
   - Play the game and touch the powerup
   - UI should appear with timer
   - Console should log: "Homing Projectiles activated for 15 seconds!"

2. **Test Homing Behavior:**
   - Fire projectiles near the boss
   - They should curve toward the boss
   - Console should log: "Homing enabled on normal projectile!"

3. **Test Timer Expiration:**
   - Wait 15 seconds
   - UI should disappear
   - Projectiles should no longer home
   - Console should log: "Homing Projectiles expired"

4. **Test Respawn:**
   - If enabled, powerup should reappear after 45 seconds
   - Console should log: "Homing Projectile powerup respawned!"

## Troubleshooting

**Projectiles not homing:**
- Check that projectile prefabs have `HomingProjectile` component
- Verify Enemy Layer is set correctly on HomingProjectile
- Ensure boss is on the correct layer
- Check console for "Homing enabled" logs

**Powerup not activating:**
- Verify player has "Player" tag
- Check that collider on powerup has "Is Trigger" enabled
- Ensure PlayerFocus reference is assigned

**UI not showing:**
- Check that Canvas is set up correctly
- Verify HomingProjectileIndicator has all references assigned
- Make sure CanvasGroup is present

**Homing too weak/strong:**
- Adjust `Rotate Speed` on HomingProjectile (higher = sharper turns)
- Adjust `Detection Radius` (larger = detects from farther away)

## Future Enhancements

Ideas for expanding the system:
- Multiple powerup tiers (longer duration, faster homing)
- Different projectile effects when homing is active
- Sound effects for homing projectiles
- Visual trail or particle effects
- Upgrade system to make homing permanent
- Homing strength/accuracy upgrades

## Code Architecture

The system follows the existing powerup pattern from infinite focus:
- **PlayerFocus**: Central state manager with timer and events
- **PowerupScript**: Collision detection and activation
- **IndicatorScript**: UI feedback via events
- **AttackScript**: Consumer that checks state and acts
- **ProjectileScript**: Behavior modifier with enable method

This makes it easy to add more powerups following the same pattern!
