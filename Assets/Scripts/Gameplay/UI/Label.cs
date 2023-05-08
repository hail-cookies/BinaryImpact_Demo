using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Label : MonoBehaviour
{
    static List<Label> active = new List<Label>();
    static GameObject prefab;

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
    public Transform Target;

    public static Label Create(Transform target, string message, float fontSize, Color color)
    {
        if (prefab == null)
            prefab = Resources.Load("Prefabs/Label") as GameObject;

        var created = ObjectPool.Create<Label>(prefab);

        var label = created.Item2;
        label.Target = target;
        label.UpdatePosition();

        var text = label.Text;
        text.text = message;
        text.fontSize = fontSize;
        text.color = color;
        text.rectTransform.SetParent(UI.Canvas.transform);

        return label;
    }

    public void Destroy()
    {
        ObjectPool.Destroy(gameObject);
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

    private void Update()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        Text.rectTransform.position = 
            Game.Camera.WorldToScreenPoint(Target.position);
    }
}
