# Code Quality Review

**Author**: Cameron Tucker

**Date**: Apr 4, 2025  

**Sprint**: 4

**Files reviewed**: MenuSystem.cs

**Author of files**: Brian Miller

## Code Smell Analysis

### 1. Duplicated Code

- Menu navigation methods repeat similar logic (Increment/Decrement)
- Button activation code appears in multiple methods

### 2. Large Method

- Dictionary initialization in constructor is bloated with hard-coded actions
- `UpdateMenuActive` handles too many responsibilities

### 3. Large Class

- MenuSystem mixes input handling, state management and UI manipulation

### 4. Switch Statements

- None

### 5. Data Clumps

- UIMenu and UIMenu2D components consistently accessed together

### 6. Feature Envy

- Directly manipulates UIMenu components instead of using proper abstractions

### 7. Shotgun Surgery

- Adding new menu types requires changes across multiple methods

### 8. Primitive Obsession

- Magic constant STATE_CHANGE_COOLDOWN = 0.2f
- Manual bounds checking for menu indices

### 9. Cyclomatic Complexity

- Multiple nested conditions in `UpdateMenuActive`
- Complex conditional logic in event handlers

### 10. Divergent Change

- System handles both menu navigation and game state changes

## Hypothetical Change Analysis

**Proposed Change**: Add settings submenu with configurable options

The current implementation makes this difficult because:

1. Menu structure tightly coupled to action handlers
2. 2D menu logic mixed with standard menu handling

## Conclusion

MenuSystem needs refactoring to:

- Split responsibilities into separate systems
- Create flexible menu component structure
- Extract cooldown logic to reusable utility

---

[**Previous Page**](../README.md)
