using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    [SerializeField]
    GameManager manager;

    [SerializeField]
    PartyMemberScript henry;

    [SerializeField]
    PartyMemberScript lucine;

    [SerializeField]
    EnemyScript enemy;


    //UI elements
    [SerializeField]
    List<TextMeshProUGUI> pMemberHPTexts;

    [SerializeField]
    List<TextMeshProUGUI> enemyHPTexts;


    [SerializeField]
    List<TextMeshProUGUI> pMemberWPTexts;

    [SerializeField]
    List<TextMeshProUGUI> enemyWPTexts;

    [SerializeField]
    Button attackButton;

    [SerializeField]
    Button restButton;

    //simple framework for how inputs work
    //public void OnAttack(InputAction.CallbackContext context, PartyMemberScript partyMember)


    //disables the buttons if Attack is clicked so you can't spam a button and then makes the currect party member attack the enemy for that turn
    public void OnAttack()
    {
        attackButton.gameObject.SetActive(false);
        restButton.gameObject.SetActive(false);

        foreach (PartyMemberScript pMember in manager.PartyMembers) 
        {
            if (pMember.IsMyTurn)
            {
                pMember.PhysicalAttack(enemy);
            }
        }
    }

    //disables the buttons if rest is clicked so you can't spam a button and then makes the currect party member rest for that turn
    public void OnRest()
    {
        attackButton.gameObject.SetActive(false);
        restButton.gameObject.SetActive(false);

        foreach (PartyMemberScript pMember in manager.PartyMembers)
        {
            if (pMember.IsMyTurn)
            {
                pMember.Rest();
            }
        }
    }

    void Update()
    {
        //checks if it is a party member's turn and then re enables the buttons if it is
        foreach (PartyMemberScript pMember in manager.PartyMembers)
        {
            Debug.Log(pMember.IsMyTurn);
            if (pMember.IsMyTurn == true)
            {
                attackButton.gameObject.SetActive(true);
                restButton.gameObject.SetActive(true);
            }
        }

        //updates each of the player hp and wp UI elements
        foreach (PartyMemberScript pMember in manager.PartyMembers)
        {
            foreach(TextMeshProUGUI hpText in pMemberHPTexts)
            {
                hpText.text = "HP: " + pMember.Health.ToString();
            }
            foreach (TextMeshProUGUI wpText in pMemberWPTexts)
            {
                wpText.text = "WP: " + pMember.Current_Willpower.ToString();
            }

        }

        //updates each of the enemy hp and wp UI elements
        foreach (EnemyScript enemy in manager.Enemies) 
        {
            foreach (TextMeshProUGUI hpText in enemyHPTexts)
            {
                hpText.text = "HP: " + enemy.Health.ToString();
            }
            foreach (TextMeshProUGUI wpText in enemyWPTexts)
            {
                wpText.text = "WP: " + enemy.Current_Willpower.ToString();
            }
        }
    }
}
