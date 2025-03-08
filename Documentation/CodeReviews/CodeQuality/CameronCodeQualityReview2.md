# Code Quality Review

**Author**: Cameron Tucker

**Date**: February 6, 2025  

**Sprint**: 3

**Files reviewed**: HitboxSystem.cs, HitSystem.cs

**Author of files**: Ely Maddox

## Code Smell Analysis

### 1. Duplicated Code

- Debug logging statements are duplicated across systems (But this is temporary)

### 2. Large Method

- `HandleCollisionEvent` in HitboxSystem performs too many operations at once
- `DealWithHitPhysics` mixes knockback, damage, and debugging concerns

### 3. Large Class

- While individual classes aren't large yet, all three show signs of expanding responsibilities

### 4. Switch Statements

- None

### 5. Data Clumps

- Position, velocity, and state components repeatedly accessed together, but this is normal for our ECS

### 6. Feature Envy

- HitSystem directly manipulates physics components that should be handled by physics systems
- HitboxSystem deeply interrogates AttackInfo data

### 7. Shotgun Surgery

- Attack mechanics are spread across three systems, requiring multi-file changes for simple additions

### 8. Primitive Obsession

- Magic numbers like `10000` in knockback calculations
- Boolean flags like `Force = true` lack meaning

### 9. Cyclomatic Complexity

- Nested conditions in event handlers can create complex logic as system responsibility grows
- Multiple early returns make logic somewhat difficult to follow

### 10. Divergent Change

- HitSystem handles both physical impacts and projectile removal, violating SRP (But projectile removal was a quick addition so I'm sure this will change)

## Hypothetical Change Analysis

**Proposed Change**: Add a status effect (slow, burn) attack type

The current implementation makes this difficult because:

1. Combat logic is fragmented across three systems with tight coupling
2. Attack types and effects lack proper abstraction
3. No existing mechanism for temporary status effects (Shouldn't be super hard to add though)

## Conclusion

The combat systems would benefit from refactoring to follow the existing ECS pattern more consistently. The hit systems should:

- Use EntityRegistry more
- Separate collision detection from hit resolution (Lean more on collision detection system)
- Better improve the event pipeline between systems
- Remove debugging code in favor of debug systems

---

[**Previous Page**](../README.md)
