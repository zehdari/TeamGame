namespace ECS.Components.Collision;

public struct Contact
{
    public Entity EntityA;
    public Entity EntityB;
    public Vector2 Normal;      // Contact normal from A to B
    public Vector2 Point;       // World space contact point
    public float Penetration;   // Penetration depth
    public float TimeOfImpact;  // Time of impact for CCD (0-1)
    public CollisionLayer LayerA;
    public CollisionLayer LayerB;
}