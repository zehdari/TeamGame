
# Roles

## Tasks

- Easy collision, just on the floor
- Physics? For gravity and such
  - Movement system needs split up
  - Proposed:
    - GravityForceSystem
    - FrictionForceSystem
    - InputForceSystem
    - ForceAggregatorSystem (combines all forces)
    - VelocitySystem
    - PositionSystem (maybe split)
      - PhysicsPositionSystem (updates pos based on velocity)
      - KinematicPositionSystem (static objects/direct position changes like moving platforms/teleporting entites)

- States for different movements?
- Need to be able to shoot out pea
- Items
- We need health

### Cameron

- Collision
- GitHub management
- ECS Support
- Code Review

### Brendan

- Map w/o collisions

### Peter

- Movement
- Jumping, walking

### Brian

#### SPRITES

- Peashooter
- Bonk Choy
  
All need attacking, idle, walking, jumping. Block, run, slide come later.

#### TERRAIN

- Blocks
- Platforms
- Background?

#### PROJECTILES

- Pea
- shockwave?

#### ITEMS

- Sun
- Fertilizer
- Shovel? maybe not rn.
  
### Katya

- Jira/Planning
- Movement
  
### Ely

- AI
- Projectiles
  
### Andy

- PlayerStateSystems

---

[**Previous Page**](README.md)