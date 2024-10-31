using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
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
    PartyMember clyde;

    [SerializeField]
    Enemy enemy1;

    [SerializeField] 
    Enemy enemy2;

    [SerializeField]
    List<Character> characterList;

    // UI elements
    [SerializeField]
    List<TextMeshProUGUI> partyMemberHPTexts;

    [SerializeField]
    List<TextMeshProUGUI> enemyHPTexts;

    [SerializeField]
    List<TextMeshProUGUI> partyMemberWPTexts;

    [SerializeField]
    TextMeshPro damageText;

    [SerializeField]
    List<Button> buttons;

    //spell UI
    [SerializeField]
    GameObject spellBox;

    [SerializeField]
    List<TextMeshProUGUI> spellButtons;

    int spellListIndex;

    /// A circle below a character to indicate who's turn it is
    [SerializeField]
    SpriteRenderer turnIndicator;

    // Win/Lose UI Elements
    [SerializeField]
    GameObject winScreen;

    [SerializeField]
    GameObject loseScreen;

    [SerializeField]
    GameObject quitButton;

    [SerializeField]
    GameObject restartButton;

    //targeting UI
    [SerializeField]
    Collider2D cursor;

    [SerializeField]
    GameObject arrowIndicator;

    /// <summary>
    /// Called when the user clicks the "Attack" button in the battle menu
    /// Takes the appropriate actions and disables all the UI buttons to prevent spam
    /// </summary>
    public void OnAttack()
    {
        ShowAndHideButtons(false);

        foreach (PartyMember partyMember in manager.PartyMembers) 
        {
            if (partyMember.IsMyTurn) 
            {
                partyMember.IsTargeting = true;
            }
        }
    }


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
    }

    public void OnSpell()
    {
        spellBox.SetActive(true);
    }

    public void OnSpell(int index)
    {
        ShowAndHideButtons(false);
        foreach (PartyMember partyMember in manager.PartyMembers)
        {
            if (partyMember.IsMyTurn)
            {
                partyMember.IsUsingSpell = true;
                partyMember.IsTargeting = true;
                spellListIndex = index;
            }
        }
    }

    /// <summary>
    /// quits game
    /// </summary>
    public void OnQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// restarts the scene
    /// </summary>
    public void OnRestart()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// This takes the damage text and updates it and moves to where the player getting hit is
    /// </summary>
    /// <param name="position"></param>
    /// <param name="damageAmount"></param>
    /// <param name="offset"></param>
    public void ShowDamagePopup(Vector3 position, int damageAmount, Vector3 offset)
    {
        damageText.text = damageAmount.ToString();
        damageText.color = Color.red;
        damageText.transform.position = position + offset; // Adjusts above the target

        StartCoroutine(FadeOutDamagePopup());
    }

    /// <summary>
    /// This makes the text that shows the damage fade away after a second
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutDamagePopup()
    {
        float duration = 1f;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
            damageText.color = new Color(1, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hide the text after fading out
        damageText.text = "";
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
            quitButton.SetActive(true);
            restartButton.SetActive(true);
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
            quitButton.SetActive(true);
            restartButton.SetActive(true);
        }
    }

    void Start()
    {
        // Initialize the turn indicator position
        UpdateTurnIndicatorPosition();

        // Hide win and lose screens at the start
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        arrowIndicator.SetActive(false);
    }

    void Update()
    {
        // Check for win/lose conditions
        CheckGameOver();

        UpdateTurnIndicatorPosition();

        for (int i = 0; i < manager.PartyMembers.Count; i++)
        {
            // Checks if it is a party member's turn and then re enables the buttons if it is
            if (manager.PartyMembers[i].IsMyTurn)
            {
                ShowAndHideButtons(true);
                //UpdateTurnIndicatorPosition();

                
                //updates text on the spell buttons
                for (int j = 0; j < spellButtons.Count; j++)
                {
                    spellButtons[j].text = manager.PartyMembers[i].spellList[j];
                }


                if (manager.PartyMembers[i].IsTargeting)
                {
                    ShowAndHideButtons(false);


                    if (manager.PartyMembers[i].IsUsingSpell)
                    {
                        //todo: add the new turn indicator 
                        //arrowIndicator.transform.position = spellText.transform.position + new Vector3(0, +0.8f, 0);
                        //arrowIndicator.SetActive(true);

                        string[] spellInfo = manager.PartyMembers[i].GetSpellInfo(spellButtons[spellListIndex].text);

                        TargetEnemies(manager.PartyMembers[i], spellListIndex, spellInfo[0], spellInfo[1]);
                        
                    }
                    else
                    {
                        TargetEnemies(manager.PartyMembers[i]);
                    }
                }
                else
                {
                    arrowIndicator.SetActive(false);
                }
            }

            // Updates each of the player hp and wp UI elements

            partyMemberHPTexts[i].text = manager.PartyMembers[i].Health.ToString();
            partyMemberWPTexts[i].text = manager.PartyMembers[i].Willpower.ToString();
        }

        for (int i = 0; i < manager.Enemies.Count; i++)
        {
            // Check if it's an enemy's turn to update the indicator if necessary
            if (manager.Enemies[i].IsMyTurn)
            {
                UpdateTurnIndicatorPosition(); // Update position during turn
            }

            // Updates each of the enemy hp and wp UI elements
            enemyHPTexts[i].text = "HP: " + manager.Enemies[i].Health.ToString();
        }
    }

    /// <summary>
    /// Update the turn indicator position based on the current turn
    /// </summary>
    private void UpdateTurnIndicatorPosition()
    {
        foreach (Character character in characterList)
        {
            if (character.IsMyTurn)
            {
                // Position the turn indicator directly below the party member
                Vector3 newPosition = character.transform.position + new Vector3(0, -0.8f, 0);
                turnIndicator.transform.position = newPosition;
                return;
            }
        }
    }


    /// <summary>
    /// Shows or hides the buttons depending on if it is a party members turn or not
    /// </summary>
    /// <param name="isActive"></param>
    void ShowAndHideButtons(bool isActive)
    {
        foreach (Button b in buttons)
        {
            b.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// lets a chosen party member target and ennemy and attack them
    /// </summary>
    /// <param name="member"></param>
    void TargetEnemies(PartyMember member)
    {
        foreach (Enemy enemy in manager.Enemies)
        {
            if (cursor.bounds.Intersects(enemy.Collider.bounds))
            {
                arrowIndicator.transform.position = enemy.transform.position + new Vector3(0, +0.8f, 0);
                arrowIndicator.SetActive(true);

                if (Input.GetMouseButton(0))
                {
                    member.PhysicalAttack(enemy);
                }
            }
        }
    }

    /// <summary>
    /// targets an enemy with a specific spell or if spell is multi targeted targets all
    /// </summary>
    /// <param name="member"></param>
    /// <param name="isSpell"></param>
    void TargetEnemies(PartyMember member, int index, string type, string enemiesTargeted)
    {
        List<Character> targetedCharacters = new List<Character>();

        //if (member.GetSpellTargeting(spellButtons[index].text) == "Multiple")
        if (enemiesTargeted == "Multiple")
        {
            if (type == "Heal" || type == "Buff")
            {
                foreach (PartyMember pMember in manager.PartyMembers)
                {
                    targetedCharacters.Add(pMember);
                }
            }
            else
            {
                foreach (Enemy enemy in manager.Enemies)
                {
                    targetedCharacters.Add(enemy);
                }
            }
            member.MagicAttack(targetedCharacters, spellButtons[index].text);
        }
        else
        {
            foreach (Enemy enemy in manager.Enemies)
            {
                if (cursor.bounds.Intersects(enemy.Collider.bounds))
                {
                    arrowIndicator.transform.position = enemy.transform.position + new Vector3(0, +0.8f, 0);
                    arrowIndicator.SetActive(true);

                    if (Input.GetMouseButton(0))
                    {
                        targetedCharacters.Add(enemy);
                        member.MagicAttack(targetedCharacters, spellButtons[index].text);
                    }
                }
            }
        }
    }
}
