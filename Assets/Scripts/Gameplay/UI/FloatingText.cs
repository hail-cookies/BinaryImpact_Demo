using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FloatingText : MonoBehaviour
{
    static List<FloatingText> active = new List<FloatingText>();
    static GameObject prefab;

    public Vector2 velocity = Vector2.zero;
    public Vector2 acceleration = Vector2.zero;
    public float lifeTime;
    public float fadeDelay;
    public float fadeTime;
    public float created;

    TextMeshProUGUI _text;
    public TextMeshProUGUI Text
    {
        get
        {
            if(_text == null)
                _text = GetComponent<TextMeshProUGUI>();

            return _text;
        }
    }

    public static FloatingText Create(
        Vector2 position, 
        Vector2 velocity, 
        Vector2 acceleration, 
        float lifeTime,
        float fadeDelay,
        float fadeTime, 
        string message,
        float fontSize,
        Color color)
    {
        if(prefab == null)
            prefab = Resources.Load("Prefabs/FloatingText") as GameObject;

        var created = ObjectPool.Create<FloatingText>(prefab);
        
        var floating = created.Item2;
        floating.velocity = velocity;
        floating.acceleration = acceleration;
        floating.lifeTime = lifeTime;
        floating.fadeDelay = Mathf.Max(0, fadeDelay);
        floating.fadeTime = fadeTime;
        floating.created = Time.time;

        var text = floating.Text;
        position = Game.Camera.WorldToScreenPoint(position);
        text.rectTransform.SetParent(UI.Canvas.transform);
        text.rectTransform.position = position;
        text.color = color;
        text.text = message;
        text.fontSize = fontSize;

        active.Add(floating);
        return floating;
    }

    private void Update()
    {
        float time = Time.time;
        float delta = time - created;

        CheckStatus(delta);
        Fade(delta);
        UpdatePosition();
    }

    private void OnEnable()
    {
        if(!active.Contains(this))
            active.Add(this);
    }

    private void OnDisable()
    {
        active.Remove(this);
    }

    void CheckStatus(float delta)
    {
        if (delta > lifeTime)
            ObjectPool.Destroy(gameObject);
    }

    void Fade(float delta)
    {
        if (delta > fadeDelay)
        {
            Color c = Text.color;
            c.a = (delta - fadeDelay) / fadeTime;
            Text.color = c;
        }
    }

    void UpdatePosition()
    {
        float dt = Time.deltaTime;
        velocity += acceleration * dt;
        Text.rectTransform.position += (Vector3)(velocity * dt);
    }
}
