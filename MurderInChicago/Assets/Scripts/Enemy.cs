using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System;
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
    /// <param name="spellType">The type of spell to cast, if that option is selected</param>
    /// <param name="spellCost">The willpower cost to cast a spell, if that option is selected</param>
    public void DetermineAction(PartyMember target, string spellType, int spellCost)
    {
        //Currently: 40% to do a physical attack, 40% to cast a spell, and 20% to rest
        int action = Random.Range(0, 100);

        if (action < 40) { PhysicalAttack(target); }
        else if (action > 40 && action < 80) { MagicAttack(target, spellType, spellCost); }
        else { Rest(); }
    }
}
