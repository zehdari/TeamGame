# Code Quality Review

**Author**: Brendan Cabungcal

**Date**: April 3, 2025  

**Sprint**: 4

**File reviewed**: ItemSystem.cs

**Author of file**: Andy Yu

## Code Smell Analysis

### 1. Code that never actually runs

- Makes the code longer and more complex than necessary
- Just delete the code and it will be easier to read

### 2. Comparing variables to null

- If not done carefully, it can lead to many issues, both syntactical and logical

### 3. Duplicate code

- Create a method
- Makes code longer than necessary

### 4. Methods too big

- We want simple, easy to read, code
- The method may be doing too much, extract another method from it
- Potentially, you could decompose conditionals

### 5. Strikingly similar subclasses

- Lots of duplicate code
- Could potentially move the common code into the parent class

### 6. Same name different meaning

- Overloaded vocabulary
- Leads to misinterpretation
- Reusing variable names is a sign that your function has run too long

### 7. Too many parameters

- Too complex
- Can be confusing especially if there are many parameters of the same type
- Could potentially have a mutable struct for some/all of the parameters

### 8. Variables with same name as type

- Make the name convey information about its contents
- Others reading your code will not know what the variable represents

### 9. Vague identifiers

- Method names too vague or ambiguous, therefore its funtion is not clear
- Could be a sign of a method having more than one function/responsibility

### 10. While not done loops

- Having the condition of the while just be a boolean variable set to false
- Shows the writer may not know the purpose of the loop or the exit condition
- Fixing this can make the code easier to read

## Hypothetical Change Analysis

**Proposed Change**: No changes to propose

## Conclusion

The ItemSystem.cs looks good, no sign of code smells

---

[**Previous Page**](../README.md)
