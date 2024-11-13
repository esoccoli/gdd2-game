using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Spells;

public class UIScript : MonoBehaviour
{
    /// GameManager references
    [SerializeField]
    GameManager manager;

    [SerializeField]
    List<Character> characterList;

    List<PartyMember> partyMembers;

    List<Enemy> enemies;

    // UI elements
    [SerializeField]
    List<TextMeshProUGUI> partyMemberHPTexts;

    [SerializeField]
    List<TextMeshProUGUI> enemyHPTexts;

    [SerializeField]
    List<GameObject> enemyHPUI;

    [SerializeField]
    List<TextMeshProUGUI> partyMemberWPTexts;

    [SerializeField]
    TextMeshPro damageText;

    [SerializeField]
    TextMeshPro healText;

    [SerializeField]
    List<Button> buttons;

    //spell UI
    [SerializeField]
    GameObject spellBox;

    [SerializeField]
    GameObject spellDescriptionBox;

    [SerializeField]
    TextMeshProUGUI spellDescriptionText;

    [SerializeField]
    List<TextMeshProUGUI> spellButtonTexts;

    [SerializeField]
    List<Button> spellButtons;

    int spellListIndex;

    bool isLookingAtSpells = false;

    int currentSpell;

    // An indicator below a character to show who's turn it is
    [SerializeField]
    SpriteRenderer turnIndicatorFront;

    [SerializeField]
    SpriteRenderer turnIndicatorBack;

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

    [SerializeField]
    TextMeshProUGUI targetPromptText;


    #region Player UI Functions
    /// <summary>
    /// Called when the user clicks the "Attack" button in the battle menu
    /// Takes the appropriate actions and disables all the UI buttons to prevent spam
    /// </summary>
    public void OnAttack()
    {
        ShowAndHideButtons(false);

        foreach (PartyMember partyMember in partyMembers) 
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

        foreach (PartyMember partyMember in partyMembers)
        {
            if (partyMember.IsMyTurn) { partyMember.Rest(); }
        }
    }

    public void OnSpell()
    {
        spellBox.SetActive(!spellBox.activeSelf);
        isLookingAtSpells = !isLookingAtSpells;
    }

    public void OnSpell(int index)
    {
        ShowAndHideButtons(false);
        isLookingAtSpells = false;
        foreach (PartyMember partyMember in partyMembers)
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
    public void OnQuit() { Application.Quit(); }

    /// <summary>
    /// restarts the scene
    /// </summary>
    public void OnRestart() { SceneManager.LoadScene(0); }

    /// <summary>
    /// moves the arrow next to the spell button the player is hovering over
    /// </summary>
    public void OnMoveArrow(int index)
    {
        arrowIndicator.SetActive(true);
        arrowIndicator.transform.position = spellButtons[index].transform.position + new Vector3(-1.5f, 0, 0);
        spellDescriptionBox.SetActive(true);
        currentSpell = index;
    }

    #endregion

    void Start()
    {
        //get lists of characetrs from game manager
        partyMembers = manager.PartyMembers;
        enemies = manager.Enemies;

        // Initialize the turn indicator position
        UpdateTurnIndicatorPosition();

        // Hide win/lose and spell UI at start
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        arrowIndicator.SetActive(false);
        spellBox.SetActive(false);
        spellDescriptionBox.SetActive(false);
        targetPromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Check for win/lose conditions
        CheckGameOver();

        if (!isLookingAtSpells)
        {
            arrowIndicator.SetActive(false);
            spellDescriptionBox.SetActive(false);
        }

        for (int i = 0; i < partyMembers.Count; i++)
        {
            // Updates each of the player hp and wp UI elements
            partyMemberHPTexts[i].text = partyMembers[i].Health.ToString();
            partyMemberWPTexts[i].text = partyMembers[i].Willpower.ToString();

            // Checks if it is a party member's turn and then re enables the buttons if it is
            if (partyMembers[i].IsMyTurn)
            {
                ShowAndHideButtons(true);

                List<string> spellNames = new List<string>();


                Spell cSpell = partyMembers[i].GlobalSpellList.GetSpell(partyMembers[i].spellList[currentSpell]);

                spellDescriptionText.text = cSpell.description;


                //updates text on the spell buttons
                for (int j = 0; j < spellButtonTexts.Count; j++)
                {
                    spellNames.Add(partyMembers[i].spellList[j]);

                    Spell spell = partyMembers[i].GlobalSpellList.GetSpell(partyMembers[i].spellList[j]);

                    spellButtonTexts[j].text = $"{spell.name} WP: {spell.willpowerCost}";

                    if (partyMembers[i].Willpower < spell.willpowerCost)
                    {
                        spellButtons[j].interactable = false;
                        spellButtonTexts[j].color = Color.gray;
                    }
                    else
                    {
                        spellButtons[j].interactable = true;
                        spellButtonTexts[j].color = Color.white;
                    }
                }

                //while player is targeting call target function based on what the party member is doing
                if (partyMembers[i].IsTargeting)
                {
                    ShowAndHideButtons(false);
                    targetPromptText.gameObject.SetActive(true);

                    //spells
                    if (partyMembers[i].IsUsingSpell)
                    {
                        //todo: add the new turn indicator 
                        //arrowIndicator.transform.position = spellText.transform.position + new Vector3(0, +0.8f, 0);
                        //arrowIndicator.SetActive(true);

                        string[] spellInfo = partyMembers[i].GetSpellInfo(spellNames[spellListIndex]);


                        TargetEnemies(partyMembers[i], spellNames[spellListIndex], spellListIndex, spellInfo[0], spellInfo[1]);
                        
                    }
                    //attacking
                    else
                    {
                        TargetEnemies(partyMembers[i]);
                    }
                }
                else
                {
                    targetPromptText.gameObject.SetActive(false);
                }
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            if (!enemies[i].IsAlive) { enemyHPUI[i].SetActive(false); }

            // Updates each of the enemy hp and wp UI elements
            enemyHPTexts[i].text = $"HP: {enemies[i].Health}";
        }
    }

    #region Helper Functions

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
                Vector3 newPositionFront = character.transform.position + new Vector3(0, -0.8f, -5);
                Vector3 newPositionBack = character.transform.position + new Vector3(0, -0.8f, 5);
                turnIndicatorFront.transform.position = newPositionFront;
                turnIndicatorBack.transform.position = newPositionBack;
                return;
            }
        }
    }

    /// <summary>
    /// Checks whether all characters on one side are defeated
    /// </summary>
    private void CheckGameOver()
    {
        // Check if all party members are defeated
        bool allPlayersDefeated = true;
        foreach (PartyMember partyMember in partyMembers)
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
        foreach (Enemy enemy in enemies)
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

    #region Popup Functions

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

    public void ShowHealPopup(Vector3 position, int healAmount, Vector3 offset)
    {
        healText.text = healAmount.ToString();
        healText.color = Color.green;
        healText.transform.position = position + offset; // Adjusts above the target

        StartCoroutine(FadeOutHealPopup());
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

    private IEnumerator FadeOutHealPopup()
    {
        float duration = 1f;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
            healText.color = new Color(0, 1, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hide the text after fading out
        healText.text = "";
    }


    #endregion


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

    //TODO: make second show and hide buttons function for Spell UI

    #region Player Targeting Functions

    /// <summary>
    /// lets a chosen party member target and ennemy and attack them
    /// </summary>
    /// <param name="member"></param>
    void TargetEnemies(PartyMember member)
    {
        foreach (Enemy enemy in enemies)
        {
            if (cursor.bounds.Intersects(enemy.Collider.bounds) && enemy.IsAlive)
            {
                arrowIndicator.transform.position = enemy.transform.position + new Vector3(-1.5f, 0, 0);
                arrowIndicator.SetActive(true);

                if (Input.GetMouseButton(0))
                {
                    targetPromptText.gameObject.SetActive(false);
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
    void TargetEnemies(PartyMember member, string name, int index, string type, string enemiesTargeted)
    {
        spellBox.SetActive(false);
        spellDescriptionBox.SetActive(false);

        targetPromptText.gameObject.SetActive(true);


        List<Character> targetedCharacters = new List<Character>();

        if (enemiesTargeted == "Multiple")
        {
            if (type == "Heal" || type == "Buff")
            {
                foreach (PartyMember pMember in partyMembers)
                {
                    if (pMember.IsAlive) 
                    {
                        targetedCharacters.Add(pMember);
                    }
                }
            }
            else
            {
                foreach (Enemy enemy in enemies)
                {
                    if (enemy.IsAlive)
                    {
                        targetedCharacters.Add(enemy);
                    }
                }
            }
            member.MagicAttack(targetedCharacters, name);
            targetPromptText.gameObject.SetActive(false);
        }
        else
        {
            if (type == "Heal" || type == "Buff")
            {
                foreach (PartyMember pMember in partyMembers)
                {
                    if (cursor.bounds.Intersects(pMember.Collider.bounds) && pMember.IsAlive)
                    {
                        arrowIndicator.transform.position = pMember.transform.position + new Vector3(-1.5f, 0, 0);
                        arrowIndicator.SetActive(true);

                        if (Input.GetMouseButton(0))
                        {
                            targetedCharacters.Add(pMember);
                            targetPromptText.gameObject.SetActive(false);
                            member.MagicAttack(targetedCharacters, name);
                            
                        }
                    }
                }
            }
            else
            {
                foreach (Enemy enemy in enemies)
                {
                    if (cursor.bounds.Intersects(enemy.Collider.bounds) && enemy.IsAlive)
                    {
                        arrowIndicator.transform.position = enemy.transform.position + new Vector3(-1.5f, 0, 0);
                        arrowIndicator.SetActive(true);

                        if (Input.GetMouseButton(0))
                        {
                            targetedCharacters.Add(enemy);
                            targetPromptText.gameObject.SetActive(false);
                            member.MagicAttack(targetedCharacters, name);
                               
                        }
                    }
                }
            }
        }
    }

    #endregion


    #endregion
}
