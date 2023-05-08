using CustomVR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(LineRenderer), typeof(SplineComponent))]
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

    public Color color = Color.white;
    public float Speed = 1.0f;
    [Range(0f, 1f)]
    public float dampener = 0.4f;
    public List<CircleBody> AddOnStart = new List<CircleBody>();
    List<TrackedBody> TrackedBodies = new List<TrackedBody>();
    List<(Vector2, Vector2, Vector2)> gizmoData = new List<(Vector2, Vector2, Vector2)>();

    float minProg = 0f;

    public bool HasSpace
    {
        get
        {
            float bubbleSize = 2f * Game.Instance.c_bubbleRadius;
            return (TrackedBodies.Count + 1) * bubbleSize < Spline.Length;
        }
    }

    LineRenderer _lineRenderer;
    public LineRenderer LineRenderer
    { 
        get 
        { 
            if(_lineRenderer == null)
                _lineRenderer = GetComponent<LineRenderer>();

            return _lineRenderer; 
        } 
    }

    SplineComponent _spline;
    protected SplineComponent Spline
    {
        get
        {
            if(_spline == null)
                _spline = GetComponent<SplineComponent>();

            return _spline;
        }
    }

    public float ProjectPoint(Vector2 point) => ClampProgress(Spline.ProjectPoint(point));
    public Vector3 SamplePoint(float t) => Spline.SamplePoint(ClampProgress(t));
    public Vector3 SampleDirection(float t) => Spline.SampleDirection(ClampProgress(t));
    public float ClampProgress(float t) => Mathf.Clamp(t, minProg, 1f - minProg);

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

        float step = 1/20f;
        Gizmos.color = color;
        for(int i = 1; i <= 20; i++)
        {
            Gizmos.DrawLine(
                Spline.SamplePoint(step * (i - 1)),
                Spline.SamplePoint(step * i));
        }
    }

    private void Start()
    {
        minProg = Spline.spline.DistToProg(Game.Instance.c_bubbleRadius);

        foreach (var body in AddOnStart)
            Add(body);

        LineRenderer.material.color = color;
        Vector3[] positions = new Vector3[201];
        float step = 1f / 200f;
        for(int i = 0; i < positions.Length; i++)
            positions[i] = Spline.SamplePoint((float)i * step) + Vector3.forward * 0.5f;

        LineRenderer.positionCount = positions.Length;
        LineRenderer.SetPositions(positions);
    }

    public float length, stored;
    private void FixedUpdate()
    {
        gizmoData.Clear();
        TrackedBodies.Sort(CompareProgress);

        float dt = Time.fixedDeltaTime;
        for (int i = 0; i < TrackedBodies.Count; i++)
        {
            var tracked = TrackedBodies[i];
            var body = tracked.Body;

            float t = ProjectPoint(body.CurrentPosition);
            tracked.t = t;

            Vector2 dir = SampleDirection(t);
            Vector2 vel = dir * Speed;

            bool hasContact = TouchesNext(body.CurrentPosition, body.Radius, i);

            tracked.Dampener = Mathf.Clamp(
                hasContact ? 
                    tracked.Dampener * (1f - dampener) : 
                    tracked.Dampener + 0.05f, 
                0f, 
                1f);

            body.SetVelocity(vel * tracked.Dampener);
            gizmoData.Add((body.CurrentPosition, vel, Vector2.zero));
            TrackedBodies[i] = tracked;
        }
    }

    bool TouchesNext(Vector2 positon, float radius, int i)
    {
        int index = i + (Speed > 0 ? 1 : -1);
        if (index > 0 && index < TrackedBodies.Count)
            return CirclePhysics.OverlapCircle(
                TrackedBodies[index].Body, 
                positon, 
                radius * 1.1f);

        return false;
    }

    int CompareProgress(TrackedBody a, TrackedBody b) => a.t.CompareTo(b.t);

    public void Add(CircleBody body)
    {
        //Check if body is already tracked
        foreach (var tracked in TrackedBodies)
            if (tracked.Body == body)
                return;
        //Try to get ownership
        if (body.Ownership.Claim(BodyClaimed, out uint key))
        {
            float t = ProjectPoint(body.CurrentPosition);
            //Add new body
            TrackedBodies.Add(new TrackedBody(body, t, key));
            body.OnApplyConstraints += ApplyConstraint;
            body.CurrentPosition = body.transform.position = SamplePoint(t);
            body.SetVelocity(Vector2.zero);
            body.Restitution = 0;
        }
    }

    public bool Remove(CircleBody body)
    {
        for(int i = 0; i < TrackedBodies.Count; i++)
            if (TrackedBodies[i].Body == body)
            {
                body.Ownership.Release(TrackedBodies[i].key);
                TrackedBodies.RemoveAt(i);
                body.OnApplyConstraints -= ApplyConstraint;
                body.Restitution = 0.9f;
                return true;
            }

        return false;
    }

    private void ApplyConstraint(CircleBody body)
    {
        float t = ProjectPoint(body.CurrentPosition);
        t = Mathf.Clamp(t, minProg, 1 - minProg);
        Vector2 delta = (Vector2)SamplePoint(t) - body.CurrentPosition;
        body.CurrentPosition += delta;
        body.AddVelocity(-delta / CirclePhysics.DeltaTime);
    }

    bool BodyClaimed(CircleBody body)
    {
        Remove(body);
        return true;
    }
}
