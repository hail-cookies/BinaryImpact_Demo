using UnityEngine;

public class CollisionGrid
{
    public struct Cell
    {
        public int[] bodies;
        public int Index { get; private set; }
        public Cell(int capacity)
        {
            bodies = new int[capacity];
            Index = -1;
        }

        public void Add(int body)
        {
            Index = Mathf.Min(Index + 1, bodies.Length - 1);
            bodies[Index] = body;
        }

        public void Remove(int body)
        {
            for(int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] == body)
                {
                    bodies[i] = bodies[Index];
                    Index -= 1;

                    return;
                }
            }
        }

        public void Clear()
        {
            Index = -1;
        }
    }

    Cell[] cells;
    public int Width { get; private set; }
    public int Height { get; private set; }
    Vector2 offset = Vector2.zero;
    Vector2 maxPos;
    Vector2 _center = new Vector2(1,1);
    public Vector2 Center
    {
        get { return _center; }
        set
        {
            _center = value;
            offset = new Vector2(
                -Width * Scale * 0.5f,
                -Height * Scale * 0.5f) + _center;

            maxPos = new Vector2(
                Width * Scale, Height * Scale) + offset;
        }
    }
    
    float _scale = 1f;
    public float Scale
    {
        get { return _scale; }
        set { _scale = Mathf.Max(float.Epsilon, value); }
    }

    public CollisionGrid(int width, int height, float scale, Vector2 center, int capacity = 4)
    {
        this.Width = Mathf.Max(1, width) + 1;
        this.Height = Mathf.Max(1, height) + 1;
        this.Scale = scale;
        this.Center = center;

        cells = new Cell[Width * Height];
        for(int i = 0; i < cells.Length; i++)
            cells[i] = new Cell(capacity);
    }

    int GetIndex(Vector2Int position)
    {
        return Width * position.y + position.x;
    }

    public Vector2Int Discretize(Vector2 position)
    {
        Vector2 inverseTransform = (position - offset) / Scale;
        return new Vector2Int(
            Mathf.Min(Width - 1, (int)inverseTransform.x), 
            Mathf.Min(Height - 1, (int)inverseTransform.y));
    }

    public Cell GetCell(Vector2 position) 
    {
        position.x = Mathf.Clamp(position.x, offset.x, maxPos.x);
        position.y = Mathf.Clamp(position.y, offset.y, maxPos.y);
        return cells[GetIndex(Discretize(position))];
    }

    public Cell GetCell(Vector2Int coords)
    {
        return cells[GetIndex(coords)];
    }

    public Cell GetCell(int index)
    {
        return cells[index];
    }

    public void Add(int index, Vector2 position)
    {
        position.x = Mathf.Clamp(position.x, offset.x, maxPos.x);
        position.y = Mathf.Clamp(position.y, offset.y, maxPos.y);
        cells[GetIndex(Discretize(position))].Add(index);
    }

    public void Clear()
    {
        for(int i = 0; i < cells.Length; i++)
            cells[i].Clear();
    }

    public void Test(Vector2 position)
    {
        string msg = "TEST:\t\t\t" + position + "\n";
        msg += "OFFS:\t\t\t" + offset + "\n";
        position.x = Mathf.Clamp(position.x, offset.x, maxPos.x);
        position.y = Mathf.Clamp(position.y, offset.y, maxPos.y);
        msg += "CLAMP:\t\t\t" + position + "\n";
        var discrete = Discretize(position);
        msg += "INVRS:\t\t\t" + ((position - offset) / Scale) + "\n";
        msg += "DISCR:\t\t\t" + discrete + "\n";
        int index = GetIndex(discrete);
        msg += "COUNT:\t\t\t" + cells.Length + "\n";
        msg += "INDEX:\t\t\t" + index;

        Debug.Log(msg);
    }

    public void DrawGizmos()
    {
        for (int x = 0; x <= Width; x++)
        {
            if (x == 0 || x == Width)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(
                    offset + new Vector2(x * Scale, 0),
                    offset + new Vector2(x * Scale, Height * Scale));
            }
            else
                Gizmos.DrawLine(
                    offset + new Vector2(x * Scale, Scale),
                    offset + new Vector2(x * Scale, (Height - 1) * Scale));
        }
        for (int y = 0; y <= Height; y++)
        {
            if (y == 0 || y == Height)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(
                offset + new Vector2(0, y * Scale),
                offset + new Vector2(Width * Scale, y * Scale));
            }
            else
                Gizmos.DrawLine(
                offset + new Vector2(Scale, y * Scale),
                offset + new Vector2((Width - 1) * Scale, y * Scale));
        }
    }
}
