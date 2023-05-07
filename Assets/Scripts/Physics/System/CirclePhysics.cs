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

    public static CircleBody GetBody(int index)
    {
        return simulatedBodies[index];
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
    [Range(0.01f,1f)]
    public float collisionResponse = 0.6f;
    public Transform limit;
    public int cellCapacity = 8;

    static float _dt;
    public static float DeltaTime { get { return _dt; } }

    public static CollisionGrid Grid { get; private set; }

    static int[] area;
    private void Awake()
    {
        int worldSizeX = (int)limit.localScale.x + 2;
        int worldSizeY = (int)limit.localScale.y + 2;

        Grid = new CollisionGrid(
            worldSizeX, worldSizeY, 1, limit ? limit.position : transform.position, cellCapacity);

        area = new int[9 * cellCapacity];
    }

    private void OnDrawGizmos()
    {
        if(Grid != null && !Application.isPlaying) 
            Grid.DrawGizmos();
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

    public static int[] GetCollisionArea(Vector2Int coords, bool debug, out int count)
    {
        coords.x = Mathf.Clamp(coords.x, 1, Grid.Width - 2);
        coords.y = Mathf.Clamp(coords.y, 1, Grid.Height - 2);
        
        count = 0;
        //Down
        count = ReadCell(area, count, Grid.GetCell(coords + downLeft));
        count = ReadCell(area, count, Grid.GetCell(coords + down));
        count = ReadCell(area, count, Grid.GetCell(coords + downRight));
        //Center
        count = ReadCell(area, count, Grid.GetCell(coords + left));
        count = ReadCell(area, count, Grid.GetCell(coords));
        count = ReadCell(area, count, Grid.GetCell(coords + right));
        //Up
        count = ReadCell(area, count, Grid.GetCell(coords + upLeft));
        count = ReadCell(area, count, Grid.GetCell(coords + up));
        count = ReadCell(area, count, Grid.GetCell(coords + upRight));

        return area;
    }

    public static bool CheckPoint(Vector2 position, out CircleBody hit) => CheckCircle(position, 0.0001f, out hit);
    public static bool CheckCircle(Vector2 position, float radius, out CircleBody hit)
    {
        hit = null;
        int[] area = GetCollisionArea(Grid.Discretize(position), true, out int count);

        for(int i = 0; i < count; i++)
        {
            var body = simulatedBodies[area[i]];
            if (!body.disableCollision && !body.isTrigger)
            {
                if (OverlapCircle(body, position, radius))
                {
                    hit = body;
                    return true;
                }
            }
        }

        return false;
    }

    public static bool OverlapCircle(CircleBody target, Vector2 center, float radius)
    {
        Vector2 delta = target.CurrentPosition - center;
        float sqrDist = delta.sqrMagnitude;
        float maxDist = target.Radius + radius;

        if (sqrDist <= maxDist * maxDist)
        {
            return Mathf.Sqrt(sqrDist) <= maxDist;
        }

        return false;
    }

    #region Collisions
    static int ReadCell(int[] result, int index, CollisionGrid.Cell cell)
    {
        for(int i = 0; i <= cell.Index; i++)
        {
            result[index] = cell.bodies[i];
            index++;
        }

        return index;
    }

    List<(int, int, Vector2)> collisions = new List<(int, int, Vector2)>();
    void SolveCollision(int idA, int idB)
    {
        CircleBody A = simulatedBodies[idA];
        CircleBody B = simulatedBodies[idB];

        Vector2 delta = A.CurrentPosition - B.CurrentPosition;
        float sqrDist = delta.sqrMagnitude;
        float radius = A.Radius + B.Radius;
        //Bodies are in contact
        if (sqrDist < radius * radius)
        {
            float dist = Mathf.Sqrt(sqrDist);
            Vector2 normal = dist == 0 ? Vector2.right : delta / dist;

            if (!A.isTrigger && !B.isTrigger)
            {
                float penetration = (radius - dist) * collisionResponse;

                float mass = A.Mass + B.Mass;
                float massRatio = A.Mass / mass;

                A.CurrentPosition += massRatio * penetration * normal;
                B.CurrentPosition -= (1f - massRatio) * penetration * normal;
            }

            AddCollision(idA, idB, normal);
        }
    }

    void AddCollision(int idA, int idB, Vector2 n)
    {
        bool exists = false;
        for(int i = 0; i < collisions.Count; i++)
        {
            var col = collisions[i];
            //Collision pair already exists
            if ((col.Item1 == idA && col.Item2 == idB) ||
                (col.Item1 == idB && col.Item2 == idA))
            {
                //Modify normal
                col.Item3 += n;
                collisions[i] = col;
                exists = true; break;
            }
        }
        //Add new collision
        if(!exists)
            collisions.Add((idA, idB, n));
    }
    #endregion Collisions

    #region Update
    private void FixedUpdate()
    {
        _dt = Time.fixedDeltaTime / (float)substeps;
        Vector2 limScale = limit.localScale;
        Vector3 limPosition = limit ? limit.position : Vector3.zero;

        collisions = new List<(int,int, Vector2)>();
        for (int step = 0; step < substeps; step++)
        {
            PopulateGrid();
            ApplyGravity();
            ApplyConstraints(limScale, limPosition);
            CheckGrid();
            UpdatePosition(_dt);
        }

        UpdateTransform();
        InvokeCollisions();

        foreach(var body in simulatedBodies)
            body.UpdateContacts();
    }

    void PopulateGrid()
    {
        Grid.Clear();

        for (int i = 0; i < simulatedBodies.Count; i++)
            if (!simulatedBodies[i].disableCollision)
                Grid.Add(i, simulatedBodies[i].CurrentPosition);
    }

    void ApplyGravity()
    {
        foreach (var body in simulatedBodies)
            body.Acceleration += body.useGravity ? Gravity : Vector2.zero;
    }

    void CheckGrid()
    {
        for (int x = 1; x < Grid.Width - 1; x++)
            for (int y = 1; y < Grid.Height - 1; y++)
            {
                Vector2Int coords = new Vector2Int(x, y);
                int[] area = GetCollisionArea(coords, false, out int count);
                var cell = Grid.GetCell(coords);
                for (int i = 0; i <= cell.Index; i++)
                {
                    int idA = cell.bodies[i];
                    for (int k = 0; k < count; k++)
                        if (area[k] != idA)
                            SolveCollision(idA, area[k]);
                }
            }
    }

    void ApplyConstraints(Vector2 limScale, Vector2 limPosition)
    {
        Vector2 halfScale = limScale * 0.5f;
        Vector2 min = limPosition - halfScale;
        Vector2 max = limPosition + halfScale;
        

        foreach (var body in simulatedBodies)
        {
            if (!body.isTrigger)
            {
                float radius = body.Radius;
                body.CurrentPosition.x =
                    Mathf.Clamp(body.CurrentPosition.x, min.x + radius, max.x - radius);
                body.CurrentPosition.y =
                    Mathf.Clamp(body.CurrentPosition.y, min.y + radius, max.y - radius);
            }

            body.ApplyConstraints();
        }
    }

    void UpdatePosition(float dt)
    {
        foreach (var body in simulatedBodies)
        {
            if (body.isTrigger)
                continue;

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
            body.transform.position = new Vector3(
                body.CurrentPosition.x,
                body.CurrentPosition.y,
                body.transform.position.z);
        }
    }

    void InvokeCollisions()
    {
        float time = Time.time;
        List<CircleBody> bodies = new List<CircleBody>();
        bodies.AddRange(simulatedBodies);
        foreach(var col in collisions)
        {
            var A = bodies[col.Item1];
            var B = bodies[col.Item2];
            var n = col.Item3.normalized;

            A.ReceiveCollision(A, B, n, time);
            B.ReceiveCollision(A, B, n, time);
        }
    }
    #endregion Update
}
