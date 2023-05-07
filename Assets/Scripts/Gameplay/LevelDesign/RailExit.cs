using UnityEngine;

[RequireComponent(typeof(CircleBody))]
public class RailExit : MonoBehaviour
{
    public Rail rail;
    [Range(0f, 1f)]
    public float t = 0.5f;
    public bool tethered = true;
    public BubbleType filter;
    protected CircleBody body;
    public CircleBody Body { get { return body; } }

    private void OnValidate()
    {
        if(rail == null && transform.parent)
            rail = transform.parent.GetComponent<Rail>();

        if(rail)
        {
            UpdatePosition();
        }    
    }

    private void Awake()
    {
        Inititalize();
    }

    protected virtual void Inititalize()
    {
        body = GetComponent<CircleBody>();
        body.useGravity = false;
        body.isTrigger = true;
        body.SendCollisionEvents = true;

        body.OnCollision += OnCollision;
    }

    private void Update()
    {
        if(rail)
            UpdatePosition();
    }

    void UpdatePosition()
    {
        if(tethered) 
            transform.position = rail.SamplePoint(t);
    }

    protected virtual void OnCollision(CircleCollision collision)
    {
        if (Bubble.TryGetBubble(collision.Other.gameObject, out var bubble))
            if(CheckFilter(bubble))
                ProcessCollision(collision.Other, bubble, collision);
    }

    protected virtual bool CheckFilter(Bubble bubble)
    {
        return (bubble.type & filter) > 0;
    }

    protected virtual void ProcessCollision(CircleBody other, Bubble bubble, CircleCollision collision)
    {
        if (rail)
        {
            rail.Remove(other);
            other.useGravity = true;
            other.SetVelocity(other.Velocity.Rotate(Random.Range(-1,1)));
        }
    }
}
