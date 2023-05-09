using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeightedList<T>
{
    [System.Serializable]
    public class WeightedOption
    {
        [SerializeField]
        T value;
        public T Value { get { return value; } }
        [SerializeField, Min(1)]
        float weight = 1f;
        public float Weight { get { return weight; } }
        float adjustment = 1f;
        public float AdjustedWeight { get { return weight * adjustment; } }

        public WeightedOption(T value, float weight)
        {
            this.value = value;
            this.weight = weight;
            this.adjustment = 1f;
        }

        public T Use()
        {
            adjustment = 1;
            return value;
        }

        public void Pass(float adjustment)
        {
            this.adjustment *= Mathf.Max(adjustment, 1f);
        }
    }

    [Min(1)]
    public float weightAdjustment = 1.1f;
    public List<WeightedOption> options = new List<WeightedOption>();

    public void Reset()
    {
        foreach(var option in options)
            option.Use();
    }

    public T GetOption(string debug = "")
    {
        if (options.Count == 0)
            return default;

        float totalWeight = 0;
        foreach (var option in options)
            totalWeight += option.AdjustedWeight;

        if (totalWeight <= 0)
            return default;

        float accumulatedWeight = 0;
        float roll = UnityEngine.Random.Range(0, totalWeight);
        
        bool hit =  false;
        T result = default;
        foreach (var option in options)
        {
            accumulatedWeight += option.AdjustedWeight;
            if (roll <= accumulatedWeight && !hit)
            {
                hit = true;
                result = option.Use();
            }
            else
                option.Pass(weightAdjustment);
        }

        return result;
    }

    public T GetOption(Func<T,bool> condition)
    {
        if (options.Count == 0)
            return default;

        float totalWeight = 0;
        foreach (var option in options)
            totalWeight += option.AdjustedWeight;

        if (totalWeight <= 0)
            return default;

        float accumulatedWeight = 0;
        float roll = UnityEngine.Random.Range(0, totalWeight);

        bool hit = false;
        T result = default;
        foreach (var option in options)
        {
            accumulatedWeight += option.AdjustedWeight;
            if (roll <= accumulatedWeight &&
                !hit &&
                condition.Invoke(option.Value))
            {
                hit = true;
                result = option.Use();
            }
            else
                option.Pass(weightAdjustment);
        }

        if(!hit)
            foreach (var option in options)
                if(condition.Invoke(option.Value))
                    return option.Use();

        return result;
    }
}
