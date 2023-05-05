using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class IntegralAnimationCurve : MonoBehaviour
{
    public AnimationCurve editCurve;
    [ShowInInspector, ReadOnly]
    public float DistancePerSecond { get; private set; }
    [Min(1)]
    public int resolution = 50;

    public float Evaluate(float travelTime, float elapsed, float dt)
    {
        float p = (travelTime - elapsed) > dt ? 1 : (travelTime - elapsed) / dt;
        return Evaluate(elapsed / travelTime) * p;
    }

    public float Evaluate(float t)
    {
        return editCurve.Evaluate(t);
    }

    [Button("Apply")]
    void ApplyButton()
    {
        ApplyChanges();
    }

    public float ApplyChanges()
    {
        DistancePerSecond = IntegrateAnimationCurve(editCurve, resolution);
        return DistancePerSecond;
    }

    public static float IntegrateAnimationCurve(AnimationCurve curve, int resolution = 80)
    {
        float result = 0;
        int keyCount = curve.length;
        if (keyCount <= 0 || resolution <= 0)
            return result;

        Keyframe[] keys = curve.keys;
        int currentKey = 1;
        float lastValue = curve.Evaluate(0);
        for(int i = 1; i <= resolution; i++)
        {
            float _t = (i - 1) / (float)resolution;
            float t = i / (float)resolution;

            while(keys[currentKey].time <= t)
            {
                result += lastValue * (keys[currentKey].time - _t);
                lastValue = keys[currentKey].value;
                _t = keys[currentKey].time;

                if (currentKey == keyCount - 1)
                    break;

                currentKey++;
            }

            if (keys[currentKey].time == t)
                continue;
            else
            {
                result += lastValue * (t - _t);
                lastValue = curve.Evaluate(t);
            }
        }

        return result;
    }

    public float Speed(float distance, float travelTime)
    {
        return distance / (DistancePerSecond * travelTime);
    }

    public float Distance(float speed, float travelTime)
    {
        return speed * DistancePerSecond * travelTime;
    }

    public float TravelTime(float distance, float speed)
    {
        return distance / (DistancePerSecond * speed);
    }
}
