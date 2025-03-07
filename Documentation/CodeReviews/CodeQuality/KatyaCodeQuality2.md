# Code Quality Review

**Author**: Katya Liber 

**Date**: March 6, 2025  

**Sprint**: 3

**File(s) reviewed**: RawInputSystem

**Author of file(s)**: Peter Eberhard

## Code Smell Analysis

### 1. Duplicated Code

Just the getrightjoystick direction
and get left joystick direction have 
some duplicated code, not 
sure if you can just combine that into one.

### 2. Long Method

Handle triggers is longer than
25 lines but that might just 
be the way it needs to be.

### 3. Large Class

Same thing as long method but
once again, that may just be the way 
it needs to be.

### 4. Long Parameter List

Looks good!

### 5. Shotgun Surgery

None

### 6. Switch Statements

The get right/left directions have a lot of ifs and elses,
maybe that could be made into a dictionary but you 
already have a lot of dictionaries. That probably
just overcomplicates it.

### 7. Lazy Class

Absolutely not.

### 8. Message Chains

None

### 9. Comments

None

### 10. Primitve Obsession

None

## Hypothetical Change Analysis

**Proposed Change**: Propose a hypothetical change/feature

Also working with a switch controller. The groundwork
for using other controllers is there, but some of 
the specifics of how to do so may need to change.

## Conclusion

It looks good. I could never do it. You can try to implement
some of those suggestions, but not if it
overcomplicates things. 

---

[**Previous Page**](../README.md)
