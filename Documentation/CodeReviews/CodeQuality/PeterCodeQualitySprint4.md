# Code Quality Review

**Author**: Peter Eberhard

**Date**: April 5th

**Sprint**: 4

**File(s) reviewed**: MAGIC.cs

**Author of file(s)**: Katya Liber

## Code Smell Analysis

### 1. Long Class

This class is just over the comfortable length, but the fact that it is well sectioned off into groups makes it much more maintainable.

### 2. Shotgun Surgery

This class actually significant reduces this problem across our code as it allows us to have a single source on strings.

## Hypothetical Change Analysis

**Proposed Change**: Change this to a JSON file

This is the current intended plan, it just takes a lot of startup work

1. Would allow us to change magic numbers without a recompile
2. Keeps data problems in the data

## Conclusion

Overall a very good class that has been needed for a long time.

---

[**Previous Page**](../README.md)
