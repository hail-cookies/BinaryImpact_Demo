using System.Collections.Generic;
using Unity.VisualScripting;
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

    public float c_bubbleSize = 0.5f;
    public float c_supplySpeed = 5f;
    public float c_railSpeed = 5f;
    public float c_spawnInterval = 1f;

    public Rail supply;
    public List<Rail> rails = new List<Rail>();

    public List<GameObject> availablePrefabs = new List<GameObject>();
    public List<GameObject> startingPool = new List<GameObject>();
    List<GameObject> spawnPool = new List<GameObject>();
    List<GameObject> usedPool = new List<GameObject>();

    private void Awake()
    {
        float lineWidth = c_bubbleSize * 2.1f;
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

        spawnPool.AddRange(startingPool);
    }

    private void Update()
    {
        float t = Time.time;
        float dt = Time.deltaTime;
        UpdateRails();
        Spawn(t);
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
        if(t - _lastSpawn > c_spawnInterval)
        {
            _lastSpawn = t;
            CreateBubble();
        }
    }

    void CreateBubble()
    {
        if(spawnPool.Count == 0)
        {
            spawnPool.AddRange(usedPool);
            usedPool.Clear();
        }

        int index = Random.Range(0, spawnPool.Count);
        var selected = spawnPool[index];
        spawnPool.RemoveAt(index);
        usedPool.Add(selected);

        var created = ObjectPool.Create<Bubble>(selected).Item2;
        var body = created.Body;
        created.transform.localScale = Vector3.one * 2f * c_bubbleSize;
        supply.Add(body);
        body.Radius = c_bubbleSize;
    }
}