using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTester : MonoBehaviour
{
    public int width = 10, height = 10;
    public float scale = 1f;
    public int test = 0;

    public float Radius { get { return Mathf.Min(width, height) * 0.5f; } }

    private void OnValidate()
    {
        grid = new CollisionGrid(
            width, height, scale, transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, Radius);
    }

    CollisionGrid grid;
    private void OnDrawGizmosSelected()
    {
        if(test > 0)
        {
            if (grid == null)
                grid = new CollisionGrid(
                    width, height, scale, transform.position);

            Vector2 offset = 
                test == 1 ? Random.insideUnitCircle * Radius :
                test == 2 ? new Vector2(-width * scale * 0.5f, -height * scale * 0.5f) :
                test == 3 ? new Vector2(width * scale * 0.5f, height * scale * 0.5f) :
                Vector2.zero;

            Vector2 position = (Vector2)transform.position + offset;
            grid.Test(position);
        }
    }
}
