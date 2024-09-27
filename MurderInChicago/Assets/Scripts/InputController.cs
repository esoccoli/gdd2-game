using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField]
    GameManager gameManager;

    //simple framework for how inputs work
    public void OnAttack(InputAction.CallbackContext context)
    {
        //should call function that attacks based on their turn
    }
}
