# Code Quality Review

**Author**: Peter Eberhard

**Date**: March 5 2025

**Sprint**: 3

**File(s) reviewed**: PlayerDespawnSystem

**Author of file(s)**: Andy Yu

## Code Smell Analysis

### 1. Long Methods

- Extreamly short methods, very concice.
- No problems here

### 2. Long Class

-  Simmilar to above, does exactly what its meant to and nothing more.

### 3. Shotgun Surgery

- We have one magic number, but its centralised and its slated to be fixed

### 4. Data Clumps

- No problems here, window size is correctly one variable and isnt split into 2 or 4

### 5. Duplicated Code

- None found

### 6. Comments as Deoderant

- This class features no comments. However, it is quite readable and concise.

## Hypothetical Change Analysis

**Proposed Change**: I proposed a change to the class, removing colision and just using an offset from screen size

Andy managed to update the class before I even could finish writing this review, so the class can clearly support this.


## Conclusion

Overall this is a short and sweet class that does exactly what its meant to, with no extra weight.

---

[**Previous Page**](../README.md)
