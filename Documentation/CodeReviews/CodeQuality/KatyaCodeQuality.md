# Code Quality Review

**Author**: Katya Liber  
**Date**: 2/12/2025  
**Sprint**: Sprint 2  
**File(s) reviewed**: GravitySystem  
**Author of file(s)**: Peter Eberhard  

## Code Smell Analysis

- Duplicated Code  
- Long Method  
- Large Class  
- Long Parameter List  
- Shotgun Surgery  
- Switch Statements  
- Lazy Class  
- Message Chains  
- Comments  
- Primitve Obsession

## Conclusion

Techincally there is duplicated code within the systems in general, not just this one. However, there is not really a way around a loop going through  
all entities in each system, unless we want to change our entire structure to have a single system that loops through the entities instead of repeating the same loop in each system, but that seems  
unecessary. Other than that, after going through all of my code smells, GravitySystem does not seem to exhibit any other issues.

---

[**Previous Page**](../README.md)
