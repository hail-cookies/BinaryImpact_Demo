using System.Collections.Generic;
using UnityEngine;

public class SupplyExit : RailExit
{
    public List<Rail> rails = new List<Rail>();

    protected override void ProcessCollision(CircleBody other, Bubble bubble, CircleCollision collision)
    {
        rail.Remove(other);

        if (rails.Count > 0)
        {
            rails[Random.Range(0, rails.Count)].Add(other);
        }
    }
}
