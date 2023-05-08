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
        while(available.Count > 0)
        {
            //Get random target rail
            int index = Random.Range(0, available.Count);
            var selected = available[index];
            //Mark rail as used
            available.RemoveAt(index);
            used.Add(selected);
            //Check if rail has space
            if (selected.HasSpace)
            {
                //Transfer bubble
                rail.Remove(other);
                selected.Add(other);
                bubble.TriggerAbility();
                break;
            }
        }

        if (available.Count == 0)
        {
            bool defeat = true;
            foreach (Rail rail in used)
                defeat &= !rail.HasSpace;

            if(defeat)
                Game.Lose("All lanes are full!");

            available.AddRange(used);
            used.Clear();
        }
    }
}
