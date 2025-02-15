# Code Quality Review

**Author**: Ely Maddox  

**Date**: February 13, 2025  

**Sprint**: 2

**File reviewed**: CollisionDetectionSystem.cs

**Author of file**: Cameron Tucker

## Code Smell Analysis

### 1. Long Methods

- Some methods are a bit long, but nothing excessive.
- Mostly because of the large amount of math needed to be done here.

### 2. Long Classes

- Verging on being too long, but everything done in CollisionDetection is related to collision detection.
- The math could possibly be pulled out into a CollisionUtilities class?

### 3. Long Parameter List

- Nothing to see here.

### 4. Shotgun Surgery

- Nothing to see here.

### 5. Data Clumps

- Nothing to see here.

### 6. Switch Case

-When deciding what shape we should do collision for, it seems a bit switch casey. This will really
be an issue when we go to add more shapes, but it's small enough for now. 

### 7. Comments

- Nothing to see here. All comments only help understanding. 

### 8. Feature Envy

- Nothing to see here.

### 9. Lazy Class

- Nothing to see here.

### 10. Duplicated Code

- Same comments as the 'switch' case.

## Hypothetical Change Analysis

**Proposed Change**: Propose a hypothetical change/feature

Back to the 'switch' case, there are n^2 types of combinations. This will scale not great once
more shapes get added. 

1. Reason 1
2. Reason 2
3. Reason 3

## Conclusion

Everything here is very good for the first draft of collision. I't may not scale super super well, 
but it works well.

---

[**Previous Page**](../README.md)