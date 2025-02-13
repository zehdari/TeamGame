# Code Quality Review

**Author**: Cameron Tucker  

**Date**: February 12, 2025  

**Sprint**: 2  

**Files reviewed**: ProjectileSystem.cs, ProjectileShootingSystem.cs, ProjectileSpawningSystem.cs, DespawnSystem.cs (Projectile systems)

**Author of files**: Ely Maddox  

## Code Smell Analysis

### 1. Duplicated Code

- Each system has unique implementation
- No repeated logic across files
- Component checks are necessarily similar but not duplicated logic

### 2. Large Method

- All methods are concise and focused

### 3. Large Class

- Each system is focused and minimal:
  - ProjectileSystem handles lifetime
  - ProjectileShootingSystem handles shooting flags
  - ProjectileSpawningSystem handles creation
  - DespawnSystem handles cleanup

### 4. Switch Statements

- Logic flows through direct checks
- No complex conditionals
- Clean if/continue pattern in checks

### 5. Data Clumps

- Data is well-organized
- No repeated groups of parameters
- Event data is cohesive

### 6. Feature Envy

- Systems operate in their own domain
- No unncessary access of other system's data

### 7. Shotgun Surgery

- Changes are localized to specific systems
- Adding new projectile types only affects spawning

### 8. Primitive Obsession

- Types are appropriate for their use
- Data structures are well-chosen
- No overuse of primitive types

### 9. Cyclomatic Complexity

- Logic flows are straightforward
- Checks are flat and clear
- No nested decision complexity

### 10. Divergent Change

- Systems have clear and single responsibilities
- Changes are contained within relevant systems
- Good separation of concerns

## Hypothetical Change Analysis

**Proposed Change**: Add different types of projectiles with unique behaviors and effects

The current implementation supports this well because:

1. ProjectileSpawningSystem only cares about creation; new projectile types just need new components
2. ProjectileSystem handles lifetime through timer events regardless of projectile type
3. DespawnSystem will clean up any projectile type the same way
4. No existing systems would need modification to support new behaviors

## Conclusion

The projectile systems contain clean code with no significant code smells. The design allows for easy extension and modification, without needing to modify exisiting systems. Each system is focused and shows high cohesion with low coupling.
