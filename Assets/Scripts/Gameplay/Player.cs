using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public InputActionReference a_lMouse, a_mousePosition;
    public GameObject prefab;

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
        var body = CurrentFocus;
        ToggleFocus(CurrentFocus, false);

        if (body != null)
        {
            if (targetRail != null && targetRail.HasSpace)
            {
                debug2.position = (Vector3)body.CurrentPosition - Vector3.forward;
                body.CurrentPosition = targetRail.SamplePoint(0);
                targetRail.Add(body);
            }
            else
                Game.Lose("Cannot drop off bubble!");
        }
    }

    private void LMouseDown(InputAction.CallbackContext obj)
    {
        lMouse = true;
        Getfocus();
    }

    Rail targetRail;
    void DragFocus()
    {
        if(CurrentFocus== null) return;

        Vector2 mousePos = Game.Camera.ScreenToWorldPoint(MousePosition);
        CurrentFocus.SetVelocity((mousePos - CurrentFocus.CurrentPosition) / Time.fixedDeltaTime);

        targetRail = GetClosestRail(mousePos);
    }

    private void FixedUpdate()
    {
        DragFocus();
    }

    public CircleBody CurrentFocus;
    void Getfocus()
    {
        Vector2 mousePos = Game.Camera.ScreenToWorldPoint(MousePosition);
        CirclePhysics.CheckPoint(mousePos, out var newFocus);

        if (Game.Instance.supply.Contains(newFocus))
            return;
        if (newFocus && !newFocus.Ownership.Claimed)
            return;

        if(newFocus != CurrentFocus)
        {
            ToggleFocus(CurrentFocus, false);
            ToggleFocus(newFocus, true);
        }
    }

    uint ownershipKey = 0;
    public bool focusIsTrigger = true;   
    void ToggleFocus(CircleBody body, bool state)
    {
        if (!body)
            return;

        bool legal = state ? body.Ownership.Claim(FocusClaimed, out ownershipKey) : true;

        if (legal)
        {
            if (!state) body.Ownership.Release(ownershipKey);

            CurrentFocus = state ? body : null;
            body.Renderer.material.SetFloat("_HighlightIntensity", state ? 5f : 0f);
            body.disableCollision = state;
            body.transform.position =
                new Vector3(
                    body.CurrentPosition.x,
                    body.CurrentPosition.y,
                    state ? -1 : 0);

            if (Bubble.TryGetBubble(body.gameObject, out var bubble))
                bubble.Suspend(state);
        }
    }

    bool FocusClaimed(CircleBody body)
    {
        ToggleFocus(body, false);
        return true;
    }

    public Transform debug1, debug2;
    public List<Rail> rails = new List<Rail>();
    Rail GetClosestRail(Vector3 mousePos)
    {
        if (rails.Count == 0) 
            return null;
        mousePos.z = 0;

        Vector3 pos = Vector3.zero;
        Rail closestRail = null;
        float closest = Mathf.Infinity;
        foreach (var rail in rails)
        {
            if (!rail.HasSpace)
                continue;

            Vector3 p = rail.SamplePoint(rail.ProjectPoint(mousePos));

            float sqrDist = (p - mousePos).sqrMagnitude;
            if (sqrDist < closest)
            {
                closest = sqrDist;
                pos = p;
                closestRail = rail;
            }
        }

        debug1.position = pos - Vector3.forward;
        debug2.position = mousePos - Vector3.forward;
        return closestRail;
    }
}
