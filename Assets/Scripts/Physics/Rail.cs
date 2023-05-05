using CustomVR;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
{
    public struct TrackedBody
    {
        public CircleBody Body;
        public float Dampener;
        public float t;
        public readonly uint key;

        public TrackedBody(CircleBody body, float t, uint key)
        {
            this.Body = body;
            Dampener = 1;
            this.t = t;
            this.key = key;
        }
    }

    public SplineComponent Spline;
    public float Speed = 1.0f;
    [Range(0f, 1f)]
    public float trackingStrength = 0.3f;
    public List<CircleBody> AddOnStart = new List<CircleBody>();
    List<TrackedBody> TrackedBodies = new List<TrackedBody>();

    List<(Vector2, Vector2, Vector2)> gizmoData = new List<(Vector2, Vector2, Vector2)>();
    private void OnDrawGizmos()
    {
        foreach (var data in gizmoData)
        {
            Vector3 pos1 = (Vector3)data.Item1 - Vector3.forward;
            Vector3 pos2 = pos1 - Vector3.forward * 0.1f;
            Vector3 nrm = (Vector3)data.Item2;
            Vector3 dir = (Vector3)data.Item3;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos1, pos1 + dir * 0.2f);
            Gizmos.DrawSphere(pos1, 0.01f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(pos2, 0.01f);
            Gizmos.DrawLine(pos2, pos2 + nrm * 0.2f);
        }
    }

    private void Start()
    {
        foreach(var body in AddOnStart)
            Add(body);
    }

    public bool useDampener = false;
    private void FixedUpdate()
    {
        gizmoData.Clear();

        for (int i = 0; i < TrackedBodies.Count; i++)
        {
            var tracked = TrackedBodies[i];
            var body = tracked.Body;

            float t = Spline.ProjectPoint(body.CurrentPosition);
            t = Mathf.Clamp(t, 0.001f, 0.999f);

            Vector2 dir = Spline.SampleDirection(t);
            Vector2 vel = dir * Speed;

            bool hasContact = false;
            if (body.HasContacts)
            {
                var contacts = body.GetContacts();
                foreach (var contact in contacts)
                {
                    Vector2 n = contact.Normal * (contact.A == body ? -1 : 1);
                    if (Vector2.Dot(vel, n) < 0)
                    {
                        hasContact = true;
                        break;
                    }
                }
            }

            tracked.Dampener = Mathf.Clamp(
                hasContact ? 
                    tracked.Dampener * 0.8f : 
                    tracked.Dampener + 0.05f, 
                0.2f, 
                1f);

            body.SetVelocity(vel * (useDampener ? tracked.Dampener : 1f));
            gizmoData.Add((body.CurrentPosition, vel, Vector2.zero));
            TrackedBodies[i] = tracked;
        }
    }

    public void Add(CircleBody body)
    {
        //Check if body is already tracked
        foreach (var tracked in TrackedBodies)
            if (tracked.Body == body)
                return;
        //Try to get ownership
        if (body.Ownership.Claim(BodyClaimed, out uint key))
        {
            float t = Spline.ProjectPoint(body.CurrentPosition);
            //Add new body
            TrackedBodies.Add(new TrackedBody(body, t, key));
            body.OnApplyConstraints += ApplyConstraint;
            body.CurrentPosition = Spline.SamplePoint(t);
            body.SetVelocity(Vector2.zero);
        }
    }

    public void Remove(CircleBody body)
    {
        for(int i = 0; i < TrackedBodies.Count; i++)
            if (TrackedBodies[i].Body == body)
            {
                body.Ownership.Release(TrackedBodies[i].key);
                TrackedBodies.RemoveAt(i);
                body.OnApplyConstraints -= ApplyConstraint;
                break;
            }
    }

    private void ApplyConstraint(CircleBody body)
    {
        float t = Spline.ProjectPoint(body.CurrentPosition);
        t = Mathf.Clamp(t, 0.005f, 0.999f);
        body.CurrentPosition = Spline.SamplePoint(t);
    }

    bool BodyClaimed(CircleBody body)
    {
        Remove(body);
        return true;
    }
}
