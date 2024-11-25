using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
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
    Sprite[] damageNumbers;

    [SerializeField]
    Sprite[] healNumbers;
    
    [SerializeField]
    Sprite[] willpowerNumbers;

    [SerializeField]
    List<Button> buttons;

    [SerializeField]
    GameObject pauseBG;

    bool pauseBool = false;

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
    GameObject targetSelectArrowIndicator;

    [SerializeField]
    TextMeshProUGUI targetPromptText;

    AnimationManager animManager;


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

        buttons[0].interactable = !buttons[0].interactable;
        buttons[1].interactable = !buttons[1].interactable;
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
        buttons[0].interactable = true;
        buttons[1].interactable = true;
    }

    /// <summary>
    /// quits game
    /// </summary>
    public void OnQuit() { Application.Quit(); }

    /// <summary>
    /// restarts the scene
    /// </summary>
    public void OnRestart() { SceneManager.LoadScene(0); }

    public void OnPause()
    {
        pauseBool = !pauseBool;
        quitButton.SetActive(pauseBool);
        restartButton.SetActive(pauseBool);
        pauseBG.SetActive(pauseBool);
    }

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

        // Hide things that do not need to be seen at start
        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        arrowIndicator.SetActive(false);
        targetSelectArrowIndicator = Instantiate(arrowIndicator);
        targetSelectArrowIndicator.transform.localScale = new Vector3(2, 2, 1);
        targetSelectArrowIndicator.SetActive(false);
        spellBox.SetActive(false);
        spellDescriptionBox.SetActive(false);
        targetPromptText.gameObject.SetActive(false);
        animManager = gameObject.GetComponent<AnimationManager>();
    }

    void Update()
    {
        // Check for win/lose conditions
        CheckGameOver();

        //checking for if the player has paused the game
        if (pauseBool)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }

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

                if (partyMembers[i].HasFear || partyMembers[i].HasDisgust)
                {
                    buttons[1].enabled = false;
                    //buttons[1].interactable = false;
                    buttons[1].targetGraphic.color = Color.gray;
                    //buttons[1].GetComponent<SpriteRenderer>().color = Color.gray;
                }
                else
                {
                    buttons[1].enabled = true;
                    //buttons[1].interactable = true;
                    buttons[1].targetGraphic.color = Color.white;
                    //buttons[1].GetComponent<Button>(). = Color.white;
                }
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

                    // The UI elements will hide while an animated sprite is playing
                    targetPromptText.gameObject.SetActive(!animManager.IsActive);

                    //spells
                    if (partyMembers[i].IsUsingSpell)
                    {
                        //todo: add the new turn indicator 
                        //arrowIndicator.transform.position = spellText.transform.position + new Vector3(0, +0.8f, 0);
                        //arrowIndicator.SetActive(true);

                        string[] spellInfo = partyMembers[i].GetSpellInfo(spellNames[spellListIndex]);


                        TargetCharacters(partyMembers[i], spellNames[spellListIndex], spellListIndex, spellInfo[0], spellInfo[1], spellInfo[2]);
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
                    targetSelectArrowIndicator.SetActive(false);
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
                targetSelectArrowIndicator.transform.position = enemy.transform.position + new Vector3(-1.5f, 0, 0);
                targetSelectArrowIndicator.SetActive(!animManager.IsActive);

                //Ensure there's no animations played during the click, preventing double attacks
                if (Input.GetMouseButtonUp(0) && !animManager.IsActive)
                {
                    targetPromptText.gameObject.SetActive(false);
                    StartCoroutine(member.PhysicalAttack(enemy));
                }
            }
        }
    }

    /// <summary>
    /// targets an enemy or player with a specific spell or if spell is multi targeted targets all
    /// </summary>
    /// <param name="member"></param>
    /// <param name="isSpell"></param>
    void TargetCharacters(PartyMember member, string name, int index, string type, string charactersTargeted, string emotion)
    {
        spellBox.SetActive(false);
        spellDescriptionBox.SetActive(false);


        List<Character> targetedCharacters = new List<Character>();

        if (charactersTargeted == "Multiple")
        {
            targetPromptText.gameObject.SetActive(false);
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
            if (Input.GetMouseButton(0))
            {
                StartCoroutine(member.MagicAttack(targetedCharacters, name));
                
            }
        }
        else
        {
            if (type == "Heal" || type == "Buff")
            {
                foreach (PartyMember pMember in partyMembers)
                {
                    if (cursor.bounds.Intersects(pMember.Collider.bounds) && pMember.IsAlive)
                    {
                        targetSelectArrowIndicator.transform.position = pMember.transform.position + new Vector3(-1.5f, 0, 0);
                        targetSelectArrowIndicator.SetActive(!animManager.IsActive);

                        if (Input.GetMouseButtonUp(0) && !animManager.IsActive)
                        {
                            targetedCharacters.Add(pMember);
                            targetPromptText.gameObject.SetActive(false);
                            StartCoroutine(member.MagicAttack(targetedCharacters, name));

                        }
                    }
                }
            }
            else if (type == "Emotion")
            {
                if (emotion != "Fear" && emotion != "Disgust")
                {
                    foreach (PartyMember pMember in partyMembers)
                    {
                        if (cursor.bounds.Intersects(pMember.Collider.bounds) && pMember.IsAlive)
                        {
                            targetSelectArrowIndicator.transform.position = pMember.transform.position + new Vector3(-1.5f, 0, 0);
                            targetSelectArrowIndicator.SetActive(!animManager.IsActive);

                            if (Input.GetMouseButtonUp(0) && !animManager.IsActive)
                            {
                                targetedCharacters.Add(pMember);
                                targetPromptText.gameObject.SetActive(false);
                                StartCoroutine(member.MagicAttack(targetedCharacters, name));
                            }
                        }
                    }
                }
                foreach (Enemy enemy in enemies)
                {
                    if (cursor.bounds.Intersects(enemy.Collider.bounds) && enemy.IsAlive)
                    {
                        targetSelectArrowIndicator.transform.position = enemy.transform.position + new Vector3(-1.5f, 0, 0);
                        targetSelectArrowIndicator.SetActive(!animManager.IsActive);

                        if (Input.GetMouseButtonUp(0) && !animManager.IsActive)
                        {
                            targetedCharacters.Add(enemy);
                            targetPromptText.gameObject.SetActive(false);
                            StartCoroutine(member.MagicAttack(targetedCharacters, name));
                        }
                    }
                }
                //member.MagicAttack(targetedCharacters, name);
            }
            else
            {
                foreach (Enemy enemy in enemies)
                {
                    if (cursor.bounds.Intersects(enemy.Collider.bounds) && enemy.IsAlive)
                    {
                        targetSelectArrowIndicator.transform.position = enemy.transform.position + new Vector3(-1.5f, 0, 0);
                        targetSelectArrowIndicator.SetActive(!animManager.IsActive);

                        if (Input.GetMouseButtonUp(0) && !animManager.IsActive)
                        {
                            targetedCharacters.Add(enemy);
                            targetPromptText.gameObject.SetActive(false);
                            StartCoroutine(member.MagicAttack(targetedCharacters, name));
                        }
                    }
                }
                //TargetHelper(true, member, name);
            }
        }
    }

    //future targeting helper function
    void TargetHelper(bool isEnemy, PartyMember member, string spellName)
    {

        List<Character> targets = new List<Character>();

        if (isEnemy) 
        {
            foreach (Enemy enemy in enemies)
            {
                if (cursor.bounds.Intersects(enemy.Collider.bounds) && enemy.IsAlive)
                {
                    targetSelectArrowIndicator.transform.position = enemy.transform.position + new Vector3(-1.5f, 0, 0);
                    targetSelectArrowIndicator.SetActive(true);

                    if (Input.GetMouseButtonUp(0))
                    {
                        targets.Add(enemy);
                        targetPromptText.gameObject.SetActive(false);
                    }
                }
            }
        }
        else 
        {
            foreach (PartyMember pMember in partyMembers)
            {
                if (cursor.bounds.Intersects(pMember.Collider.bounds) && pMember.IsAlive)
                {
                    targetSelectArrowIndicator.transform.position = pMember.transform.position + new Vector3(-1.5f, 0, 0);
                    targetSelectArrowIndicator.SetActive(true);

                    if (Input.GetMouseButtonUp(0))
                    {
                        targets.Add(pMember);
                        targetPromptText.gameObject.SetActive(false);
                    }
                }
            }
        }
        member.MagicAttack(targets, name);
    }


    #endregion


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
    public void ShowNumberPopup(Vector3 position, int damageAmount, Vector3 offset, string numberType)
    {
        Sprite[] sourceNumbers;
        switch (numberType)
        {
            case "Heal":
                sourceNumbers = healNumbers;
                break;
            case "Willpower":
                sourceNumbers = willpowerNumbers;
                break;
            default:
                sourceNumbers = damageNumbers;
                break;
        }

        GameObject[] damageNumberArray;
        if (damageAmount > 9)
        {
            damageNumberArray = new GameObject[2];
            damageNumberArray[0] = CreateNumber((int)Mathf.Floor(damageAmount / 10), position + offset, sourceNumbers);
            while (damageAmount >= 10)
            {
                damageAmount -= 10;
            }
            damageNumberArray[1] = CreateNumber(damageAmount, position + offset + new Vector3(0.4f, 0, 0), sourceNumbers);
        }
        else
        {
            damageNumberArray = new GameObject[1];
            damageNumberArray[0] = CreateNumber(damageAmount, position + offset, sourceNumbers);
        }
       
        StartCoroutine(FadeOutPopup(damageNumberArray));
    }

    private GameObject CreateNumber(int number, Vector3 position, Sprite[] sourceNumbers)
    {
        GameObject displayNumber = new();
        displayNumber.AddComponent<SpriteRenderer>().sprite = sourceNumbers[number];
        displayNumber.transform.position = position;
        displayNumber.transform.localScale = new Vector3(5, 5, 5);

        return displayNumber;
    }

    /// <summary>
    /// This makes the text that shows the damage fade away after a second
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutPopup(GameObject[] numbers)
    {
        yield return new WaitForSeconds(1f);

        // Fade out
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            foreach (GameObject number in numbers)
            {
                Color color = number.GetComponent<SpriteRenderer>().color;
                color.a = Mathf.Lerp(1, 0, t); // Fade to transparent
                number.GetComponent<SpriteRenderer>().color = color;
                yield return null; // Wait for the next frame
            }
        }

        // Hide the text after fading out
        for (int i = 0; i < numbers.Length; i++)
        {
            Destroy(numbers[i]);
        }
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

   
    #endregion
}
