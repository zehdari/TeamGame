# Readability Review

**Author**: Cameron Tucker

**Date**: February 6, 2025

**Sprint**: 2

**Files reviewed**: HitboxSystem.cs, HitSystem.cs

**Author of files**: Ely Maddox

**Time spent**: 30 minutes

## Readability Comments

### Positive Aspects

1. Clear method naming conventions throughout all files
2. Consistent ECS pattern usage with component references
3. Good use of early returns to avoid excessive nesting
4. Event-based communication between systems

### Areas for Improvement

1. Debug statements (`System.Diagnostics.Debug.WriteLine`) should be removed or replaced with proper logging
2. Magic numbers (`flippedContact.Y -= 1`, `10000` in knockback calculation) need constants or comments
3. Complex conditional branching in collision detection is hard to follow
4. Redundant code like `return attackerParent.Value == target.Id ? true : false;` instead of simply `return attackerParent.Value == target.Id;`

---

[**Previous Page**](../README.md)
