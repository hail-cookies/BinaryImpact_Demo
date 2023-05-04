using UnityEngine;

public class CircleBody : MonoBehaviour
{
    [HideInInspector]
    public Vector2 LastPosition = Vector2.zero;
    [HideInInspector]
    public Vector2 CurrentPosition = Vector2.zero;
    [HideInInspector]
    public Vector2 Acceleration = Vector2.zero;

    public Vector2 Velocity
    {
        get { return (CurrentPosition - LastPosition) / CirclePhysics.DeltaTime; }
        set
        {
            LastPosition = CurrentPosition - value * CirclePhysics.DeltaTime;
        }
    }

    [SerializeField, Min(float.Epsilon)]
    float mass = 1.0f;
    public float Mass
    {
        get { return mass; } 
        set { mass = Mathf.Max(float.Epsilon, value); }
    }

    [SerializeField, Min(float.Epsilon)]
    float radius = 0.5f;
    public float Radius
    {
        get { return radius; }
        set { radius = Mathf.Max(float.Epsilon, value);}
    }

    [SerializeField, Range(0,1)]
    float friction = 0.01f;
    public float Friction
    {
        get { return friction; }
        set { friction = Mathf.Clamp01(value); }
    }

    public bool useGravity = true;
    public bool noClip = false;

    private void OnEnable()
    {
        CirclePhysics.AddBody(this);
        CurrentPosition = LastPosition = transform.position;
    }

    private void OnDisable()
    {
        CirclePhysics.RemoveBody(this);
    }
}
