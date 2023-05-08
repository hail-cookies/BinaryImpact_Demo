using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailTest : MonoBehaviour
{
    public Rail rail;

    Color color;
    public float t;
    public CircleBody current;
    private void Update()
    {
        if(rail)
        {
            t = rail.ProjectPoint(transform.position);
            rail.TryGetBody(t, out var tracked);
            Swap(tracked.Body);
        }
    }

    void Swap(CircleBody body)
    {
        if (current)
            current.Renderer.material.color = color;

        if(body)
        {
            color = body.Renderer.material.color;
            body.Renderer.material.color = Color.black;
        }

        current = body;
    }
}
