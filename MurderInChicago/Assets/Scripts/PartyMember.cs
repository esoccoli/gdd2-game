using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
using System;
using Random = UnityEngine.Random;
using UnityEngine.TextCore.Text;

public class PartyMember : Character
{
    /// <summary>
    /// Allows the player to select an item from their inventory and use it
    /// Only players can use items
    /// This function will get called when the player selects the "Use Item" option in the battle menu
    /// </summary>
    public void UseItem()
    {

    }

    /// <summary>
    /// Waits for input from the UI and takes the appropriate action based on the specific input
    /// </summary>
    /// <returns></returns>
    public IEnumerator AwaitInputFromUI()
    {
        //yield return new WaitForSeconds(2.0f);
        while (IsMyTurn)
        {
            yield return null;
        }
    }
}
