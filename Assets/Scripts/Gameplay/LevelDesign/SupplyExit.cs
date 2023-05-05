using System.Collections.Generic;
using UnityEngine;

public class SupplyExit : RailExit
{
    public List<Rail> rails = new List<Rail>();
    List<Rail> available = new List<Rail>();
    List<Rail> used = new List<Rail>();

    protected override void Inititalize()
    {
        base.Inititalize();
        available.AddRange(rails);
    }

    protected override void ProcessCollision(CircleBody other, Bubble bubble, CircleCollision collision)
    {
        rail.Remove(other);

        if(available.Count == 0)
        {
            available.AddRange(used);
            used.Clear();
        }

        int index = Random.Range(0, available.Count);
        var selected = available[index];
        available.RemoveAt(index);
        used.Add(selected);

        selected.Add(other);
    }
}
