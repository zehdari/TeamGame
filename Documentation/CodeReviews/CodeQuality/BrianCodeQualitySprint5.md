# Code Quality Review

**Author**: Brian Miller

**Date**: April 18, 2025  

**Sprint**: 5

**Files reviewed**: PlatformMoveSystem, PlatformPassengerSystem

**Author of files**: Peter Eberhard

## Code Smell Analysis

### 1. Data Clumps

- no issues here

### 2. Long Method

- Update in PlatformPassengerSystem is pretty long and it could probably be split into a couple extra methods

### 3. Large Class

- Nothing too bad in comparison to many of our other System classes

### 4. Duplicated Code

- Don't see any

### 5. Shotgun Surgery

- These are the only two files that would need to change if moving platforms needed an update

### 6. Lazy Class

- nothing here

### 7. Speculative Generality

- nothing wrong here

### 8. Refused Bequest

- nothing here

### 9. Cyclomatic Complexity

- Not too bad, the worse it gets is 6 indents deep

### 10. Comments (as "deodorant")

- comments used appropriately

## Hypothetical Change Analysis

**Proposed Change**: Make some helper methods for the Update methods

This will reduce cyclomatic complexity and make the code more succinct

## Conclusion

Very good code.

---

[**Previous Page**](../README.md)
