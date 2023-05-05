using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public InputActionReference a_lMouse, a_mousePosition;
    public GameObject prefab;
    public float radius = 0.1f;
    public float spawnRate = 0.1f;
    public Vector2 spawnVelocity = Vector2.right;
    public bool spawn = true;

    static Camera _cam;
    public static Camera Camera
    {
        get
        {
            if(!_cam)
                _cam = Camera.main;

            return _cam;
        }
    }

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
        ToggleFocus(CurrentFocus, false);
    }

    private void LMouseDown(InputAction.CallbackContext obj)
    {
        lMouse = true;
        Getfocus();
    }

    float lastSpawn = 0;
    int spawnCount = 0;
    private void Spawn()
    {
        if (!spawn) return;

        float time = Time.time;
        if (time - lastSpawn < spawnRate)
            return;
        lastSpawn = time;

        var obj = ObjectPool.Create<CircleBody>(prefab, (Vector2)transform.position, Quaternion.identity, null);
        obj.Item2.Radius = radius;
        obj.Item2.SetVelocity(spawnVelocity);
        obj.Item1.GetComponent<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV(0, 1, 0, 1, 0.3f, 1);
        obj.Item1.name = "Phys " + spawnCount++;
    }

    void DragFocus()
    {
        if(CurrentFocus== null) return;

        Vector2 mousePos = Camera.ScreenToWorldPoint(MousePosition);
        CurrentFocus.SetVelocity((mousePos - CurrentFocus.CurrentPosition) / Time.fixedDeltaTime);
    }

    private void FixedUpdate()
    {
        Spawn();
        DragFocus();
    }

    public CircleBody CurrentFocus;
    public Color focusBaseColor;
    void Getfocus()
    {
        Vector2 mousePos = Camera.ScreenToWorldPoint(MousePosition);
        CirclePhysics.CheckPoint(mousePos, out var newFocus);

        if(newFocus != CurrentFocus)
        {
            ToggleFocus(CurrentFocus, false);
            ToggleFocus(newFocus, true);
        }
    }

    public bool focusIsTrigger = true;   
    void ToggleFocus(CircleBody body, bool state)
    {
        if (!body)
            return;

        CurrentFocus = state ? body : null;
        focusBaseColor = state ? body.Renderer.material.color : focusBaseColor;
        body.Renderer.material.color = state ? Color.white : focusBaseColor;
        body.isTrigger = state && focusIsTrigger;
        body.transform.position = 
            new Vector3(
                body.CurrentPosition.x,
                body.CurrentPosition.y,
                state ? -1 : 0);

        body.SendCollisionEvents = state;
    }
}
