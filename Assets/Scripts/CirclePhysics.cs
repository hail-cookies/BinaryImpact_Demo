using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePhysics : MonoBehaviour
{
    #region List<CircleBody> simulatedBodies
    static List<CircleBody> simulatedBodies = new List<CircleBody>();

    public static bool AddBody(CircleBody body)
    {
        if(body== null) return false;
        if(simulatedBodies.Contains(body)) return true;

        simulatedBodies.Add(body);
        return true;
    }

    public static bool RemoveBody(CircleBody body)
    {
        return simulatedBodies.Remove(body);
    }
    #endregion List<CircleBody> simulatedBodies

    #region Singleton
    static CirclePhysics _instance;
    public static CirclePhysics Instance
    {
        get
        {
            if(_instance == null)
            {
                GameObject go = new GameObject("CirclePhysics");
                _instance = go.AddComponent<CirclePhysics>();
            }

            return _instance;
        }
    }
    #endregion Singleton

    public Vector2 Gravity = new Vector2(0, -9.81f);
    public int substeps = 8;
    public Transform limit;
    public int worldSize = 10;
    public float gridScale = 1f;
    public int cellCapacity = 8;

    static float _dt;
    public static float DeltaTime { get { return _dt; } }

    public static CollisionGrid Grid { get; private set; }
    private void Awake()
    {
        gridScale = Mathf.Max(float.Epsilon, gridScale);
        worldSize = (int)(worldSize / gridScale);
        worldSize = Mathf.Max(1, worldSize);
        
        Grid = new CollisionGrid(
            worldSize, worldSize, gridScale, limit ? limit.position : transform.position, cellCapacity);
    }

    private void OnDrawGizmos()
    {
        if(Grid != null) 
            Grid.DrawGizmos();
    }

    private void FixedUpdate()
    {
        _dt = Time.fixedDeltaTime / (float)substeps;
        float limRadius = limit ? limit.localScale.x / 2f : -1;
        Vector3 limPosition = limit ? limit.position : Vector3.zero;

        for (int step = 0; step < substeps; step++)
        {
            PopulateGrid();
            ApplyGravity();
            EnforceLimit(limRadius, limPosition);
            CheckGrid();
            UpdatePosition(_dt);
        }

        UpdateTransform();
    }

    void PopulateGrid()
    {
        Grid.Clear();

        for (int i = 0; i < simulatedBodies.Count; i++)
            if (!simulatedBodies[i].noClip)
                Grid.Add(i, simulatedBodies[i].CurrentPosition);
    }

    void ApplyGravity()
    {
        foreach(var body in simulatedBodies)
            body.Acceleration += body.useGravity ? Gravity : Vector2.zero;
    }

    void EnforceLimit(float limRadius, Vector2 limPosition)
    {
        foreach(var body in simulatedBodies)
            if (limRadius > 0)
            {
                float radius = body.Radius;
                Vector2 delta = body.CurrentPosition - limPosition;
                float dist = delta.magnitude;
                if (dist > limRadius - radius)
                {
                    body.CurrentPosition = limPosition + (delta / dist) * (limRadius - radius);
                }
            }
    }
    
    void GetContacts(int idA, int start, CollisionGrid.Cell cell)
    {
        for(int i = start; i <= cell.Index; i++)
            SolveCollision(idA, cell.bodies[i]);
    }

    static Vector2Int
        downLeft = new Vector2Int(-1, -1),
        down = new Vector2Int(0, -1),
        downRight = new Vector2Int(1, -1),
        left = new Vector2Int(-1, 0),
        right = new Vector2Int(1, 0),
        upLeft = new Vector2Int(-1, 1),
        up = new Vector2Int(0, 1),
        upRight = new Vector2Int(1, 1);

    void CheckCell(CollisionGrid.Cell cell, Vector2Int coords)
    {
        //Iterate through all bodies in this cell
        for(int i = 0; i <= cell.Index; i++)
        {
            int idA = cell.bodies[i];
            //Compare against other bodies in this cell
            GetContacts(idA, 0, cell);
            //Check neighbouring cells
            GetContacts(idA, 0, Grid.GetCell(coords + downLeft));
            GetContacts(idA, 0, Grid.GetCell(coords + down));
            GetContacts(idA, 0, Grid.GetCell(coords + downRight));

            GetContacts(idA, 0, Grid.GetCell(coords + left));
            GetContacts(idA, 0, Grid.GetCell(coords + right));

            GetContacts(idA, 0, Grid.GetCell(coords + upLeft));
            GetContacts(idA, 0, Grid.GetCell(coords + up));
            GetContacts(idA, 0, Grid.GetCell(coords + upRight));
        }
    }

    void CheckGrid()
    {
        for (int x = 1; x < Grid.Width - 1; x++)
            for (int y = 1; y < Grid.Height - 1; y++)
            {
                Vector2Int coords = new Vector2Int(x, y);
                CheckCell(Grid.GetCell(coords), coords);
            }
    }

    void SolveCollision(int idA, int idB)
    {
        CircleBody A = simulatedBodies[idA];
        CircleBody B = simulatedBodies[idB];

        Vector2 delta = A.CurrentPosition - B.CurrentPosition;
        float sqrDist = delta.sqrMagnitude;
        float radius = A.Radius + B.Radius;

        if (sqrDist < radius * radius)
        {
            float dist = Mathf.Sqrt(sqrDist);
            Vector2 normal = dist == 0 ? Vector2.right : delta / dist;
            float penetration = radius - dist;

            float mass = A.Mass + B.Mass;
            float massRatio = A.Mass / mass;

            A.CurrentPosition += massRatio * penetration * normal;
            B.CurrentPosition -= (1f - massRatio) * penetration * normal;
        }
    }

    void UpdatePosition(float dt)
    {
        foreach (var body in simulatedBodies)
        {
            //Calculate velocity
            Vector2 velocity = body.CurrentPosition - body.LastPosition;
            //Update stored position
            body.LastPosition = body.CurrentPosition;
            //Update position (Verlet)
            body.CurrentPosition += velocity + body.Acceleration * dt * dt;
            //Reset acceleration
            body.Acceleration *= 0;
        }
    }

    void UpdateTransform()
    {
        foreach (var body in simulatedBodies)
        {
            body.transform.localScale = Vector3.one * body.Radius * 2f;
            body.transform.position = body.CurrentPosition;
        }
    }
}
