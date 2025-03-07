# Code Quality Review

**Author**: Brian Miller

**Date**: March 5, 2025  

**Sprint**: 3

**Files reviewed**: CollisionDetectionSystem, CollisionResponseSystem

**Author of files**: Cameron Tucker

## Code Smell Analysis

### 1. Data Clumps

- Pairs of entities are often used together but that's unavoidable with a collision system and these two files are the only place this occurs so not an issue

### 2. Long Method

- the HandleCollision method is almost 100 lines long
- the other methods are pretty succinct

### 3. Large Class

- the CollisionDetection class is 360 lines long but I don't think it would be worth splitting it into multiple classes

### 4. Duplicated Code

- some lines are repeated for two entities but it's not long enough to warrant change

### 5. Shotgun Surgery

- If anything needs changed with collision, these are the only two affected files so no issues here

### 6. Lazy Class

- nothing here

### 7. Speculative Generality

- the collision class has the option for expanding but isn't anticipating anything that for the future

### 8. Refused Bequest

- nothing here

### 9. Cyclomatic Complexity

- nothing here

### 10. Comments (as "deodorant")

- There's a lot of comments but not to cover up bad code

## Hypothetical Change Analysis

**Proposed Change**: Split up the CollisionDetectionSystem

The main reason for this is just that it's a pretty long class but it might not be worth the effort to do since it works well

## Conclusion

Very good code.

---

[**Previous Page**](../README.md)
