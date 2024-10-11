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
    /// GameManager object/script reference
    [SerializeField]
    GameManager manager;

    [SerializeField]
    PartyMember henry;

    [SerializeField]
    PartyMember lucine;

    [SerializeField]
    Enemy enemy;


    // UI elements
    [SerializeField]
    List<TextMeshProUGUI> partyMemberHPTexts;

    [SerializeField]
    List<TextMeshProUGUI> enemyHPTexts;


    [SerializeField]
    List<TextMeshProUGUI> partyMemberWPTexts;

    [SerializeField]
    List<TextMeshProUGUI> enemyWPTexts;

    [SerializeField]
    List<Button> buttons;

    /// A circle below a character to indicate who's turn it is
    [SerializeField]
    SpriteRenderer turnIndicator;

    // Win/Lose UI Elements
    [SerializeField]
    GameObject winScreen;

    [SerializeField]
    GameObject loseScreen;


    // Update the turn indicator position based on the current turn
    private void UpdateTurnIndicatorPosition()
    {
        // Check for active party member
        foreach (PartyMember partyMember in manager.PartyMembers)
        {
            if (partyMember.IsMyTurn)
            {
                // Position the turn indicator directly below the party member
                Vector3 newPosition = partyMember.transform.position + new Vector3(0, -0.8f, 0);
                turnIndicator.transform.position = newPosition;
                return;
            }
        }

        // Check for active enemy
        foreach (Enemy enemy in manager.Enemies)
        {
            if (enemy.IsMyTurn)
            {
                // Position the turn indicator directly below the enemy
                Vector3 newPosition = enemy.transform.position + new Vector3(0, -0.8f, 0);
                turnIndicator.transform.position = newPosition;
                return;
            }
        }
    }

    /// <summary>
    /// Called when the user clicks the "Attack" button in the battle menu
    /// Takes the appropriate actions and disables all the UI buttons to prevent spam
    /// </summary>
    public void OnAttack()
    {
        ShowAndHideButtons(false);

        foreach (PartyMember partyMember in manager.PartyMembers) 
        {
            if (partyMember.IsMyTurn) { partyMember.PhysicalAttack(enemy); }
        }

        UpdateTurnIndicatorPosition();
    }

    //disables the buttons if rest is clicked so you can't spam a button and then makes the currect party member rest for that turn

    /// <summary>
    /// Called when the user clicks the "Rest" button in the battle menu
    /// Takes the appropriate actions and disables all the UI buttons to prevent spam
    /// </summary>
    public void OnRest()
    {
        ShowAndHideButtons(false);

        foreach (PartyMember partyMember in manager.PartyMembers)
        {
            if (partyMember.IsMyTurn) { partyMember.Rest(); }
        }

        UpdateTurnIndicatorPosition();
    }

    /// <summary>
    /// Checks whether all characters on one side are defeated
    /// </summary>
    private void CheckGameOver()
    {
        // Check if all party members are defeated
        bool allPlayersDefeated = true;
        foreach (PartyMember partyMember in manager.PartyMembers)
        {
            if (partyMember.Health > 0)
            {
                allPlayersDefeated = false;
                break;
            }
        }

        if (allPlayersDefeated)
        {
            // Show lose screen
            loseScreen.SetActive(true);

            // Stop the game
            Time.timeScale = 0;
            return;
        }

        // Check if all enemies are defeated
        bool allEnemiesDefeated = true;
        foreach (Enemy enemy in manager.Enemies)
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

            // Stop the game
            Time.timeScale = 0;
        }
    }

    void Start()
    {
        // Initialize the turn indicator position
        UpdateTurnIndicatorPosition();

        // Hide win and lose screens at the start
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
    }

    void Update()
    {
        // Check for win/lose conditions
        CheckGameOver();

        foreach (PartyMember partyMember in manager.PartyMembers)
        {
            // Checks if it is a party member's turn and then re enables the buttons if it is
            if (partyMember.IsMyTurn == true)
            {
                ShowAndHideButtons(true);
                UpdateTurnIndicatorPosition();
            }

            // Updates each of the player hp and wp UI elements
            foreach (TextMeshProUGUI hpText in partyMemberHPTexts)
            {
                hpText.text = "HP: " + partyMember.Health.ToString();
            }
            foreach (TextMeshProUGUI wpText in partyMemberWPTexts)
            {
                wpText.text = "WP: " + partyMember.Willpower.ToString();
            }
        }

        foreach (Enemy enemy in manager.Enemies)
        {
            // Check if it's an enemy's turn to update the indicator if necessary
            if (enemy.IsMyTurn == true)
            {
                UpdateTurnIndicatorPosition(); // Update position during turn
            }

            // Updates each of the enemy hp and wp UI elements
            foreach (TextMeshProUGUI hpText in enemyHPTexts)
            {
                hpText.text = "HP: " + enemy.Health.ToString();
            }
            foreach (TextMeshProUGUI wpText in enemyWPTexts)
            {
                wpText.text = "WP: " + enemy.Willpower.ToString();
            }
        }
    }

    // Shows or hides the buttons depending on if it is a party members turn or not
    void ShowAndHideButtons(bool isActive)
    {
        foreach (Button b in buttons)
        {
            b.gameObject.SetActive(isActive);
        }
    }
}
