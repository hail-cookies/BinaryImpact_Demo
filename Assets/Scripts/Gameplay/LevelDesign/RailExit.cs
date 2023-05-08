using UnityEngine;

[RequireComponent(typeof(CircleBody))]
public class RailExit : MonoBehaviour
{
    public Rail rail;
    [Range(0f, 1f)]
    public float t = 0.5f;
    public bool tethered = true;
    public BubbleType filter;
    public bool canScore = false;

    CircleBody _body;
    public CircleBody Body 
    { 
        get 
        {
            if(_body== null)
                _body = GetComponent<CircleBody>();

            return _body;
        } 
    }

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
        Body.useGravity = false;
        Body.isTrigger = true;
        Body.SendCollisionEvents = true;
        Body.OnCollision += OnCollision;
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
            if (rail.Remove(other))
            {
                float randomDeviation = Random.Range(-3, 3);
                randomDeviation += randomDeviation < 0 ? -0.2f : 0.2f;

                other.useGravity = true;
                other.SetVelocity(other.Velocity.Rotate(randomDeviation));

                if (canScore)
                    Score.Add(gameObject, transform.position, bubble);
            }
        }
    }
}
