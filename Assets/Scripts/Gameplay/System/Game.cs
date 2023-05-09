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

    public static TimedEvents TimedEvents { get; private set; } = new TimedEvents();

    [SerializeField]
    GameSettings _gameSettings;
    public static GameSettings GameSettings
    {
        get => Instance._gameSettings;
        set => Instance._gameSettings = value;
    }
    [SerializeField]
    SpawnSettings _spawnSettings;
    public static SpawnSettings SpawnSettings
    {
        get => Instance._spawnSettings; 
        set => Instance._spawnSettings = value;
    }

    public static float SpeedSupply { get; set; }
    public static float SpeedRail { get; set; }
    public static float TimerSpawn { get; set; }

    public TextMeshProUGUI scoreDisplay;
    public Rail supply;
    public List<Rail> rails = new List<Rail>();
    public List<RailExit> sinks = new List<RailExit>();

    private void Awake()
    {
        _spawnSettings.Initialize();

        float lineWidth = _spawnSettings.bubbleRadius * 2.1f;
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

        SpeedSupply = GameSettings.speedRail;
        SpeedRail = GameSettings.speedRail;
        TimerSpawn = _spawnSettings.spawnInterval;
    }

    private void BubbleEnteredSink(CircleCollision collision)
    {
        ObjectPool.Destroy(collision.Other.gameObject);
    }

    public BubbleType test, all;
    public int score = 0;
    private void Update()
    {
        all = BubbleType.Blocked | BubbleType.Red | BubbleType.Green | BubbleType.Yellow | BubbleType.Blue;
        score = (int)(all & test);

        float t = Time.time;
        float dt = Time.deltaTime;
        UpdateRails();
        Spawn(t);
        UpdateEvents(t);

        scoreDisplay.text = Score.Current + "";
    }

    void UpdateRails()
    {
        supply.Speed = SpeedSupply;
        foreach(Rail rail in rails)
            rail.Speed = SpeedRail;
    }

    float _lastSpawn = -Mathf.Infinity;
    void Spawn(float t)
    {
        if (TimerSpawn < 0)
            return;

        if(t - _lastSpawn > TimerSpawn)
        {
            _lastSpawn = t;
            _spawnSettings.CreateBubble(supply);
        }
    }

    void UpdateEvents(float t)
    {
        for(int i = 0; i < TimedEvents.Count; i++)
        {
            var evt = TimedEvents.Get(i);
            if(t - evt.created > evt.duration)
            {
                TimedEvents.Remove(evt.key);
                i--;

                evt.callBack?.Invoke();
            }
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
