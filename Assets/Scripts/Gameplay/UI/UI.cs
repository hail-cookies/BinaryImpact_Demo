using Unity.VisualScripting;
using UnityEngine;

public class UI : MonoBehaviour
{
    static Canvas _canvas;
    public static Canvas Canvas
    {
        get
        {
            if(_canvas == null)
                _canvas = 
                    GameObject.Instantiate(Resources.Load("Prefabs/UI")).
                    GetComponent<Canvas>();

            return _canvas;
        }
    }

    static UI _instance;
    public static UI Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<UI>();

                if(_instance == null)
                {
                    var go = new GameObject("UI");
                    _instance = go.AddComponent<UI>();
                }
            }

            return _instance;
        }
    }

    public float c_scoreTextDuration = 1f;
    public float c_scoreTextSize = 24;
    public Vector2 c_scoreTextVelocity = Vector2.up;
    public Color c_scoreTextColor = Color.red;
}
