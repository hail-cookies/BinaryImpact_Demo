using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum BubbleType
{
    Blocked = 1,
    Red = 2,
    Blue = 4,
    Yellow = 8,
    Green = 16
}

public enum BubbleSelect
{
    Blocked = 0, 
    Red = 1, 
    Blue = 2, 
    Yellow = 3, 
    Green = 4
}

[RequireComponent(typeof(CircleBody))]
public class Bubble : MonoBehaviour
{
    static Dictionary<GameObject, Bubble> cache = new Dictionary<GameObject, Bubble>();
    public static bool TryGetBubble(GameObject obj, out Bubble bubble) =>
        cache.TryGetValue(obj, out bubble);
    public static bool IsBubble(GameObject obj) => cache.ContainsKey(obj);

    public BubbleType bubbleType = BubbleType.Green;

    public Label Label { get; protected set; }

    CircleBody _body;
    public CircleBody Body
    {
        get
        {
            if(_body == null)
                _body = GetComponent<CircleBody>();

            return _body;
        }
    }

    public virtual void EnterPlay()
    {
        
    }

    public virtual void LeavePlay()
    {

    }

    public virtual void AbilitySuspend(bool state)
    {

    }

    private void OnEnable()
    {
        if(!cache.ContainsKey(gameObject))
            cache.Add(gameObject, this);
    }

    private void OnDisable()
    {
        cache.Remove(gameObject);
        if (Label)
        {
            Label.Destroy();
            Label = null;
        }
    }
}
