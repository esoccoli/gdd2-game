using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField]
    HubControls HubControls;


    public void OnMove(InputAction.CallbackContext context)
    {
        HubControls.SetMoveDirection(context.ReadValue<Vector2>());
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        //HubControls.   dialogue/whiteboard open
    }
}
