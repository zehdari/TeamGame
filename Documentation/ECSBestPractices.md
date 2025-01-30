# ECS Best Practices

## Components

- Keep them small and data-only
- Always use structs, never classes
- Name them clearly (Position, Health, etc.)
- Initialize all fields

## Systems

- One system = one job
- Always check if components exist before using them
- Handle related events in Initialize()
- Use appropriate execution phases (Input, Update, Render)

## Events

- Keep them small
- Always include Entity field
- Only use for discrete changes, not continuous state
- Unsubscribe when done

## Entity Creation

- Use EntityFactory
- Set good defaults
- Initialize all required components
- Don't create entities in systems

## Performance

- Use 'ref' when getting components
- Don't get components repeatedly in loops

## Tips

- Load resources at startup
- Handle errors
- Keep systems simple
- Test edge cases

## Common Mistakes

- Assuming components exist
- Forgetting to remove components
- Mixing logic and data
- Doing heavy work every frame
