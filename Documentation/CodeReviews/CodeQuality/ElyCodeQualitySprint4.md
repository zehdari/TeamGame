# Code Quality Review

**Author**: Ely Maddox

**Date**: April 3, 2025

**Sprint**: 4

**File reviewed**: TerminalSystem.cs

**Author of file(s)**: Cameron Tucker

## Code Smell Analysis

**If its debug it can't smell... right... right? 😅🤫**

### 1. Long Methods

Some are long, but because of switches/long lists of things to do. This will probably
be shortened here soon. 

### 2. Long Classes

Maybe just a tad long. A little.

### 3. Long Parameter Lists

N/A

### 4. Comments as Deodorant

Good, comments were informative and helpful while only being places where needed.

### 5. Switch Case

There's a few, though none are bad uses. Colors could possibly be set into a dictionary instead.

### 6. Lazy Class

Definitely not.

### 7. Feature Envy

Also none here.

### 8. Shotgun Surgery

If any strings would need to be changed they would need to change within the code.
This may want to be a data problem?

### 9. Data Clumps

N/A

### 10. Duplicated Code

N/A

## Hypothetical Change Analysis

**Proposed Change**: Allow entities to be spawned

The current implementation allows this, as only a new mapping would need to be
added with a call to some "SpawnEvent."

## Conclusion

Put your conclusion here.

---

[**Previous Page**](../README.md)
