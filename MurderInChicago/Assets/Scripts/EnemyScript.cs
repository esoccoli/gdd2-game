using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System;
using Random = UnityEngine.Random;

public class EnemyScript : MonoBehaviour
{
    [SerializeField]
    GameObject enemy;

    SpriteRenderer sr_enemy;


    //used for UI so game knows when to disable the button
    bool isMyTurn;

    //STATS

    int current_health;

    [SerializeField]
    int max_health; //Base health, gets increased by vitality

    int current_willpower;

    [SerializeField]
    int max_willpower; //Base willpower, gets increased by resolve

    [SerializeField]
    int regen_willpower; //How much willpower they recover after each turn. Increased by resting

    string current_emotion = "None";
    string past_emotion = "None"; //This is to ensure we return any stats back to their original values when they lose an emotion

    [SerializeField]
    int vitality; //Increases health

    [SerializeField]
    int strength; //Increases physical attack damage

    [SerializeField]
    int resolve; //Increases max willpower and magical damage

    [SerializeField]
    int fortitude; //Increases defense

    [SerializeField]
    int fortune; //Increases crit damage chance and dodge chance

    [SerializeField]
    string weakness; //Determines what type of damage they are weak too


    public int Health { get { return current_health; } }
    public int Current_Willpower { get { return current_willpower; } }


    public bool IsMyTurn { get { return isMyTurn; } set { isMyTurn = value; } }


    // Start is called before the first frame update
    void Start()
    {
        sr_enemy = GetComponent<SpriteRenderer>();
        isMyTurn = false;
        current_health = max_health;
        current_willpower = max_willpower;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //One of the main differences between the PlayerMember script
    //and the Enemy Script is this function here that determines
    //enemy moves (the enemy AI)
    //Enemies can only do physical attacks, cast spells, and rest
    //Everything else is the same
    //Currently, it will just randomly choose an option.
    //TODO: Improve AI
    public void DetermineAction(PartyMemberScript target, string spellType, int spellCost)
    {
        //Currently: 40% to do a physical attack, 40% to cast a spell, and 20% to cast a spell
        int action = Random.Range(0, 100);
        if (action < 40)
        {
            PhysicalAttack(target);
        }
        else if (action < 80)
        {
            MagicAttack(target, spellType, spellCost);
        }
        else
        {
            Rest();
        }
    }

    //Deals physical type damage to the target
    //Because these are the scripts for the enemy characters,
    //they will get the Party Member Scripts of the target.
    //Gets called when the enemy selects "Attack" option
    public void PhysicalAttack(PartyMemberScript targetScript)
    {
        //PartyMemberScript targetScript = target.GetComponent<PartyMemberScript>();
        if (targetScript != null)
        {
            targetScript.TakeDamage("physical", 4 + strength + Crit(), "physical");
        }
        TurnEnd();
    }

    //Deals magical type damage to the target. Magic spells can have multiple different subtypes,
    //such as fire, ice, electricity, etc
    //Gets called when the enemy selects "Use Spell" option
    public void MagicAttack(PartyMemberScript target, string spellType, int spellCost)
    {
        PartyMemberScript targetScript = target.GetComponent<PartyMemberScript>();
        if (targetScript != null)
        {
            if (current_willpower - spellCost >= 0)
            {
                if (spellType == "Heal")
                {
                    Heal(5);
                }
                else if (spellType == "Buff")
                {
                    BuffDebuff("Buff");
                }
                else if (spellType == "Debuff")
                {
                    BuffDebuff("Debuff");
                }
                else
                {
                    targetScript.TakeDamage("spell", 3 + resolve + Crit(), spellType);
                }

                TurnEnd();
            }
            else
            {
                // Implement what happens when you don't have enough Willpower to cast the spell
            }
        }
    }


    //Resting ends your turn, resets your emotion, and increases your willpower regen
    //Gets called when the enemy selects the "Rest" option
    public void Rest()
    {
        ChangeEmotion("None");
        TurnEnd(regen_willpower + 3);
    }


    //Takes damage. First determines if a dodge occurs. If not, physical attacks get reduced
    //by the fortitude stat. If the character is weak to a certain spell type, they take increased damage.
    public void TakeDamage(string damageType, int damageAmount, string spellType)
    {
        if (Dodge() == true)
        {
            return;
        }

        if (damageType == "physical")
        {
            if (damageAmount - fortitude < 0)
            {
                damageAmount = 0;
            }
            else
            {
                damageAmount = damageAmount - fortitude;
            }
        }

        if (spellType == weakness)
        {
            damageAmount += 3;
        }

        current_health = current_health - damageAmount;

        if (current_health < 0)
        {
            current_health = 0;
        }

        //For now, after any attack, there is a 20% chance of getting either Angry or Sad
        //This will be improved and changed at a later date
        //TODO: Improve the emotion system
        int emotionChance = Random.Range(0, 100);
        if (emotionChance >= 80)
        {
            if (emotionChance % 2 == 0)
            {
                ChangeEmotion("Sad");
            }
            else
            {
                ChangeEmotion("Angry");
            }
        }
    }

    //Heals the character
    public void Heal(int healAmount)
    {
        current_health += (healAmount + Crit());
        if (current_health > max_health)
        {
            current_health = max_health;
        }
    }

    //TODO: Implement spells that buff or debuff certain stats
    public void BuffDebuff(string spellType)
    {

    }

    //Determines if a crit happens for either an attack or healing. Either way,
    //increases it's effect. Current it's a 25% base chance + fortune
    public int Crit()
    {
        int critDamage = 0;

        int critChance = Random.Range(0, 100) + fortune;

        if (critChance >= 75)
        {
            critDamage = 3 + fortune;
        }

        return critDamage;
    }

    //Determines if a dodge happens, which nullifies all damage. 
    //TODO: Implement true strike attacks/spells that deal damage regardless of dodge
    public bool Dodge()
    {
        bool dodge = false;

        int dodgeChance = Random.Range(0, 100) + fortune;

        if (dodgeChance >= 75)
        {
            dodge = true;
        }

        return dodge;
    }

    //Runs after you either attack, cast spell, use item, or resting. 
    //Willpower regen defaults to your base regen amount unless you rest
    //This is done by having the first function overload the TurnEnd
    //And pass in regen_willpower if TurnEnd is called from anywhere but Rest()
    public void TurnEnd()
    {
        isMyTurn = false;
        TurnEnd(regen_willpower);
    }

    public void TurnEnd(int willpowerAmount)
    {
        current_willpower += willpowerAmount;
        if (current_willpower > max_willpower)
        {
            current_willpower = max_willpower;
        }
    }

    //Changes your emotion to either Angry, Sad, or None
    //Angry: Increases your physical attack strength and resolve, decreases fortitude and fortune. Tint color: Red
    //Sad: Increases your fortitude and fortune, decreases strength and resolve. Tint color: Blue
    //None: Resets your stats back to their original values. Tint color: None/white
    //TODO: Implement Happy, Fear, and Disgust
    public void ChangeEmotion(string newEmotion)
    {
        past_emotion = current_emotion;
        current_emotion = newEmotion;

        switch (current_emotion)
        {
            case "None":
                switch (past_emotion)
                {
                    case "Angry": strength -= 5; resolve -= 5; fortitude += 5; fortune += 5; break;
                    case "Sad": fortitude -= 5; fortune -= 5; strength += 5; resolve += 5; break;
                }
                sr_enemy.color = Color.white;
                break;
            case "Angry":
                strength += 5;
                resolve += 5;
                fortitude -= 5;
                fortune -= 5;

                if (fortitude < 0) { fortitude = 0; }

                if (fortune < 0) { fortune = 0; }

                sr_enemy.color = Color.red;
                break;
            case "Sad":
                fortitude += 5;
                fortune += 5;
                strength -= 5;
                resolve -= 5;

                if (strength < 0) { strength = 0; }

                if (resolve < 0) { resolve = 0; }

                sr_enemy.color = Color.blue;
                break;
        }
    }
}
