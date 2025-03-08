# Code Quality Review

**Author**: Ely Maddox 

**Date**: March 5, 2025  

**Sprint**: 3

**Files reviewed**: LevelLoader.cs, LevelSwitchSystem.cs, LevelLoaderSystem.cs

**Author of files**: Katya Liber

## Code Smell Analysis

### 1. Shotgun Surgery

-Not much detected, only thing is the magic strings with the names of levels, and the strings with different entity types. These should probably be moved into JSON to be a data issue.

### 2. Long Classes

- None Detected

### 3. Long Parameter List

- The MakeEntity delegate takes in lots of parameters. Still stays under 6, but it's edging on too many.

### 4. Long Methods

- Nothing is too long.

### 5. Comments as Deodorant

- None Detected, not many comments needed in the first place.

### 6. Lazy Class

- None Detected

### 7. Duplicated Code

- There might be a bit in level loader, but the different types of entities that need to be made kind of need to have neccessitates this

### 8. Switch Case

- None Detected

### 9. Data Clumps

- None Detected

### 10. Feature Envy

- None Detected

## Hypothetical Change Analysis

**Proposed Change**: Load in lots of levels

The current implementation supports this, as it loads in lists of entities from JSON. There would need to be changes with the magic strings, so this should likely be moved to data.

## Conclusion

Everything looked good! The main concern are the magic strings littered around the code, but these are present everywhere in the codebase at the moment, so it is not only an issue with these files. 

---

[**Previous Page**](../README.md)
