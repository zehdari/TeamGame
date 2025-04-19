# Code Quality Review

**Author**: Ely Maddox

**Date**: April 18, 2025

**Sprint**: 5

**File(s) reviewed**: Menu System

**Author of file**: Brian Miller

## Code Smell Analysis

### 1. Long Methods

- ExecuteMenuOption is doing a lot. It'd probably be best to split into multiple helper methods.

### 2. Long Class

- Lots of responsibilites, though everything is cohesive, so it may not be the worst thing.

### 3. Long Parameter List

- None Here

### 4. Comments (as deodorant)

- None Here

### 5. Switch Case

- None Here

### 6. Lazy class

- None Here

### 7. Feature Envy

- None Here

### 8. Shotgun Surgery

- None Here. All actions are mapped in dictionaries to specific functions that handle details of that action.

### 9. Data Clumps

- None Here

### 10. Duplicated Code

- None Here

## Hypothetical Change Analysis

**Proposed Change**: Adding buttons to the character select menu to support a new character

- This would be easy, as only a new button would need to be added in the json
- No issues would be expected



## Conclusion

The file as a whole is well written. All methods are small snippets of functionality that are only focused on accomplishing their one goal.

---

[**Previous Page**](../README.md)
