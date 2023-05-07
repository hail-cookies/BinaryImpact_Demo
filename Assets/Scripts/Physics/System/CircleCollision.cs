using UnityEngine;

public struct CircleCollision
{
    public readonly CircleBody Self, Other;
    public readonly Vector2 Normal;
    public readonly float Created;


    public CircleCollision(CircleBody Self, CircleBody Other, Vector2 normal, float created)
    {
        this.Self = Self;
        this.Other = Other;
        this.Normal = normal;
        this.Created = created;
    }
}
