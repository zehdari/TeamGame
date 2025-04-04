# Code Quality Review

**Author**: Brian Miller

**Date**: April 4, 2025  

**Sprint**: 4

**Files reviewed**: SoundManager, SoundSystem

**Author of files**: Brendan Cabungcal

## Code Smell Analysis

### 1. Data Clumps

- no issues here

### 2. Long Method

- all methods are succinct and short

### 3. Large Class

- the classes aren't more than 50 lines each so all good here

### 4. Duplicated Code

- The "handleX" methods are repeated but they will be removed

### 5. Shotgun Surgery

- If anything needs changed with Audio, these are the only two affected files so no issues here

### 6. Lazy Class

- nothing here

### 7. Speculative Generality

- nothing wrong here

### 8. Refused Bequest

- nothing here

### 9. Cyclomatic Complexity

- nothing here

### 10. Comments (as "deodorant")

- There's some commented out code in SoundSystem that should be removed but nothing else

## Hypothetical Change Analysis

**Proposed Change**: Merge the SoundSystem handle events into one, drive the sounds into a json to make it a data problem

This will make the sounds a data problem and it will be easier to add more sounds when there's just a sound (or list of sounds) and it's conditions

## Conclusion

Very good code.

---

[**Previous Page**](../README.md)
