using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class Player : MonoBehaviour
{
    public InputActionReference a_lMouse, a_mousePosition;
    public GameObject prefab;
    public float radius = 0.1f;
    public float spawnRate = 0.1f;
    public Vector2 spawnVelocity = Vector2.right;
    public bool spawn = true;

    // Start is called before the first frame update
    void Start()
    {
        a_lMouse.action.Enable();
        a_lMouse.action.performed += LMouseDown;
        a_lMouse.action.canceled += LMouseUp;

        a_mousePosition.action.Enable();
        a_mousePosition.action.performed += GetMousePosition;
    }

    public Vector2 MousePosition { get; private set; } = Vector2.zero;
    private void GetMousePosition(InputAction.CallbackContext obj)
    {
        MousePosition = obj.ReadValue<Vector2>();
    }

    bool lMouse = false;
    private void LMouseUp(InputAction.CallbackContext obj)
    {
        lMouse = false;
    }

    private void LMouseDown(InputAction.CallbackContext obj)
    {
        lMouse = true;
    }

    float lastSpawn = 0;
    private void Spawn()
    {
        float time = Time.time;
        if (time - lastSpawn < spawnRate)
            return;
        lastSpawn = time;

        var obj = ObjectPool.Create<CircleBody>(prefab, (Vector2)transform.position, Quaternion.identity, null);
        obj.Item2.Radius = radius;
        obj.Item2.Velocity = spawnVelocity;
        obj.Item1.GetComponent<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV(0, 1, 0, 1, 0.3f, 1);
    }

    public int CellCount = 0;
    private void FixedUpdate()
    {
        if(spawn)
            Spawn();

        CellCount =
            lMouse ? 
            CirclePhysics.Grid.GetCell(Camera.main.ScreenToWorldPoint(MousePosition)).Index + 1 : 
            0;
    }
}
