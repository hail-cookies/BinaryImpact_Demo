using UnityEngine;
public class LevelExit : RailExit
{
    protected override void ProcessCollision(CircleBody other, Bubble bubble, CircleCollision collision)
    {
        rail.Add(other);
    }
}
