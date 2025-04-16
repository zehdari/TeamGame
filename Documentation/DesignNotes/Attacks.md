# Attack Notes
---
**Thoughts**
- We're assuming two variables - direction and type of attack
- direction will be up, down, left, right
- type will be normal or special
- Let's restrict ourselves to these for now to keep art and complexity managable
while still being semi-true to smash

---
**How to do this**
- Make attack systems that deal with the spawning of the attack
- Figure out what type of attack by dealing with raw input events
- make a new type of event, AttackAction?
- Parallel system to InputMapping that sends these events out

---

