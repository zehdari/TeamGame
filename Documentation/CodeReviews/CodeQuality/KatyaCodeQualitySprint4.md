# Code Quality Review

**Author**: Katya Liber 

**Date**: April 3, 2025  

**Sprint**: 4

**File(s) reviewed**: AttackHitSystem, HitResolutionSystem, HitDetectionSystem

**Author of file(s)**: Ely Maddox

## Code Smell Analysis

### 1. Duplicated Code

No duplicated code.

### 2. Long Method

No methods over 25 lines.

### 3. Large Class

None of the classes over 100 lines.

### 4. Long Parameter List

Not over the limit for any of the methods.

### 5. Shotgun Surgery

IsBlocking could be unified into a higher level so none of the lower level systems need to change it.

### 6. Switch Statements

No switch statements.

### 7. Lazy Class

None

### 8. Message Chains

None

### 9. Comments

None

### 10. Primitve Obsession

None

## Hypothetical Change Analysis

**Proposed Change**: Propose a hypothetical change/feature: Attacks with dynamic hit boxes.

Hitboxes assume a single collision shape for the entirety of the duration of the attack, and this assumption breaks once we need the hitbox to change during the attack. This would require a major refactor to accomplish.

## Conclusion

Everything looks pretty good. No evident issues.

---

[**Previous Page**](../README.md)
