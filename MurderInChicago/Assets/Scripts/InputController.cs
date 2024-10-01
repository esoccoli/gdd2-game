using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField]
    GameManager gameManager;

    //[SerializeField]
    //PartyMemberScript partyScript;

    //simple framework for how inputs work
    public void OnAttack(InputAction.CallbackContext context)
    {
        //partyScript.PhysicalAttack();
    }
    public void OnRest(InputAction.CallbackContext context)
    {
        //partyScript.Rest();
    }
}
