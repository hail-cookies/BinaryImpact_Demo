using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{ 
    static Game _instance;
    public static Game Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<Game>();

            return _instance;
        }
    }

    static Camera _cam;
    public static Camera Camera
    {
        get
        {
            if (!_cam)
                _cam = Camera.main;

            return _cam;
        }
    }

    public float c_supplySpeed = 5f;
    public float c_railSpeed = 5f;
    public float c_comboDuration = 0.3f;
    public float c_laserTimer = 5f;
    public SpawnSettings spawnSettings;

    public TextMeshProUGUI scoreDisplay;
    public Rail supply;
    public List<Rail> rails = new List<Rail>();
    public List<RailExit> sinks = new List<RailExit>();

    private void Awake()
    {
        spawnSettings.Initialize();

        float lineWidth = spawnSettings.c_bubbleRadius * 2.1f;
        supply.LineRenderer.widthCurve = 
            new AnimationCurve(new Keyframe[] { 
                new Keyframe(0, 1), 
                new Keyframe(1, 1) });
        supply.LineRenderer.widthMultiplier = lineWidth;

        foreach(Rail rail in rails)
        {
            rail.LineRenderer.widthCurve =
                new AnimationCurve(new Keyframe[] {
                    new Keyframe(0, 1),
                    new Keyframe(1, 1) });
            rail.LineRenderer.widthMultiplier = lineWidth;
        }

        foreach (var sink in sinks)
            sink.Body.OnCollision += BubbleEnteredSink;
    }

    private void BubbleEnteredSink(CircleCollision collision)
    {
        ObjectPool.Destroy(collision.Other.gameObject);
    }

    private void Update()
    {
        float t = Time.time;
        float dt = Time.deltaTime;
        UpdateRails();
        Spawn(t);

        scoreDisplay.text = Score.Current + "";
    }

    void UpdateRails()
    {
        supply.Speed = c_supplySpeed;
        foreach(Rail rail in rails)
            rail.Speed = c_railSpeed;
    }

    float _lastSpawn = -Mathf.Infinity;
    void Spawn(float t)
    {
        if(t - _lastSpawn > spawnSettings.c_spawnInterval)
        {
            _lastSpawn = t;
            spawnSettings.CreateBubble(supply);
        }
    }

    public static void DestroyBubble(Bubble bubble)
    {
        ObjectPool.Destroy(bubble.gameObject);
    }

    public static void Lose(string msg)
    {
        Debug.Log("DEFEAT! " + msg);
    }
}
