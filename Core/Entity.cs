namespace ECS.Core;

public readonly struct Entity
{
    public readonly int Id;
    public Entity(int id) => Id = id;
    public override int GetHashCode() => Id;
    public override bool Equals(object obj) => 
        obj is Entity other && other.Id == Id;
}