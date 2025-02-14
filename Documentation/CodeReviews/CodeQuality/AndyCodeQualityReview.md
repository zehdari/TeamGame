# Code Quality Review

**Author**: Andy Yu  

**Date**: February 12, 2025  

**Sprint**: 2  

**Files reviewed**: MoveSystem.cs, AttackSystem.cs, BlockSystem.cs, JumpSystem.cs

**Author of files**: Katya Liber  

## Code Smell Analysis

### 1. You Don't Need It Anymore

- All code written has a purpose, so they are always used

### 2. Large Method

- All methods are concise and focused

### 3. Large Class

- The system is concise and under 100 lines of code

### 4. Switch Statements

- No switch statements were used in the system

### 5. Data Clumps

- Data is well-organized
- No repeated groups of parameters

### 6. Same Name Different Meaning

- Variable names describe its purpose in the code and helps with understandability

### 7. While Not Done Loop

- No while loops were used, only conditionals that were descriptive

### 8. Too Many Parameters

- Methods did not have an overload of parameters
- Methods are a good length

### 9. Asymmetrical Code

- File is fairly symmetrical with the other files in the project

### 10. Comments as Smell

- There are a few comments, but there was no case of comments being used to explain complex code
- Comments used for readability

## Conclusion

MoveSystem contains clean code with no significant code smells. The design allows for easy extension
and modification, without needing to modify exisiting systems. The system is focused and shows high cohesion.
The code is concise and has no signs of repeatability.

---

[**Previous Page**](../README.md)
