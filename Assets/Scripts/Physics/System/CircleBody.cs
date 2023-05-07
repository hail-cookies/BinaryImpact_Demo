using System;
using System.Collections.Generic;
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

    MeshRenderer _renderer;
    public MeshRenderer Renderer
    {
        get
        {
            if(!_renderer)
                _renderer = GetComponent<MeshRenderer>();

            return _renderer;
        }
    }

    Ownership<CircleBody> _ownership;
    public Ownership<CircleBody> Ownership
    {
        get
        {
            if (_ownership == null)
                _ownership = new Ownership<CircleBody>(this);

            return _ownership;
        }
    }

    public bool useGravity = true;
    public bool disableCollision = false;
    public bool isTrigger = false;

    private void OnEnable()
    {
        CirclePhysics.AddBody(this);
        CurrentPosition = LastPosition = transform.position;
    }

    private void OnDisable()
    {
        CirclePhysics.RemoveBody(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position, radius);
    }

    public delegate void Constraint(CircleBody body);
    public event Constraint OnApplyConstraints;
    public void ApplyConstraints() => OnApplyConstraints?.Invoke(this);

    public delegate void CollisionEvent(CircleCollision collision);
    public event CollisionEvent OnCollision;
    public bool SendCollisionEvents = false;

    public bool HasContacts { get => contacts.Count > 0; }
    List<CircleCollision> contacts = new List<CircleCollision>();

    void AddContact(CircleCollision contact)
    {
        int index = -1;
        for(int i = 0; i < contacts.Count; i++)
        {
            CircleCollision col = contacts[i];
            if ((col.Self == contact.Self && col.Other == contact.Other))
            {
                index = i; break;
            }
        }

        if(index > -1)
            contacts[index] = contact;
        else
            contacts.Add(contact);
    }

    public void UpdateContacts()
    {
        float time = Time.time;
        for (int i = 0; i < contacts.Count; i++)
        {
            if (time - contacts[i].Created > Time.fixedDeltaTime * 2)
            {
                contacts.RemoveAt(i);
                i -= 1;
            }
        }
    }

    public CircleCollision[] GetContacts() => contacts.ToArray();

    public void ReceiveCollision(CircleBody A, CircleBody B, Vector2 n, float time)
    {
        bool isA = A == this;
        CircleCollision collision = new CircleCollision(
            isA ? A : B,
            isA ? B : A,
            n, time);

        AddContact(collision);

        if(!SendCollisionEvents) return;
        OnCollision?.Invoke(collision);
    }

    public void SetVelocity(Vector2 velocity)
    {
        LastPosition = CurrentPosition - velocity * CirclePhysics.DeltaTime;
    }

    public void AddVelocity(Vector2 velocity)
    {
        LastPosition -= velocity * CirclePhysics.DeltaTime;
    }
}
