using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System;
using Random = UnityEngine.Random;
//using System.Diagnostics;

/// Set of emotions a character could potentially have
public enum Emotion
{
    Happiness,
    Anger,
    Sadness,
    Fear,
    Disgust,
    None
}

/// Stores the names of each stat for easy access and minimal string validation
public enum Stats
{
    Vitality,
    Strength,
    Resolve,
    Fortitude,
    Fortune
}

// Stores the type of buff or debuff
public enum BuffType
{
    Buff,
    Debuff
}

public class Character : MonoBehaviour
{
    /// The GameObject component for the current character
    [SerializeField]
    GameObject character;

    /// Emotion sprites
    [SerializeField]
    SpriteRenderer angerSprite;

    [SerializeField]
    SpriteRenderer sadnessSprite;

    [SerializeField]
    SpriteRenderer fearSprite;

    [SerializeField]
    SpriteRenderer disgustSprite;

    /// Crit and Miss sprites
    [SerializeField]
    SpriteRenderer critSprite;

    [SerializeField]
    SpriteRenderer missSprite;

    /// Offset to control where the sprites appear
    [SerializeField] private Vector3 spriteOffset;

    /// The SpriteRenderer component associated with the current character
    SpriteRenderer srCharacter;

    /// The tombstone sprite used for dead characters
    [SerializeField]
    Sprite tombstone;

    /// True if it is currently this characters' turn, or false otherwise
    protected bool isMyTurn;

    /// The GameManager empty object, used for spells and eventually items
    GameObject gameManager;

    /// Contains the list of all spells that are available in the game.
    Spells spells;

    /// Contains the list of all spells that are known to this character only.
    [SerializeField]
    public List<string> spellList;


    [SerializeField]
    Collider2D collider;

    bool isTargeting;

    bool isUsingSpell;


    public bool IsTargeting { get { return isTargeting; } set { isTargeting = value; } }
    public bool IsUsingSpell { get { return isUsingSpell; } set { isUsingSpell = value; } }

    #region Defining Stats

    protected int currentHealth;
    protected int currentWillpower;

    /// <summary>
    /// The maximum amount of health the character can have
    /// This value will get increased if the character is affected by vitality
    /// </summary>
    [SerializeField]
    protected int maxHealth;

    /// <summary>
    /// The maximum amount of willpower the character can have.
    /// This value will be increased if the character is affected by resolve
    /// </summary>
    [SerializeField]
    protected int maxWillpower;

    /// <summary>
    /// The amount of willpower the character will recover at the end of this turn.
    /// This value will increase if the character chooses to rest this turn
    /// </summary>
    [SerializeField]
    protected int regenWillpower;

    /// <summary>
    /// Influences what the characters' max health is.
    /// As vitality increases, the value of maxHealth will increase with it
    /// </summary>
    [SerializeField]
    protected int vitality;

    /// <summary>
    /// Influences how much damage this character does with physical attacks
    /// </summary>
    [SerializeField]
    protected int strength;

    /// <summary>
    /// Influences the damage this character does with magic attacks, as well as their maximum willpower
    /// </summary>
    [SerializeField]
    protected int resolve;

    /// <summary>
    /// Influences this character's defensive capabilities
    /// </summary>
    [SerializeField]
    protected int fortitude;

    /// <summary>
    /// Determines how lucky the characer is. 
    /// Greater vaues increase crit damage and dodge chances
    /// </summary>
    [SerializeField]
    protected int fortune;

    /// <summary>
    /// Determines what type of damage the character is weak to
    /// </summary>
    [SerializeField]
    protected string weakness;

    /// <summary>
    /// Which emotion this character current has
    /// </summary>
    [SerializeField]
    protected Emotion currentEmotion;

    /// <summary>
    /// Which emotion the character had before gaining the current one
    /// </summary>
    [SerializeField]
    protected Emotion pastEmotion;

    /// <summary>
    /// This stores how many emotion points of each emotion this character has recieved during the battle
    /// Order: Happiness, Anger, Sadness, Fear, Disgust
    /// Once an Emotion eachs 100 Emotion Points (from various sources) the character gets that emotion
    /// and the other emotions are zeroed out.
    /// </summary>
    int[] emotionPoints = new int[5];

    /// <summary>
    /// If a Character has Happiness, their heals & buffs get boosted, but their health & physical attacks get nerfed
    /// </summary>
    bool hasHappiness = false;

    /// <summary>
    /// If a Character has Fear, they can't Rest and all spells will require more Willpower to cast
    /// </summary>
    bool hasFear = false;

    /// <summary>
    /// If a Character has Disgust, they can't Rest and all heals, buffs, and positive effects will do nothing
    /// </summary>
    bool hasDisgust = false;

    /// <summary>
    /// When a character gets a stat buff or debuff, the stats that were changed, given as an array of ints
    /// get stored here. After the spell expires, the stat changes are reversed and it gets deleted.
    /// </summary>
    List<int[]> affectedStats = new List<int[]>();

    /// <summary>
    /// How long a buff or debuff spell lasts. In TurnStart(), each of these get decreased by 1. If any gets to 0,
    /// the buff/debuff is revoked.
    /// </summary>
    List<int> affectedStatsTurncount = new List<int>();

    /// <summary>
    /// The types of buffs or debuffs currently applied. You can have multiple buffs and debuffs active at one time.
    /// </summary>
    List<BuffType> affectedStatsType = new List<BuffType>();

    /// <summary>
    /// Stores whether the Character is alive. If not, their turn is skipped.
    /// </summary>
    public bool isAlive = true;

    #endregion

    public int Health { get { return currentHealth; } }
    public int Willpower { get { return currentWillpower; } }
    public bool IsMyTurn { get { return isMyTurn; } set { isMyTurn = value; } }
    public Collider2D Collider { get { return collider; } }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        spells = gameManager.GetComponent<Spells>();

        srCharacter = GetComponent<SpriteRenderer>();
        isMyTurn = false;
        currentHealth = maxHealth;
        currentWillpower = maxWillpower;

        isTargeting = false;

        angerSprite.enabled = false;
        sadnessSprite.enabled = false;
        fearSprite.enabled = false;
        disgustSprite.enabled = false;
        critSprite.enabled = false;
        missSprite.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Deals physical damage to a specified target
    /// </summary>
    /// <param name="target">The 'Character' component of the target</param>
    public void PhysicalAttack(Character target)
    {
        int crit = Crit();
        if (target != null)
        {
            if (crit > 0)
            {
                critSprite.transform.position = target.transform.position + spriteOffset + new Vector3(0.0f, 0.75f, 3.0f);
                StartCoroutine(ShowSpriteAndFade(critSprite)); // Show crit sprite
                target.TakeDamage("physical", 4 + strength + crit, "physical");
            }
            else
            {
                target.TakeDamage("physical", 4 + strength, "physical");
            }

        }

        TurnEnd();
    }

    //Deals magical type damage to the target. Magic spells can have multiple different subtypes,
    //such as fire, ice, electricity, etc
    //Gets called when player selects "Use Spell" option

    /// <summary>
    /// Deals magic damage of the specified type to a specified target
    /// </summary>
    /// <param name="target">The 'Character' component of desired the target</param>
    /// <param name="spellType">The type of the spell to cast</param>
    /// <param name="spellCost">The amount of willpower required to cast this spell</param>
    public void MagicAttack(List<Character> targetList, string spellName)
    {
        if (targetList.Count == 0)
        {
            TurnEnd();
            return;
        }

        var spell = spells.GetSpell(spellName);


        // If the character has fear, then all of their spells require more willpower to cast
        if (hasFear)
        {
            spell.willpowerCost = spell.willpowerCost + ((int)(spell.willpowerCost * 0.5f));
        }

        // If the character has enough willpower to cast the spell
        if (currentWillpower >= spell.willpowerCost)
        {
            //First, the spell increases Emotion, then is case
            //Order of Emotion Point Array: Happiness, Anger, Sadness, Fear, Disgust
            switch (spell.emotion)
            {
                case Emotion.Happiness: emotionPoints[0] += spell.emotionPoints; break;
                case Emotion.Anger: emotionPoints[1] += spell.emotionPoints; break;
                case Emotion.Sadness: emotionPoints[2] += spell.emotionPoints; break;
                case Emotion.Fear: emotionPoints[3] += spell.emotionPoints; break;
                case Emotion.Disgust: emotionPoints[4] += spell.emotionPoints; break;
            }

            switch (spell.type)
            {
                case "Heal":
                    Heal(spell.damageAmount);
                    break;
                case "Buff":
                    Buff(spell, targetList);
                    break;
                case "Debuff":
                    Buff(spell, targetList);
                    break;
                case "Emotion":
                    //Emotion spells just increase your Emotion and nothing else
                    TurnEnd();
                    break;
                default:
                    for (int i = 0; i < targetList.Count; i++)
                    {
                        targetList[i].TakeDamage("spell", spell.damageAmount + resolve + Crit(), spell.type);
                    }
                    TurnEnd();
                    break;
            }
        }
        else
        {
            // TODO: Implement functionality for what happens if you try to cast a spell but don't have enough willpower
            UnityEngine.Debug.Log("You do not have enough Willpower to cast this spell");
        }
    }

    /// <summary>
    /// Ends your turn, resets your emotion to none, and increases your willpower regen
    /// Ths function gets called when the player selects the "Rest" option in the battle menu
    /// Resting does not work if the Character has Fear or Disgust
    /// </summary>
    public void Rest()
    {
        if (hasFear == false && hasDisgust == false)
        {
            ChangeEmotion(Emotion.None);

        }
        TurnEnd(regenWillpower + 3);
    }

    //Takes damage. First determines if a dodge occurs. If not, physical attacks get reduced
    //by the fortitude stat. If the character is weak to a certain spell type, they take increased damage.
    public void TakeDamage(string damageType, int damageAmount, string spellType)
    {
        if (Dodge())
        {
            missSprite.transform.position = transform.position + spriteOffset;
            StartCoroutine(ShowSpriteAndFade(missSprite)); // Show miss sprite
            return;
        }

        if (damageType == "physical")
        {
            damageAmount = fortitude > damageAmount ? 0 : damageAmount - fortitude;
        }

        if (spellType == weakness)
        {
            damageAmount += 3;
        }

        currentHealth -= damageAmount;

        FindObjectOfType<UIScript>().ShowDamagePopup(transform.position, damageAmount, spriteOffset);

        currentHealth = currentHealth < 0 ? 0 : currentHealth;

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    /// <summary>
    /// Heals the character by the specified amount
    /// </summary>
    /// <param name="healAmount">Amount of health to heal</param>
    public void Heal(int healAmount)
    {
        // Happiness increases the effectiveness of healing
        if (hasHappiness)
        {
            healAmount += 3;
        }

        // Disgust prevents healing from having any positive effect
        // You can still cast it, but it won't do anything
        if (!hasDisgust)
        {
            currentHealth += (healAmount + Crit());
        }

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    //TODO: Implement spells that buff or debuff certain stats
    public void Buff(Spells.Spell spell, List<Character> targetList)
    {
        // Guide: { vitality, strength, resolve, fortitude, fortune }
        for (int i = 0; i < targetList.Count; i++)
        {
            // oldStats hold the previous stats of the Character, used for Debuffs only
            // Here is how it works: If a stat goes negative due to a debuff, it will be set to 0
            // When the debuff wears off, the stat gets added back to it's original value
            // by adding back to the stat. However, say you have two mana, and subtract three mana
            // due to a debuff. You now have -1 mana, so it gets set to 0 mana. When the debuff
            // wears off, it would normally add the value back, so 3 mana would be added back after
            // the debuff ends. However, you started at only 2 mana. So instead, we store 2 in oldStat
            // and add 2 back, instead of 3. 
            int[] oldStats = new int[5];

            if (spell.type == "Buff")
            {
                // If the Character has happiness, buffs will be more effective
                // If a buff does not touch a stat, it gets skipped for Happy buff

                if (hasHappiness)
                {
                    for (int j = 0; j < spell.statChanges.Length; j++)
                    {
                        if (spell.statChanges[j] > 0)
                        {
                            spell.statChanges[j] += 2;
                        }
                    }
                }

                // Disgust will prevent buffs from having an effect
                // You can still cast it, but it won't do anything

                if (!hasDisgust)
                {
                    targetList[i].vitality += spell.statChanges[0];
                    targetList[i].strength += spell.statChanges[1];
                    targetList[i].resolve += spell.statChanges[2];
                    targetList[i].fortitude += spell.statChanges[3];
                    targetList[i].fortune += spell.statChanges[4];
                }
            }
            else if (spell.type == "Debuff")
            {
                oldStats[0] = targetList[i].vitality;
                oldStats[1] = targetList[i].strength;
                oldStats[2] = targetList[i].resolve;
                oldStats[3] = targetList[i].fortitude;
                oldStats[4] = targetList[i].fortune;

                targetList[i].vitality -= spell.statChanges[0];
                targetList[i].strength -= spell.statChanges[1];
                targetList[i].resolve -= spell.statChanges[2];
                targetList[i].fortitude -= spell.statChanges[3];
                targetList[i].fortune -= spell.statChanges[4];

                // Sets stats to 0 if they are negative, and makes sure the 
                // stat will be reset to the correct amount
                if (targetList[i].vitality <= 0)
                {
                    targetList[i].vitality = 0;
                    spell.statChanges[0] = oldStats[0];
                }
                if (targetList[i].strength <= 0)
                {
                    targetList[i].strength = 0;
                    spell.statChanges[1] = oldStats[1];
                }
                if (targetList[i].resolve <= 0)
                {
                    targetList[i].resolve = 0;
                    spell.statChanges[2] = oldStats[2];
                }
                if (targetList[i].fortitude <= 0)
                {
                    targetList[i].fortitude = 0;
                    spell.statChanges[3] = oldStats[3];
                }
                if (targetList[i].fortune <= 0)
                {
                    targetList[i].fortune = 0;
                    spell.statChanges[4] = oldStats[4];
                }
            }

            targetList[i].affectedStats.Add(spell.statChanges);
            targetList[i].affectedStatsTurncount.Add(spell.turnCount);

            // Takes the string spell type and adds the correct BuffType enum
            if (spell.type == "Buff")
            {
                targetList[i].affectedStatsType.Add(BuffType.Buff);
            }
            else
            {
                targetList[i].affectedStatsType.Add(BuffType.Debuff);
            }

            // TODO: Make a new function to reverse the buff/ debuff
        }
    }

    /// <summary>
    /// Checks to see if any of the buff or debuff spells have expired. If they have, reverse their effects
    /// </summary>
    public void BuffEnd()
    {
        for (int i = 0; i < affectedStatsTurncount.Count; i++)
        {
            affectedStatsTurncount[i]--;

            if (affectedStatsTurncount[i] <= 0)
            {
                if (affectedStatsType[i] == BuffType.Buff)
                {
                    vitality -= affectedStats[i][0];
                    strength -= affectedStats[i][1];
                    resolve -= affectedStats[i][2];
                    fortitude -= affectedStats[i][3];
                    fortune -= affectedStats[i][4];
                }
                else
                {
                    vitality += affectedStats[i][0];
                    strength += affectedStats[i][1];
                    resolve += affectedStats[i][2];
                    fortitude += affectedStats[i][3];
                    fortune += affectedStats[i][4];
                }

                // Gets rid of buff/debuff data from this Character
                affectedStats.RemoveAt(i);
                affectedStatsTurncount.RemoveAt(i);
                affectedStatsType.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Determines whether a given attack is a crit
    /// </summary>
    /// <returns>True if the attack crits, false otherwise</returns>
    public int Crit()
    {
        int critChance = Random.Range(0, 100) + fortune;
        return critChance >= 10 ? 0 : 3 + fortune;
    }

    /// <summary>
    /// Determines whether the character dodges an incoming attack
    /// </summary>
    /// <returns>True if the character dodges, false otherwise</returns>
    public bool Dodge()
    {
        int dodgeChance = Random.Range(0, 100) + fortune;
        return dodgeChance >= 95 ? true : false;
    }

    /// <summary>
    /// This is for making the crit and miss sprites have a fading aspect
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    private IEnumerator ShowSpriteAndFade(SpriteRenderer sprite)
    {
        sprite.enabled = true; // Show the sprite
        float duration = 1f; // Duration to show
        yield return new WaitForSeconds(duration); // Wait for a second

        // Fade out
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            Color color = sprite.color;
            color.a = Mathf.Lerp(1, 0, t); // Fade to transparent
            sprite.color = color;
            yield return null; // Wait for the next frame
        }

        sprite.enabled = false; // Hide the sprite after fading
    }

    /// <summary>
    /// Runs before a character takes their turn. Calls BuffEnd to 
    /// decrease already applied stat buffs and debuffs
    /// </summary>
    public void TurnStart()
    {
        BuffEnd();
    }

    /// <summary>
    /// Runs after a character takes their action for the turn, and causes willpower to be regenerated
    /// This function being called also tells the GameManager that this character's turn is complete
    /// </summary>
    public void TurnEnd()
    {
        isMyTurn = false;
        isTargeting = false;
        TurnEnd(regenWillpower);
    }

    /// <summary>
    /// Runs after a character takes their action for the turn, and causes willpower to be regenerated
    /// This function being called also tells the GameManager that this character's turn is complete
    /// </summary>
    public void TurnEnd(int willpowerAmount)
    {
        currentWillpower += willpowerAmount;
        currentWillpower = currentWillpower > maxWillpower ? maxWillpower : currentWillpower;
        CheckEmotion();
        isMyTurn = false;
        UnityEngine.Debug.Log("Reached end of TurnEnd()");
    }

    /// <summary>
    /// Checks if any of the emotions have reached 100 emotion points
    /// If so, changes the emotion to that newly reached emotion
    /// </summary>
    public void CheckEmotion()
    {
        /// Order: Happiness, Anger, Sadness, Fear, Disgust

        if (emotionPoints[0] >= 100) { ChangeEmotion(Emotion.Happiness); }
        else if (emotionPoints[1] >= 100) { ChangeEmotion(Emotion.Anger); }
        else if (emotionPoints[2] >= 100) { ChangeEmotion(Emotion.Sadness); }
        else if (emotionPoints[3] >= 100) { ChangeEmotion(Emotion.Fear); }
        else if (emotionPoints[4] >= 100) { ChangeEmotion(Emotion.Disgust); }
    }

    /// <summary>
    /// Changes the character's emotion to the specified new one
    /// </summary>
    /// <param name="newEmotion">The new emotion to apply to the character</param>
    public void ChangeEmotion(Emotion newEmotion)
    {
        pastEmotion = currentEmotion;
        currentEmotion = newEmotion;

        hasFear = false;
        hasDisgust = false;
        hasHappiness = false;

        /// Resets the emotion points values
        emotionPoints = new int[5];

        switch (currentEmotion)
        {
            case Emotion.None:
                switch (pastEmotion)
                {
                    case Emotion.Anger:
                        strength -= 5;
                        resolve -= 5;
                        fortitude += 5;
                        fortune += 5;
                        break;
                    case Emotion.Sadness:
                        fortitude -= 5;
                        fortune -= 5;
                        strength += 5;
                        resolve += 5;
                        break;
                    case Emotion.Happiness:
                        strength += 5;
                        currentHealth += 5;
                        maxHealth += 5;
                        break;
                }
                srCharacter.color = Color.white;

                angerSprite.enabled = false;
                sadnessSprite.enabled = false;
                // happinessSprite.enabled = false;
                fearSprite.enabled = false;
                disgustSprite.enabled = false;
                break;
            case Emotion.Happiness:
                hasHappiness = true;
                strength -= 5;
                currentHealth -= 5;
                maxHealth -= 5;

                if (currentHealth <= 0) { currentHealth = 1; }
                if (maxHealth <= 0) { maxHealth = 1; }
                if (strength < 0) { strength = 0; }

                srCharacter.color = Color.yellow;

                // TODO: Add a happinessSprite
                //happinessSprite.enabled = true;
                //happiness.transform.position = transform.position + spriteOffset;
                break;
            case Emotion.Anger:
                strength += 5;
                resolve += 5;
                fortitude -= 5;
                fortune -= 5;

                if (fortitude < 0) { fortitude = 0; }
                if (fortune < 0) { fortune = 0; }

                srCharacter.color = Color.red;

                angerSprite.enabled = true;
                angerSprite.transform.position = transform.position + spriteOffset;
                break;
            case Emotion.Sadness:
                fortitude += 5;
                fortune += 5;
                strength -= 5;
                resolve -= 5;

                if (strength < 0) { strength = 0; }
                if (resolve < 0) { resolve = 0; }

                srCharacter.color = Color.blue;

                sadnessSprite.enabled = true;
                sadnessSprite.transform.position = transform.position + spriteOffset;
                break;
            case Emotion.Fear:
                hasFear = true;
                srCharacter.color = new Color(160f, 32f, 240f, 1f); //purple

                fearSprite.enabled = true;
                fearSprite.transform.position = transform.position + spriteOffset;
                break;
            case Emotion.Disgust:
                hasDisgust = true;
                srCharacter.color = Color.green;

                disgustSprite.enabled = true;
                disgustSprite.transform.position = transform.position + spriteOffset;
                break;
        }
    }

    /// <summary>
    /// Sets them to dead, removes their emotions, and sets their sprite to tombstone
    /// </summary>
    public void Death()
    {
        isAlive = false;
        ChangeEmotion(Emotion.None);
        srCharacter.sprite = tombstone;
    }
}
