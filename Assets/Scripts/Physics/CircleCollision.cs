using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CircleCollision
{
    public CircleBody A { get; private set; }
    public CircleBody B { get; private set; }

    public CircleCollision(CircleBody A, CircleBody B)
    {
        this.A = A;
        this.B = B;
    }
}
