using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Enemy : Character
{
    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Randomly selects one of the possible battle options and has the enemy do that
    /// </summary>
    /// <param name="target">The target of the action</param>
    /// <param name="spellName">The name of the spell that this enemy is casting</param>
    public void DetermineAction(List<PartyMember> targetList, string spellName)
    {
        //Currently: 40% to do a physical attack, 40% to cast a spell, and 20% to rest
        int action = Random.Range(0, 100);

        Character target = targetList[Random.Range(0, targetList.Count - 1)];
        List<Character> characterList;
        if (spellName != "None" && GetSpellInfo(spellName)[1] == "Multiple")
            characterList = targetList.ConvertAll(target => (Character)target);
        else characterList = new List<Character>() { target };

        if (action < 30) { StartCoroutine(PhysicalAttack(target)); }
        else if (action > 30 && action < 80) {
            if (spellName == "None") StartCoroutine(PhysicalAttack(target));
            else if (spellName == "Heal") StartCoroutine(MagicAttack(new List<Character>() { this }, spellName));
            else StartCoroutine(MagicAttack(characterList, spellName)); 
        }
        else { Rest(); }
    }

    public string GetRandomSpell()
    {
        if (spellList.Count == 0) return "None";
        string spell = spellList[Random.Range(0, spellList.Count)];
        //Ensure the user has enough willpower to cast the spell
        while (currentWillpower < GetSpellStats(spell)[1])
        {
            spell = spellList[Random.Range(0, spellList.Count)];
        }
        return spell;
    }
}
