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

    private void OnValidate()
    {
        if(rail == null)
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
        UpdatePosition();
    }

    void UpdatePosition()
    {
        if(tethered) 
            transform.position = rail.Spline.SamplePoint(t);
    }

    protected virtual void OnCollision(CircleCollision collision)
    {
        var other = collision.A != body ? collision.A : collision.B;
        if (Bubble.TryGetBubble(other.gameObject, out var bubble))
            if(CheckFilter(bubble))
                ProcessCollision(other, bubble, collision);
    }

    protected virtual bool CheckFilter(Bubble bubble)
    {
        return (bubble.type & filter) > 0;
    }

    protected virtual void ProcessCollision(CircleBody other, Bubble bubble, CircleCollision collision)
    {
        rail.Remove(other);
        other.useGravity = true;
    }
}
