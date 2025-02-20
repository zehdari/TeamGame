# Code Quality Review

**Author**: Peter Eberhard  
**Date**: 2/13/2025  
**Sprint**: Sprint 2  
**File(s) reviewed**: AISystem  
**Author of file(s)**: Ely Maddox  

## Code Smell Analysis

- **Duplicated Code**  
  Very little, ECS by nature has some, but this code has the bare minimum.
- **Long parameter lists**  
  Longest Parameter list is 2, very clear for this one.
- **Cyclomatic Complexity**  
  All logic is very simple and readable, completely clear of this one.
- **Empty Catch Statements**  
  No catch statements to be empty.
- **Switch Statements**  
  No switch statements here.
- **Data Clumps**  
  By nature of an ECS we use the components togethers, but I think this is a feature and not a bug in this case, as coupling is still quite low.
- **Primitive Obsession**  
  Possibly should make actions their own entity or class rather than a string, but this code matches the current convention so nothing wrong here.
- **Shotgun Surgery**  
  Very low coupling here, seems good in this regard.
- **Long methods**  
  All methods are very short and readable.
- **Innapropirate Intimacy**  
  We know very little about other classes in this file.

## Conclusion

After going through all my code smells, the only possible change here would be to change how actions are handled away from strings. However, that is more a group effort than a problem with this particular code.

---

[**Previous Page**](../README.md)
