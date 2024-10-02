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

    [SerializeField]
    SpriteRenderer turnIndicator; //The circle indicator

    // Win/Lose UI Elements
    [SerializeField]
    GameObject winScreen;

    [SerializeField]
    GameObject loseScreen;

    //simple framework for how inputs work
    //public void OnAttack(InputAction.CallbackContext context, PartyMemberScript partyMember)


    //Update the turn indicator position based on the current turn
    private void UpdateTurnIndicatorPosition()
    {

        //Check for active party member
        foreach (PartyMemberScript pMember in manager.PartyMembers)
        {
            if (pMember.IsMyTurn)
            {
                //Position the turn indicator directly below the party member
                Vector3 newPosition = pMember.transform.position + new Vector3(0, -0.8f, 0);
                turnIndicator.transform.position = newPosition;
                return; //Exit after finding the current turn member
            }
        }

        //Check for active enemy
        foreach (EnemyScript enemy in manager.Enemies)
        {
            if (enemy.IsMyTurn) //Assuming EnemyScript has IsMyTurn property
            {
                //Position the turn indicator directly below the enemy
                Vector3 newPosition = enemy.transform.position + new Vector3(0, -0.8f, 0);
                turnIndicator.transform.position = newPosition;
                return; //Exit after finding the current enemy
            }
        }
    }


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

        UpdateTurnIndicatorPosition(); // Update position after action
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

        UpdateTurnIndicatorPosition(); // Update position after action
    }

    private void CheckGameOver()
    {
        // Check if all party members are defeated
        bool allPlayersDefeated = true;
        foreach (PartyMemberScript pMember in manager.PartyMembers)
        {
            if (pMember.Health > 0)
            {
                allPlayersDefeated = false;
                break;
            }
        }

        if (allPlayersDefeated)
        {
            // Show lose screen
            loseScreen.SetActive(true);
            Time.timeScale = 0; // Stop the game
            return;
        }

        // Check if all enemies are defeated
        bool allEnemiesDefeated = true;
        foreach (EnemyScript enemy in manager.Enemies)
        {
            if (enemy.Health > 0)
            {
                allEnemiesDefeated = false;
                break;
            }
        }

        if (allEnemiesDefeated)
        {
            // Show win screen
            winScreen.SetActive(true);
            Time.timeScale = 0; // Stop the game
        }
    }

    void Start()
    {
        //Initialize the turn indicator position
        UpdateTurnIndicatorPosition();

        // Hide win and lose screens at the start
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
    }

    void Update()
    {
        // Check for win/lose conditions
        CheckGameOver();

        //checks if it is a party member's turn and then re enables the buttons if it is
        foreach (PartyMemberScript pMember in manager.PartyMembers)
        {
            //Debug.Log(pMember.IsMyTurn);
            if (pMember.IsMyTurn == true)
            {
                attackButton.gameObject.SetActive(true);
                restButton.gameObject.SetActive(true);

                UpdateTurnIndicatorPosition(); // Update position during turn
            }
        }

        // Check if it's an enemy's turn to update the indicator if necessary
        foreach (EnemyScript enemy in manager.Enemies)
        {
            if (enemy.IsMyTurn == true)
            {
                UpdateTurnIndicatorPosition(); // Update position during turn
            }
        }

        //updates each of the player hp and wp UI elements
        foreach (PartyMemberScript pMember in manager.PartyMembers)
        {
            foreach (TextMeshProUGUI hpText in pMemberHPTexts)
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
