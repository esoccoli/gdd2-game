using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System;
using Random = UnityEngine.Random;

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

public class Character : MonoBehaviour
{
    /// The GameObject component for the current character
    [SerializeField]
    GameObject character;

    /// The SpriteRenderer component associated with the current character
    SpriteRenderer srCharacter;

    /// True if it is currently this characters' turn, or false otherwise
    protected bool isMyTurn;

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

    #endregion

    public int Health { get { return currentHealth; } }
    public int Willpower { get { return currentWillpower; } }
    public bool IsMyTurn { get { return isMyTurn; } set { isMyTurn = value; } }

    // Start is called before the first frame update
    void Start()
    {
        srCharacter = GetComponent<SpriteRenderer>();
        isMyTurn = false;
        currentHealth = maxHealth;
        currentWillpower = maxWillpower;
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
        if (target != null)
        {
            target.TakeDamage("physical", 4 + strength + Crit(), "physical");
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
    public void MagicAttack(Character target, string spellType, int spellCost)
    {
        if (target == null)
        {
            TurnEnd();
            return;
        }

        // If the character has enough willpower to cast the spell
        if (currentWillpower >= spellCost)
        {
            switch (spellType) {
                case "Heal":
                    Heal(5);
                    break;
                case "Buff":
                    Buff(/*Stats.Strength, 2*/);
                    break;
                case "Debuff":
                    // List<Stats> debuffStats = new List<Stats>() { Stats.Strength };
                    Buff(/*debuffStats, 2*/);
                    break;
                default:
                    target.TakeDamage("spell", 3 + resolve + Crit(), spellType);
                    break;
            }
        }
        else
        {
            // Implement functionality for what happens if you try to cast a spell but don't have enough willpower
        }

        TurnEnd();
    }

    /// <summary>
    /// Ends your turn, resets your emotion to none, and increases your willpower regen
    /// Ths function gets called when the player selects the "Rest" option in the battle menu
    /// </summary>
    public void Rest()
    {
        ChangeEmotion(Emotion.None);
        TurnEnd(regenWillpower + 3);
    }

    //Takes damage. First determines if a dodge occurs. If not, physical attacks get reduced
    //by the fortitude stat. If the character is weak to a certain spell type, they take increased damage.
    public void TakeDamage(string damageType, int damageAmount, string spellType)
    {
        if (Dodge()) { return; }

        if (damageType == "physical")
        {
            damageAmount = fortitude > damageAmount ? 0 : damageAmount - fortitude;
        }

        if (spellType == weakness)
        {
            damageAmount += 3;
        }

        currentHealth -=  damageAmount;
        currentHealth = currentHealth < 0 ? 0 : currentHealth;

        //For now, after any attack, there is a 20% chance of getting either Angry or Sad
        //This will be improved and changed at a later date
        //TODO: Improve the emotion system
        int emotionChance = Random.Range(0, 100);

        if (emotionChance >= 80)
        {
            if (emotionChance % 2 == 0)
            {
                ChangeEmotion(Emotion.Sadness);
            }
            else
            {
                ChangeEmotion(Emotion.Anger);
            }
        }
    }

    /// <summary>
    /// Heals the character by the specified amount
    /// </summary>
    /// <param name="healAmount">Amount of health to heal</param>
    public void Heal(int healAmount)
    {
        currentHealth += (healAmount + Crit());
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    //TODO: Implement spells that buff or debuff certain stats
    public void Buff(/*List<Stats> statList, List<Character> targetList*/)
    {

    }

    /// <summary>
    /// Determines whether a given attack is a crit
    /// </summary>
    /// <returns>True if the attack crits, false otherwise</returns>
    public int Crit()
    {
        int critChance = Random.Range(0, 100) + fortune;
        return critChance >= 90 ? 0 : 3 + fortune;
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
    /// Runs after a character takes their action for the turn, and causes willpower to be regenerated
    /// This function being called also tells the GameManager that this character's turn is complete
    /// </summary>
    public void TurnEnd()
    {
        isMyTurn = false;
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
    }

    /// <summary>
    /// Changes the character's emotion to the specified new one
    /// </summary>
    /// <param name="newEmotion">The new emotion to apply to the character</param>
    public void ChangeEmotion(Emotion newEmotion)
    {
        pastEmotion = currentEmotion;
        currentEmotion = newEmotion;

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
                }
                srCharacter.color = Color.white;
                break;
            case Emotion.Anger:
                strength += 5;
                resolve += 5;
                fortitude -= 5;
                fortune -= 5;

                if (fortitude < 0) { fortitude = 0; }
                if (fortune < 0) { fortune = 0; }

                srCharacter.color = Color.red;
                break;
            case Emotion.Sadness:
                fortitude += 5;
                fortune += 5;
                strength -= 5;
                resolve -= 5;

                if (strength < 0) { strength = 0; }
                if (resolve < 0) { resolve = 0; }

                srCharacter.color = Color.blue;
                break;
        }
    }
}
