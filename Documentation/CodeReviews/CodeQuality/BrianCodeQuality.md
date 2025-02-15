# Code Quality Review

**Author**: Brian Miller

**Date**: February 14, 2025  

**Sprint**: 2

**File reviewed**: PlayerStateSystem.cs PlayerState.cs

**Author of file**: Andy Yu

## Code Smell Analysis

### 1. Data Clumps

- Everything seems pretty well organized, there's no fields/etc that require each other often

### 2. Long Method

- Most methods are pretty short (under 10-15 lines)
- The Update method is 34 lines so it's a bit long, however most of the lines are essential pieces.
- If it get's too long, the setting state part could be extracted into it's own method after the component checks and whatnot are done.

### 3. Large Class

- The class is 98 lines so it's just under the 100-150 lines.

### 4. Duplicated Code

- Each method is unique and logic is not repeated anywhere

### 5. Shotgun Surgery

- The only place I could see having this issue is with the PlayerState Enum since adding a new state means updating this and then updating how the current state is determined.
- However preexisting state handling shouldn't be affected when new states are added and these two files should be the only ones changed so probably not an issue.

### 6. Lazy Class

- The nature of ECS means entities are somewhat "lazy", but everything else is good
- Control is handed off only where necessary

### 7. Speculative Generality

- There's room for adding new states but nothing here is preemptive in doing so
- Planned states are already in the PlayerState enum but those should be implimented soon so not an issue

### 8. Refused Bequest

- PlayerStateSystem.cs inherits the abstract class SystemBase.
- The only overriden method is Initialize which calls the SystemBase version first so no issues there

### 9. Cyclomatic Complexity

- Most of the code is good
- Update() reaches 5 indentation levels which could grow in complexity when more states are added

### 10. Comments (as "deodorant")

- Comments are in appropriate places
- Variable and method names are all appropriate

## Hypothetical Change Analysis

**Proposed Change**: In future sprints, Update for PlayerStateSystem.cs will probably have to be split up to decrease length and complexity. Also implement more states

The current implementation supports this well because:

1. The logic in Update is straightforward
2. States can easily be added to the PlayerState Enum
3. Updating the player state in PlayerStateSystem won't impact other files or code
4. Preexisting systems won't need to be modified to accommodate this

## Conclusion

The Player State system has very good code with no notable code smells. The system should be extendable pretty easily without much impact on unrelated parts.

---

[**Previous Page**](../README.md)
