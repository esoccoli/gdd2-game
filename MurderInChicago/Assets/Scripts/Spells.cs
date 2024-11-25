using System.Collections.Generic;
using UnityEngine;

public class Spells : MonoBehaviour
{
    // This list serves as the central spell database that all characters refer back to. It exists only in the GameManager empty object.
    // A character will have their own list of spells that they know, and when they want to cast a spell, they will pass in the name of the
    // spell they know, and this script will return all the needed info. 

    // TLDR: This database is every single spell that exists in the game, that both party members and enemies use.
    List<Spell> spells = new List<Spell>();

    // Creates all of the spells
    void Start()
    {
        // Fire spells
        spells.Add(new Spell("Fire", "Fire", "Single", Emotion.Anger, "Deals a small amount of fire damage. Increases Anger.", 3, 1, 4, 15)); //Deals 3 base damage
        spells.Add(new Spell("Fireball", "Fire", "Single", Emotion.Anger, "Deals a large amount of fire damage. Increases Anger.", 6, 1, 6, 15)); //Deals 6 base damage
        spells.Add(new Spell("Soulfire", "Fire", "Multiple", Emotion.Anger, "Deals a small amount of fire damage to all enemies. Increases Anger.", 2, 1, 5, 20)); //Deals 2 base damage to all enemies

        // Ice spells
        spells.Add(new Spell("Ice", "Ice", "Single", Emotion.Sadness, "Deals a small amount of ice damage. Increases Sadness.", 3, 1, 4, 15)); //Deals 3 base damage
        spells.Add(new Spell("Ice Blast", "Ice", "Single", Emotion.Sadness, "Deals a large amount of ice damage. Increases Sadness.", 6, 1, 6, 15)); //Deals 6 base damage
        spells.Add(new Spell("Frost Winter", "Ice", "Multiple", Emotion.Sadness, "Deals a small amount of ice damage to all enemies. Increases Sadness.", 2, 1, 5, 20)); //Deals 2 base damage to all enemies

        // Curse spells
        spells.Add(new Spell("Curse", "Curse", "Single", Emotion.Fear, "Deals a small amount of curse damage. Increases Fear.", 5, 1, 3, 35)); //Deals 5 base damage
        spells.Add(new Spell("Cursebind", "Curse", "Single", Emotion.Fear, "Deals a large amount of curse damage. Increases Fear.", 8, 1, 4, 35)); //Deals 8 base damage
        spells.Add(new Spell("Dreadcall", "Curse", "Multiple", Emotion.Fear, "Deals a small amount of curse damage to all enemies. Increases Fear.", 4, 1, 2, 40)); //Deals 4 base damage to all enemies

        // Poison spells
        spells.Add(new Spell("Poison", "Poison", "Single", Emotion.Disgust, "Deals a small amount of poison damage. Increases Disgust.", 5, 1, 3, 35)); //Deals 5 base damage
        spells.Add(new Spell("Deadly Poison", "Poison", "Single", Emotion.Disgust, "Deals a large amount of poison damage. Increases Disgust.", 8, 1, 4, 35)); //Deals 8 base damage
        spells.Add(new Spell("Putrescence", "Poison", "Multiple", Emotion.Disgust, "Deals a small amount of poison damage to all enemies. Increases Disgust.", 4, 1, 2, 40)); //Deals 4 base damage tp all enemies

        // Heal spells
        spells.Add(new Spell("Heal", "Heal", "Single", Emotion.Happiness, "Heals a small amount. Increases Happiness.", 4, 1, 4, 20)); //Heals 4 base damage
        spells.Add(new Spell("Strong Heal", "Heal", "Single", Emotion.Happiness, "Heals a large amount. Increases Happiness.", 7, 1, 5, 20)); //Heals 7 base damage
        spells.Add(new Spell("Mercy", "Heal", "Multiple", Emotion.Happiness, "Heals a small amount to all allies. Increases Happiness.", 3, 1, 4, 25)); //Heals 3 base damage to all allies

        // Buff spells
        // { vitality, strength, resolve, fortitude, fortune }
        // Note: Vitality currently does not get increased or decreased, even by Supreme Being or Annihilation.
        // This is because weird things start happening when you temporarily increase or decrease your max health 
        spells.Add(new Spell("Strengthen", "Buff", "Single", Emotion.Happiness, "Boosts your physical attack damage.", 0, 4, 3, 15, new int[] { 0, 4, 0, 0, 0 })); //Boosts Strength by 4
        spells.Add(new Spell("Invigorate", "Buff", "Single", Emotion.Happiness, "Boosts your magical attack damage.", 0, 4, 3, 15, new int[] { 0, 0, 4, 0, 0 })); //Boosts Resolve by 4
        spells.Add(new Spell("Galvanize", "Buff", "Single", Emotion.Happiness, "Boosts your defense.", 0, 4, 3, 15, new int[] { 0, 0, 0, 4, 0 })); //Boosts Fortitude by 4
        spells.Add(new Spell("Resolution", "Buff", "Single", Emotion.Happiness, "Boosts your chances at crits and dodges.", 0, 4, 3, 15, new int[] { 0, 0, 0, 0, 4 })); //Boosts Fortune by 4
        spells.Add(new Spell("Rally", "Buff", "Multiple", Emotion.Happiness, "Boosts physical attacks of all allies.", 0, 2, 4, 20, new int[] { 0, 2, 0, 0, 0 })); //Boosts Strength by 2 to all allies
        spells.Add(new Spell("Energize", "Buff", "Multiple", Emotion.Happiness, "Boosts defenses of all allies.", 0, 2, 4, 20, new int[] { 0, 0, 0, 2, 0 })); //Boosts Fortitude by 2 to all allies
        spells.Add(new Spell("Supreme Being", "Buff", "Single", Emotion.Happiness, "Boosts all stats.", 0, 3, 7, 30, new int[] { 0, 2, 2, 2, 2 })); //Boosts all stats by 2 

        // Debuff spells
        spells.Add(new Spell("Debilitate", "Debuff", "Single", Emotion.Sadness, "Nerfs enemy physical attack damage.", 0, 4, 2, 15, new int[] { 0, 2, 0, 0, 0 })); //Nerds Strength by 2
        spells.Add(new Spell("Incapacitate", "Debuff", "Single", Emotion.Sadness, "Nerfs enemy magical attack damage.", 0, 4, 2, 15, new int[] { 0, 0, 2, 0, 0 })); //Nerds Resolve by 2
        spells.Add(new Spell("Corruption", "Debuff", "Single", Emotion.Sadness, "Nerfs enemy defenses.", 0, 4, 2, 15, new int[] { 0, 0, 0, 2, 0 })); //Nerds Fortitude by 2
        spells.Add(new Spell("Misfortune", "Debuff", "Single", Emotion.Sadness, "Nerfs enemy chances at crits and dodges.", 0, 4, 2, 15, new int[] { 0, 0, 0, 0, 2 })); //Nerds Fortune by 2
        spells.Add(new Spell("Devastation", "Debuff", "Multiple", Emotion.Sadness, "Nerfs physical attacks of all enemies.", 0, 2, 3, 20, new int[] { 0, 1, 0, 0, 0 })); //Nerds Strength by 1 to all enemies
        spells.Add(new Spell("Wither", "Debuff", "Multiple", Emotion.Sadness, "Nerfs defenses of all enemies.", 0, 2, 3, 20, new int[] { 0, 0, 0, 1, 0 })); //Nerds Fortitude by 1 to all enemies
        spells.Add(new Spell("Annihilation", "Debuff", "Single", Emotion.Sadness, "Nerfs all stats of an enemy.", 0, 3, 6, 30, new int[] { 0, 2, 2, 2, 2 })); //Nerfs all stats by 2 

        // Emotion spells
        spells.Add(new Spell("Taunt", "Emotion", "Single", Emotion.Anger, "Inflicts Anger, which boosts all damage but nerfs defense and crit chances.", 0, 1, 1, 50)); //Increases Anger by a lot
        spells.Add(new Spell("Melancholy", "Emotion", "Single", Emotion.Sadness, "Inflicts Sadness, which boosts defense and crit chances but nerfs all damage.", 0, 1, 1, 50)); //Increases Sadness by a lot
        spells.Add(new Spell("Joy", "Emotion", "Single", Emotion.Happiness, "Inflicts Happiness, which boosts buff and heal spells but nerfs health and physical attack damage.", 0, 1, 1, 50)); //Increases Happiness by a lot
        spells.Add(new Spell("Despair", "Emotion", "Single", Emotion.Fear, "Inflicts Fear, which prevents Resting and makes all spells cost more Willpower.", 0, 1, 1, 50)); //Increases Fear by a lot
        spells.Add(new Spell("Repugnance", "Emotion", "Single", Emotion.Disgust, "Inflicts Disgust, which prevents Resting and negates heals and buff spells.", 0, 1, 1, 50)); //Increases Disgust by a lot
    }

    // When a Character chooses a spell from their list of known spells, this function will search the spell database and
    // return all of the info needed for that spell. If the spell does not exist, it returns null
    public Spell GetSpell(string name)
    {
        for (int i = 0; i < spells.Count; i++)
        {
            if (spells[i].name == name)
            {
                return spells[i];
            }
        }

        return null;
    }

    /// <summary>
    /// A spell contains the following:
    ///     Name: The name of the spell
    ///     Type: The damage/effect type of the spell. Fire, Ice, Curse, Poison, Heal, Buff, Debuff, or Emotion
    ///     Target: Whether this spell targets only a Single character, or Multiple
    ///     Emotion: What Emotion this spell increases when cast
    ///     Description: Info that will appear in the spell menu
    ///     Damage Amount: How much damage or how much healing this spells this spell does
    ///     Turn Count: How long a spell will last (1 means that does not last several turns)
    ///     Willpower Cost: How much Willpower it takes to cast this spell
    ///     Emotion Points: An Emotion triggers when it gets 100 Emotion Points. This shows how much EP this spell gives
    ///     Stat Changes: For only Buffs and Debuffs, this int array is what determines what stats get affected, and by how much.
    /// </summary>
    public class Spell
    {
        public string name;
        public string type;
        public string target;
        public Emotion emotion;
        public string description;
        public int damageAmount;
        public int turnCount;
        public int willpowerCost;
        public int emotionPoints;
        public int[] statChanges = new int[5];

        // For all spells that are not Buffs or Debuffs, as they do not use the Stat Changes int array
        public Spell(string name, string type, string target, Emotion emotion, string description, int damageAmount, int turnCount, int willpowerCost, int emotionPoints) 
        {
            this.name = name;
            this.type = type;
            this.target = target;
            this.emotion = emotion;
            this.description = description;
            this.damageAmount = damageAmount;
            this.turnCount = turnCount;
            this.willpowerCost = willpowerCost;
            this.emotionPoints = emotionPoints;
            this.statChanges = new int[5];
        }

        // Buffs and Debuffs additionally have info on which stats to modify, and by how much.
        // Guide: { vitality, strength, resolve, fortitude, fortune }
        // Note: Nothing so far actually changes your vitality
        public Spell(string name, string type, string target, Emotion emotion, string description, int damageAmount, int turnCount, int willpowerCost, int emotionPoints, int[] statChanges)
        {
            this.name = name;
            this.type = type;
            this.target = target;
            this.emotion = emotion;
            this.description = description;
            this.damageAmount = damageAmount;
            this.turnCount = turnCount;
            this.willpowerCost = willpowerCost;
            this.emotionPoints = emotionPoints;
            this.statChanges = statChanges;
        } 
    }
}
