using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static Spells;

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

    [SerializeField]
    GameObject[] emotionSprites;
    GameObject displayEmotion;

    [SerializeField]
    SpriteRenderer critSprite;

    [SerializeField]
    SpriteRenderer missSprite;

    /// Stat icons and arrows for displaying stat buffs and debuffs
    [SerializeField]
    GameObject[] statIcons;
    bool[] areIconsUsed = new bool[4];

    [SerializeField]
    GameObject boostArrow;

    [SerializeField]
    GameObject dropArrow;

    List<GameObject> buffIcons = new();
    List<GameObject> buffArrows = new();

    /// Offset to control where the sprites appear
    [SerializeField] 
    private Vector3 spriteOffset;

    /// The SpriteRenderer component associated with the current character
    SpriteRenderer srCharacter;

    /// The tombstone sprite used for dead characters
    [SerializeField]
    Sprite tombstone;

    /// True if it is currently this characters' turn, or false otherwise
    protected bool isMyTurn;

    /// The GameManager empty object, used for spells and eventually items
    GameObject gameManager;

    /// The animation manager, used to display emotion and spell effects
    AnimationManager animManager;

    /// Contains the list of all spells that are available in the game.
    Spells spells;

    /// Contains the list of all spells that are known to this character only.
    [SerializeField]
    public List<string> spellList;

    //TODO: make a list of these spells using the names of them so that the UI manager can easily reference a spell's name, cost, damage, etc
    public Spells GlobalSpellList { get => spells; }

    [SerializeField]
    Collider2D collider;

    bool isTargeting;

    bool isUsingSpell;

    int healthAmount;


    public bool IsTargeting { get => isTargeting; set => isTargeting = value;  }
    public bool IsUsingSpell { get => isUsingSpell; set => isUsingSpell = value ; }

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
    bool isAlive = true;
    

    #endregion

    public int Health { get => currentHealth; }
    public int Willpower { get => currentWillpower; }
    public bool IsMyTurn { get => isMyTurn; set => isMyTurn = value; }

    public bool IsAlive { get => isAlive; }
    public Collider2D Collider { get => collider; }

    public bool HasFear { get => hasFear; }
    public bool HasDisgust { get => hasDisgust; }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        animManager = gameManager.GetComponent<AnimationManager>();
        spells = gameManager.GetComponent<Spells>();

        srCharacter = GetComponent<SpriteRenderer>();
        isMyTurn = false;
        currentHealth = maxHealth;
        currentWillpower = maxWillpower;

        isTargeting = false;

        critSprite.enabled = false;
        missSprite.enabled = false;

        for (int i = 0; i < areIconsUsed.Length; i++)
        {
            areIconsUsed[i] = false;
        }
    }

    // Update is called once per frame
    void Update() { }

    /// <summary>
    /// Deals physical damage to a specified target
    /// </summary>
    /// <param name="target">The 'Character' component of the target</param>
    public IEnumerator PhysicalAttack(Character target)
    {
        int crit = Crit();
        if (target != null)
        {
            yield return StartCoroutine(animManager.AnimateSprite("Attack", target.transform.position));
            while (animManager.IsActive) { /*Do absolutely nothing*/ }
            if (crit > 0)
            {
                critSprite.transform.position = target.transform.position + spriteOffset + new Vector3(1f, 0, 0);
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
    public IEnumerator MagicAttack(List<Character> targetList, string spellName)
    {
        if (targetList.Count == 0)
        {
            TurnEnd();
            yield return null;
        }

        var spell = spells.GetSpell(spellName);

        Debug.Log($"{spell.name} : {spell.damageAmount}");


        // If the character has fear, then all of their spells require more willpower to cast
        if (hasFear)
        {
            spell.willpowerCost += (int)(spell.willpowerCost * 0.5f);
        }
        
        // If the character has enough willpower to cast the spell
        if (currentWillpower >= spell.willpowerCost)
        {
            //First, the spell increases Emotion, then is case
            //Order of Emotion Point Array: Happiness, Anger, Sadness, Fear, Disgust
            if (spell.type != "Emotion")
            {
                switch (spell.emotion)
                {
                    case Emotion.Happiness: emotionPoints[0] += spell.emotionPoints; break;
                    case Emotion.Anger: emotionPoints[1] += spell.emotionPoints; break;
                    case Emotion.Sadness: emotionPoints[2] += spell.emotionPoints; break;
                    case Emotion.Fear: emotionPoints[3] += spell.emotionPoints; break;
                    case Emotion.Disgust: emotionPoints[4] += spell.emotionPoints; break;
                }
            }
            currentWillpower -= spell.willpowerCost;
            switch (spell.type)
            {
                case "Heal":
                    Heal(spell.damageAmount, targetList);
                    foreach (var c in targetList)
                    {
                        FindObjectOfType<UIScript>().ShowNumberPopup(c.transform.position, healthAmount, spriteOffset, "Heal");
                    }
                    break;
                case "Buff":
                case "Debuff":
                    yield return StartCoroutine(Buff(spell, targetList));
                    break;
                case "Emotion":
                    //Emotion spells just increase your Emotion and nothing else
                    foreach (var c in targetList)
                    {
                        c.AddEmotionPoints(spell.emotion, spell.emotionPoints);
                    }
                    break;
                default:
                    if (targetList.Count > 1)
                    {
                        Vector3[] positions = new Vector3[targetList.Count];
                        for(int i = 0; i < targetList.Count; i++)
                        {
                            positions[i] = targetList[i].transform.position;
                        }
                        yield return StartCoroutine(animManager.AnimateMultiTarget(spellName, positions));
                    }
                    else yield return StartCoroutine(animManager.AnimateSprite(spellName, targetList[0].transform.position));
                    for (int i = 0; i < targetList.Count; i++)
                    {
                        targetList[i].TakeDamage("spell", spell.damageAmount + resolve + Crit(), spell.type);
                    }
                    break;
            }
            while (animManager.IsActive) { };
            TurnEnd();
        }
        else
        {
            UnityEngine.Debug.Log("You do not have enough Willpower to cast this spell");
        }
    }

    /// <summary>
    /// returns an array of spell type, spell description, and how many characters it targets based on what spell name is given
    /// </summary>
    /// <param name="spellName"></param>
    /// <returns></returns>
    public string[] GetSpellInfo(string spellName)
    {
        //return new string[] {spells.GetSpell(spellName).type, spells.GetSpell(spellName).target, spells.GetSpell(spellName).description};
        return new string[] {spells.GetSpell(spellName).type, spells.GetSpell(spellName).target, spells.GetSpell(spellName).emotion.ToString()};
    }

    /// <summary>
    /// gets the damage and willpower cost of a spell
    /// </summary>
    /// <param name="spellName"></param>
    /// <returns></returns>
    public int[] GetSpellStats(string spellName)
    {
        return new int[] { spells.GetSpell(spellName).damageAmount, spells.GetSpell(spellName).willpowerCost };
    }

    /// <summary>
    /// Ends your turn, resets your emotion to none, and increases your willpower regen
    /// Ths function gets called when the player selects the "Rest" option in the battle menu
    /// Resting does not work if the Character has Fear or Disgust
    /// </summary>
    public void Rest()
    {
        if (!hasFear && !hasDisgust)
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

        if (damageType == "physical") { damageAmount = fortitude > damageAmount ? 0 : damageAmount - fortitude; }

        if (spellType == weakness) { damageAmount += 3; }

        currentHealth -= damageAmount;

        FindObjectOfType<UIScript>().ShowNumberPopup(transform.position, damageAmount, spriteOffset, "");

        currentHealth = currentHealth < 0 ? 0 : currentHealth;

        if (currentHealth <= 0) { Death(); }
    }

    /// <summary>
    /// Heals the character by the specified amount
    /// </summary>
    /// <param name="healAmount">Amount of health to heal</param>
    //public void Heal(int healAmount)
    public void Heal(int healAmount, List<Character> targetList)
    {
        for (int i = 0; i < targetList.Count; i++)
        {
            // Happiness increases the effectiveness of healing
            if (targetList[i].hasHappiness)
            {
                healAmount += 3;
            }

            int healCrit = healAmount + Crit();

            // Disgust prevents healing from having any positive effect
            // You can still cast it, but it won't do anything
            if (!targetList[i].hasDisgust)
            {
                targetList[i].currentHealth += healCrit;
            }
            if (targetList[i].currentHealth > targetList[i].maxHealth)
            {
                targetList[i].currentHealth = targetList[i].maxHealth;
            }

            healthAmount = healCrit;
        }
    }

    //TODO: Implement spells that buff or debuff certain stats
    public IEnumerator Buff(Spell spell, List<Character> targetList)
    {
        //Animate the buffs or debuffs first
        foreach (Character target in targetList)
        {
            animManager.Enqueue(animManager.DetermineBuff(spell.statChanges, spell.type == "Debuff"), target.transform.position);
        }
        if (animManager.animQueueCount > 0) yield return StartCoroutine(animManager.PlayQueue());
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
            targetList[i].DisplayBuffs(spell.statChanges, spell.type);
            // TODO: Make a new function to reverse the buff/ debuff
        }
        yield return null;
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
                Destroy(buffIcons[i]);
                buffIcons.RemoveAt(i);
                Destroy(buffArrows[i]);
                buffArrows.RemoveAt(i);
                ResetUsedBuffIcons();
                i--;
                // Shift the other buffs to the left
                for (int j = 0; j < buffIcons.Count; j++)
                {
                    buffIcons[j].transform.position -= new Vector3(0.5f, 0, 0);
                    buffArrows[j].transform.position -= new Vector3(0.5f, 0, 0);
                }
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
        //int critChance = 1;
        return critChance >= 10 ? 0 : 3 + fortune;
    }

    /// <summary>
    /// Determines whether the character dodges an incoming attack
    /// </summary>
    /// <returns>True if the character dodges, false otherwise</returns>
    public bool Dodge()
    {
        int dodgeChance = Random.Range(0, 100) + fortune;
        return dodgeChance >= 95;
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
    /// Runs after a character takes their action for the turn, and causes willpower to be regenerated
    /// This function being called also tells the GameManager that this character's turn is complete
    /// </summary>
    public void TurnEnd()
    {
        BuffEnd();
        //isMyTurn = false;
        isTargeting = false;
        //TurnEnd(regenWillpower)
        CheckEmotion();
        isMyTurn = false;
        UnityEngine.Debug.Log("Reached end of TurnEnd()");
    }

    /// <summary>
    /// Runs after a character takes their action for the turn, and causes willpower to be regenerated
    /// This function being called also tells the GameManager that this character's turn is complete
    /// </summary>
    public void TurnEnd(int willpowerAmount)
    {
        BuffEnd();
        currentWillpower += willpowerAmount;
        FindObjectOfType<UIScript>().ShowNumberPopup(transform.position, willpowerAmount, spriteOffset, "Willpower");
        currentWillpower = currentWillpower > maxWillpower ? maxWillpower : currentWillpower;
        CheckEmotion();
        isMyTurn = false;
        UnityEngine.Debug.Log("Reached end of TurnEnd()");
    }

    public void AddEmotionPoints(Emotion emotion, int pointValue)
    {
        switch (emotion)
        {
            case Emotion.Happiness:
                emotionPoints[0] += pointValue;
                break;
            case Emotion.Anger:
                emotionPoints[1] += pointValue;
                break;
            case Emotion.Sadness:
                emotionPoints[2] += pointValue;
                break;
            case Emotion.Fear:
                emotionPoints[3] += pointValue;
                break;
            case Emotion.Disgust:
                emotionPoints[4] += pointValue;
                break;
        }
        CheckEmotion();
    }

    /// <summary>
    /// Checks if any of the emotions have reached 100 emotion points
    /// If so, changes the emotion to that newly reached emotion
    /// </summary>
    public void CheckEmotion()
    {
        /// Order: Happiness, Anger, Sadness, Fear, Disgust

        if (emotionPoints[0] >= 50) { ChangeEmotion(Emotion.Happiness); }
        else if (emotionPoints[1] >= 50) { ChangeEmotion(Emotion.Anger); }
        else if (emotionPoints[2] >= 50) { ChangeEmotion(Emotion.Sadness); }
        else if (emotionPoints[3] >= 50) { ChangeEmotion(Emotion.Fear); }
        else if (emotionPoints[4] >= 50) { ChangeEmotion(Emotion.Disgust); }
    }

    /// <summary>
    /// Changes the character's emotion to the specified new one
    /// </summary>
    /// <param name="newEmotion">The new emotion to apply to the character</param>
    public void ChangeEmotion(Emotion newEmotion)
    {
        Vector3 animOffset;
        bool flipSprite = this is Enemy ? true : false;

        pastEmotion = currentEmotion;
        currentEmotion = newEmotion;

        hasFear = false;
        hasDisgust = false;
        //hasHappiness = false;
        //Delete all the animation sprites if they exist
        animManager.Delete(name + "Happy");
        animManager.Delete(name + "Sad");
        animManager.Delete(name + "Angry");
        animManager.Delete(name + "Afraid");
        animManager.Delete(name + "Disgust");

        /// Resets the emotion points values
        emotionPoints = new int[5];
        Destroy(displayEmotion);

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

                if (this is Enemy)
                    animOffset = new Vector3(-1.1f, 0, 0);
                else
                    animOffset = new Vector3(0.25f, 0.85f, 0);
                animManager.AddLoop("Happy", name, transform.position + animOffset, flipSprite);

                DisplayCurrentEmotion((int)Emotion.Happiness);
                break;
            case Emotion.Anger:
                strength += 5;
                resolve += 5;
                fortitude -= 5;
                fortune -= 5;

                if (fortitude < 0) { fortitude = 0; }
                if (fortune < 0) { fortune = 0; }

                srCharacter.color = Color.red;
                if (this is Enemy)
                    animOffset = new Vector3(-0.65f, -0.2f, 0);
                else
                    animOffset = new Vector3(0.25f, 0.75f, 0);
                animManager.AddLoop("Angry", name, transform.position + animOffset, flipSprite);

                DisplayCurrentEmotion((int)Emotion.Anger);
                break;
            case Emotion.Sadness:
                fortitude += 5;
                fortune += 5;
                strength -= 5;
                resolve -= 5;

                if (strength < 0) { strength = 0; }
                if (resolve < 0) { resolve = 0; }

                srCharacter.color = Color.blue;
                if (this is Enemy)
                    animOffset = new Vector3(-0.9f, -0.5f, 0);
                else
                    animOffset = new Vector3(0.15f, 0.1f, 0);
                animManager.AddLoop("Sad", name, transform.position + animOffset, flipSprite);

                DisplayCurrentEmotion((int)Emotion.Sadness);
                break;
            case Emotion.Fear:
                hasFear = true;
                srCharacter.color = new Color(0.6f, 0.1f, 0.9f, 1f); //purple
                if (this is Enemy)
                    animOffset = new Vector3(-0.85f, -0.3f, 0);
                else
                    animOffset = new Vector3(0, 0.45f, 0);
                animManager.AddLoop("Afraid", name, transform.position + animOffset, flipSprite);

                DisplayCurrentEmotion((int)Emotion.Fear);
                break;
            case Emotion.Disgust:
                hasDisgust = true;
                srCharacter.color = Color.green;
                if (this is Enemy)
                    animOffset = new Vector3(-1.2f, -0.5f, 0);
                else
                    animOffset = new Vector3(0.05f, 0.25f, 0);
                animManager.AddLoop("Disgust", name, transform.position + animOffset, flipSprite);

                DisplayCurrentEmotion((int)Emotion.Disgust);
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

    /// <summary>
    /// Displays a buffed or debuffed stat
    /// </summary>
    private void DisplayBuffs(int[] buffedStat, string buffType)
    {
        for (int j = 1; j < buffedStat.Length; j++)
        {
            if (buffedStat[j] > 0 && !areIconsUsed[j - 1])
            {
                //Create a buff icon and add it to the list
                GameObject buffIcon = Instantiate(statIcons[j]);
                areIconsUsed[j - 1] = true;
                buffIcon.transform.position = transform.position + new Vector3(0.6f + 0.5f * buffIcons.Count, -0.6f, -6);
                buffIcon.SetActive(true);
                buffIcons.Add(buffIcon);

                //Create an arrow for this buff icon
                GameObject buffArrow = buffType == "Buff" ? Instantiate(boostArrow) : Instantiate(dropArrow);
                buffArrow.transform.position = buffIcon.transform.position + new Vector3(0.1f, -0.1f, -1);
                buffArrow.SetActive(true);
                buffArrows.Add(buffArrow);

            }
        }
    }

    private void ResetUsedBuffIcons()
    {
        for (int i = 0; i < areIconsUsed.Length; i++)
        {
            foreach (int[] buffs in affectedStats)
            {
                areIconsUsed[i] = buffs[i + 1] > 0;
            }
            if (affectedStats.Count == 0) areIconsUsed[i] = false;
        }
    }

    private void DisplayCurrentEmotion(int index)
    {
        displayEmotion = Instantiate(emotionSprites[index]);
        displayEmotion.transform.position = transform.position + spriteOffset;
        displayEmotion.SetActive(true);
    }
}
