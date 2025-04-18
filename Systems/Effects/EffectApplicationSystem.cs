using ECS.Core;
using ECS.Events;
using ECS.Components.Items;
using ECS.Components.Timer;
using ECS.Components.Physics;
using ECS.Components.Animation;
using ECS.Components.Effects;

namespace ECS.Systems.Effects;

public class EffectApplicationSystem : SystemBase
{
    private GameStateManager gameStateManager;

    public EffectApplicationSystem(GameStateManager gameStateManager)
    {
        this.gameStateManager = gameStateManager;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ItemPickupEvent>(HandleItemPickup);
        Subscribe<TimerEvent>(HandleEffectTimers);
    }

    private void HandleItemPickup(IEvent evt)
    {
        var itemPickupEvent = (ItemPickupEvent)evt;
        var player = itemPickupEvent.Player;
        var item = itemPickupEvent.Item;
        var itemEntity = itemPickupEvent.ItemEntity;

        Logger.Log($"EffectApplicationSystem received pickup event for item '{item.Value}'");
        
        // Check if item has any effect components and apply them to the player
        ApplyEffectsFromItem(itemEntity, player);
    }

    private void ApplyEffectsFromItem(Entity itemEntity, Entity playerEntity)
    {
        // Check for each effect type and apply if present
        if (HasComponents<SpeedBoostEffect>(itemEntity))
        {
            var effect = GetComponent<SpeedBoostEffect>(itemEntity);
            ApplySpeedBoost(playerEntity, effect.Duration, effect.Magnitude);
        }
        
        if (HasComponents<JumpBoostEffect>(itemEntity))
        {
            var effect = GetComponent<JumpBoostEffect>(itemEntity);
            ApplyJumpBoost(playerEntity, effect.Duration, effect.Magnitude);
        }
        
        if (HasComponents<GravityReductionEffect>(itemEntity))
        {
            var effect = GetComponent<GravityReductionEffect>(itemEntity);
            ApplyGravityReduction(playerEntity, effect.Duration, effect.Magnitude);
        }
        
        if (HasComponents<ScaleChangeEffect>(itemEntity))
        {
            var effect = GetComponent<ScaleChangeEffect>(itemEntity);
            ApplyScaleChange(playerEntity, effect.Duration, effect.Magnitude);
        }
        
        if (HasComponents<MassChangeEffect>(itemEntity))
        {
            var effect = GetComponent<MassChangeEffect>(itemEntity);
            ApplyMassChange(playerEntity, effect.Duration, effect.Magnitude);
        }
        
        if (HasComponents<InvincibilityEffect>(itemEntity))
        {
            var effect = GetComponent<InvincibilityEffect>(itemEntity);
            ApplyInvincibility(playerEntity, effect.Duration, effect.Magnitude);
        }
    }

    // Effect Application Methods
    
    private void ApplySpeedBoost(Entity entity, float duration, float magnitude)
    {
        Logger.Log($"Applying speed boost: duration={duration}, magnitude={magnitude}");
        
        // Check if components exist
        if (!HasComponents<WalkForce>(entity) || !HasComponents<RunSpeed>(entity))
        {
            Logger.Log("Entity doesn't have required speed components");
            return;
        }
        
        // Store original values if not already stored
        StoreOriginalValues(entity);
        
        // Apply effect component
        var effect = new SpeedBoostEffect
        {
            Duration = duration,
            RemainingTime = duration,
            Magnitude = magnitude,
            IsApplied = false
        };
        
        World.GetPool<SpeedBoostEffect>().Set(entity, effect);
        
        // Apply the actual effect modifiers
        ref var walkForce = ref GetComponent<WalkForce>(entity);
        ref var runSpeed = ref GetComponent<RunSpeed>(entity);
        
        walkForce.Value *= magnitude;
        runSpeed.Scalar *= magnitude;
        
        // Update components
        World.GetPool<WalkForce>().Set(entity, walkForce);
        World.GetPool<RunSpeed>().Set(entity, runSpeed);
        
        // Mark as applied
        effect.IsApplied = true;
        World.GetPool<SpeedBoostEffect>().Set(entity, effect);
        
        // Ensure timer exists
        EnsureEffectTimer(entity);
    }
    
    private void ApplyJumpBoost(Entity entity, float duration, float magnitude)
    {
        Logger.Log($"Applying jump boost: duration={duration}, magnitude={magnitude}");
        
        if (!HasComponents<JumpForce>(entity))
        {
            Logger.Log("Entity doesn't have JumpForce component");
            return;
        }
        
        // Store original values
        StoreOriginalValues(entity);
        
        // Apply effect component
        var effect = new JumpBoostEffect
        {
            Duration = duration,
            RemainingTime = duration,
            Magnitude = magnitude,
            IsApplied = false
        };
        
        World.GetPool<JumpBoostEffect>().Set(entity, effect);
        
        // Apply the actual effect modifiers
        ref var jumpForce = ref GetComponent<JumpForce>(entity);
        jumpForce.Value *= magnitude;
        
        // Update component
        World.GetPool<JumpForce>().Set(entity, jumpForce);
        
        // Mark as applied
        effect.IsApplied = true;
        World.GetPool<JumpBoostEffect>().Set(entity, effect);
        
        // Ensure timer exists
        EnsureEffectTimer(entity);
    }
    
    private void ApplyGravityReduction(Entity entity, float duration, float magnitude)
    {
        Logger.Log($"Applying gravity reduction: duration={duration}, magnitude={magnitude}");
        
        if (!HasComponents<GravitySpeed>(entity))
        {
            Logger.Log("Entity doesn't have GravitySpeed component");
            return;
        }
        
        // Store original values
        StoreOriginalValues(entity);
        
        // Apply effect component
        var effect = new GravityReductionEffect
        {
            Duration = duration,
            RemainingTime = duration,
            Magnitude = magnitude,
            IsApplied = false
        };
        
        World.GetPool<GravityReductionEffect>().Set(entity, effect);
        
        // Apply the actual effect modifiers
        ref var gravitySpeed = ref GetComponent<GravitySpeed>(entity);
        gravitySpeed.Value *= magnitude;
        
        // Update component
        World.GetPool<GravitySpeed>().Set(entity, gravitySpeed);
        
        // Mark as applied
        effect.IsApplied = true;
        World.GetPool<GravityReductionEffect>().Set(entity, effect);
        
        // Ensure timer exists
        EnsureEffectTimer(entity);
    }
    
    private void ApplyMassChange(Entity entity, float duration, float magnitude)
    {
        Logger.Log($"Applying mass change: duration={duration}, magnitude={magnitude}");
        
        if (!HasComponents<Mass>(entity))
        {
            Logger.Log("Entity doesn't have Mass component");
            return;
        }
        
        // Store original values
        StoreOriginalValues(entity);
        
        // Apply effect component
        var effect = new MassChangeEffect
        {
            Duration = duration,
            RemainingTime = duration,
            Magnitude = magnitude,
            IsApplied = false
        };
        
        World.GetPool<MassChangeEffect>().Set(entity, effect);
        
        // Apply the actual effect modifiers
        ref var mass = ref GetComponent<Mass>(entity);
        mass.Value *= magnitude;
        
        // Update component
        World.GetPool<Mass>().Set(entity, mass);
        
        // Mark as applied
        effect.IsApplied = true;
        World.GetPool<MassChangeEffect>().Set(entity, effect);
        
        // Ensure timer exists
        EnsureEffectTimer(entity);
    }
    
    private void ApplyScaleChange(Entity entity, float duration, float magnitude)
    {
        Logger.Log($"Applying scale change: duration={duration}, magnitude={magnitude}");
        
        if (!HasComponents<Scale>(entity))
        {
            Logger.Log("Entity doesn't have Scale component");
            return;
        }
        
        // Store original values
        StoreOriginalValues(entity);
        
        // Apply effect component
        var effect = new ScaleChangeEffect
        {
            Duration = duration,
            RemainingTime = duration,
            Magnitude = magnitude,
            IsApplied = false
        };
        
        World.GetPool<ScaleChangeEffect>().Set(entity, effect);
        
        // Apply the actual effect modifiers
        ref var scale = ref GetComponent<Scale>(entity);
        scale.Value *= magnitude;
        
        // Update component
        World.GetPool<Scale>().Set(entity, scale);
        
        // Mark as applied
        effect.IsApplied = true;
        World.GetPool<ScaleChangeEffect>().Set(entity, effect);
        
        // Ensure timer exists
        EnsureEffectTimer(entity);
    }
    
    private void ApplyInvincibility(Entity entity, float duration, float magnitude)
    {
        Logger.Log($"Applying invincibility: duration={duration}");
        
        // Apply effect component
        var effect = new InvincibilityEffect
        {
            Duration = duration,
            RemainingTime = duration,
            Magnitude = magnitude,
            IsApplied = true
        };
        
        World.GetPool<InvincibilityEffect>().Set(entity, effect);
        
        // Ensure timer exists
        EnsureEffectTimer(entity);
    }
    
    // Effect Timer Management
    
    private void HandleEffectTimers(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;
        
        if (timerEvent.TimerType != TimerType.SpecialTimer)
            return;
            
        var entity = timerEvent.Entity;
        
        // Update all effects on this entity
        UpdateEffects(entity);
    }
    
    private void EnsureEffectTimer(Entity entity)
    {
        // Ensure the entity has a Timers component
        if (!HasComponents<Timers>(entity))
        {
            Logger.Log($"Creating new Timers component for entity {entity.Id}");
            World.GetPool<Timers>().Set(entity, new Timers
            {
                TimerMap = new Dictionary<TimerType, Timer>()
            });
        }
        
        ref var timers = ref GetComponent<Timers>(entity);
        
        // Create a timer for effects if it doesn't exist
        if (!timers.TimerMap.ContainsKey(TimerType.SpecialTimer))
        {
            Logger.Log($"Creating new SpecialTimer for entity {entity.Id}");
            timers.TimerMap[TimerType.SpecialTimer] = new Timer
            {
                Duration = 0.1f, // Update effects every 0.1 seconds
                Elapsed = 0f,
                Type = TimerType.SpecialTimer,
                OneShot = false
            };
            
            // Explicitly update the component
            World.GetPool<Timers>().Set(entity, timers);
        }
    }
    
    private void UpdateEffects(Entity entity)
    {
        // Check all effect types and update their timers
        bool hasAnyEffect = false;
        
        // Update SpeedBoost effect
        if (HasComponents<SpeedBoostEffect>(entity))
        {
            hasAnyEffect = true;
            UpdateEffectTime<SpeedBoostEffect>(entity, RemoveSpeedBoost);
        }
        
        // Update JumpBoost effect
        if (HasComponents<JumpBoostEffect>(entity))
        {
            hasAnyEffect = true;
            UpdateEffectTime<JumpBoostEffect>(entity, RemoveJumpBoost);
        }
        
        // Update GravityReduction effect
        if (HasComponents<GravityReductionEffect>(entity))
        {
            hasAnyEffect = true;
            UpdateEffectTime<GravityReductionEffect>(entity, RemoveGravityReduction);
        }
        
        // Update MassChange effect
        if (HasComponents<MassChangeEffect>(entity))
        {
            hasAnyEffect = true;
            UpdateEffectTime<MassChangeEffect>(entity, RemoveMassChange);
        }
        
        // Update ScaleChange effect
        if (HasComponents<ScaleChangeEffect>(entity))
        {
            hasAnyEffect = true;
            UpdateEffectTime<ScaleChangeEffect>(entity, RemoveScaleChange);
        }
        
        // Update Invincibility effect
        if (HasComponents<InvincibilityEffect>(entity))
        {
            hasAnyEffect = true;
            UpdateEffectTime<InvincibilityEffect>(entity, RemoveInvincibility);
        }
        
        // If no effects remain, remove the timer
        if (!hasAnyEffect && HasComponents<Timers>(entity))
        {
            ref var timers = ref GetComponent<Timers>(entity);
            if (timers.TimerMap.ContainsKey(TimerType.SpecialTimer))
            {
                Logger.Log("No more active effects - removing SpecialTimer");
                timers.TimerMap.Remove(TimerType.SpecialTimer);
                
                // Update the Timers component
                World.GetPool<Timers>().Set(entity, timers);
                
                // If the timer map is now empty, remove the Timers component
                if (timers.TimerMap.Count == 0)
                {
                    World.GetPool<Timers>().Remove(entity);
                }
            }
            
            // Also clean up OriginalValues if they exist
            if (HasComponents<OriginalValues>(entity))
            {
                World.GetPool<OriginalValues>().Remove(entity);
            }
        }
    }
    
    private void UpdateEffectTime<T>(Entity entity, Action<Entity> removeAction) 
        where T : struct, IEffectBase
    {
        ref var effect = ref GetComponent<T>(entity);
        
        effect.RemainingTime -= 0.1f; // Matches timer interval
        
        // Check if the effect has expired
        if (effect.RemainingTime <= 0)
        {
            Logger.Log($"Effect {typeof(T).Name} has expired - removing effect modifiers");
            
            // Remove the effect's modifications
            removeAction(entity);
            
            // Remove the effect component
            World.GetPool<T>().Remove(entity);
        }
        else
        {
            // Update the effect component
            World.GetPool<T>().Set(entity, effect);
        }
    }
    
    // Effect Removal Methods
    
    private void RemoveSpeedBoost(Entity entity)
    {
        if (!HasComponents<WalkForce>(entity) || !HasComponents<RunSpeed>(entity))
            return;
            
        // Restore original values
        RestoreOriginalValue<WalkForce>(entity);
        RestoreOriginalValue<RunSpeed>(entity);
    }
    
    private void RemoveJumpBoost(Entity entity)
    {
        if (!HasComponents<JumpForce>(entity))
            return;
            
        // Restore original values
        RestoreOriginalValue<JumpForce>(entity);
    }
    
    private void RemoveGravityReduction(Entity entity)
    {
        if (!HasComponents<GravitySpeed>(entity))
            return;
            
        // Restore original values
        RestoreOriginalValue<GravitySpeed>(entity);
    }
    
    private void RemoveMassChange(Entity entity)
    {
        if (!HasComponents<Mass>(entity))
            return;
            
        // Restore original values
        RestoreOriginalValue<Mass>(entity);
    }
    
    private void RemoveScaleChange(Entity entity)
    {
        if (!HasComponents<Scale>(entity))
            return;
            
        // Restore original values
        RestoreOriginalValue<Scale>(entity);
    }
    
    private void RemoveInvincibility(Entity entity)
    {
        // No component to restore, just remove the effect
        World.GetPool<InvincibilityEffect>().Remove(entity);
    }
    
    // Original Value Management
    
    private void StoreOriginalValues(Entity entity)
    {
        // Initialize the original values component if it doesn't exist
        if (!HasComponents<OriginalValues>(entity))
        {
            World.GetPool<OriginalValues>().Set(entity, new OriginalValues
            {
                Values = new Dictionary<Type, object>()
            });
            
            // Store original values for potential components
            if (HasComponents<WalkForce>(entity))
                StoreOriginalValue<WalkForce>(entity);
                
            if (HasComponents<RunSpeed>(entity))
                StoreOriginalValue<RunSpeed>(entity);
                
            if (HasComponents<JumpForce>(entity))
                StoreOriginalValue<JumpForce>(entity);
                
            if (HasComponents<GravitySpeed>(entity))
                StoreOriginalValue<GravitySpeed>(entity);
                
            if (HasComponents<Mass>(entity))
                StoreOriginalValue<Mass>(entity);
                
            if (HasComponents<Scale>(entity))
                StoreOriginalValue<Scale>(entity);
        }
    }
    
    private void StoreOriginalValue<T>(Entity entity) where T : struct
    {
        if (!HasComponents<OriginalValues>(entity))
            return;
            
        ref var originalValues = ref GetComponent<OriginalValues>(entity);
        
        // Only store if not already stored
        if (!originalValues.Values.ContainsKey(typeof(T)))
        {
            originalValues.Values[typeof(T)] = GetComponent<T>(entity);
            
            // Update the component
            World.GetPool<OriginalValues>().Set(entity, originalValues);
        }
    }
    
    private void RestoreOriginalValue<T>(Entity entity) where T : struct
    {
        if (!HasComponents<OriginalValues>(entity) || !HasComponents<T>(entity))
            return;
            
        ref var originalValues = ref GetComponent<OriginalValues>(entity);
        
        if (originalValues.Values.TryGetValue(typeof(T), out var value) && value is T typedValue)
        {
            // Restore the original value
            World.GetPool<T>().Set(entity, typedValue);
            
            Logger.Log($"Restored original value for {typeof(T).Name}");
            
            // Remove from the tracking dictionary
            originalValues.Values.Remove(typeof(T));
            
            // Update the component
            World.GetPool<OriginalValues>().Set(entity, originalValues);
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}