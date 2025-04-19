# Code Quality Review

**Author**: Peter Eberhard

**Date**: April 19th

**Sprint**: 5

**File(s) reviewed**: ChimneySmokeSystem

**Author of file(s)**: Cameron Tucker

## Code Smell Analysis

### 1. Long Class

Barely long, but i think its mostly fine for a class like this with so many helpers.

### 2. Shotgun Surgery

Not present, very well put together code.

### 2. Data Clumps

None seen.

## Hypothetical Change Analysis

**Proposed Change**: Refactor the way we determine who is smoke and who is player

The lines of code 
        
        if (HasComponents<Smoke>(collision.Contact.EntityA))
        {
            smokeEntity = collision.Contact.EntityA;
            characterEntity = collision.Contact.EntityB;
        }
        else if (HasComponents<Smoke>(collision.Contact.EntityB))
        {
            smokeEntity = collision.Contact.EntityB;
            characterEntity = collision.Contact.EntityA;
        }

Feel very repetitive and i'm sure we could find a way to improve them, but im mostly nitpicking.

## Conclusion

Very good overall

---

[**Previous Page**](../README.md)
