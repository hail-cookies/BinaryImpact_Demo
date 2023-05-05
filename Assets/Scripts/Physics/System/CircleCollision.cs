using UnityEngine;

public struct CircleCollision
{
    public readonly CircleBody A, B;
    public readonly Vector2 Normal;
    public readonly float Created;


    public CircleCollision(CircleBody A, CircleBody B, Vector2 normal, float created)
    {
        this.A = A;
        this.B = B;
        this.Normal = normal;
        this.Created = created;
    }
}
