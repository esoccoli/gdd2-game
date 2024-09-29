using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Notes from Igor about how to connect the PartyMembers and Enemy Scripts
    //So what I've done is create the end points for both all of the players and enemies
    //What you need to do here is manage their connections.
    //There are four actions that can be done:
    //Attack
    //Cast Spell
    //Use Item (not implemented, not needed for now, party member only)
    //Rest
    //From GameManager, you'll need to pass in certain values like the GameObject of the target,
    //spell types and costs, etc. 
    //After either a PartyMember or Enemy calls TurnEnd(), then the GameManager needs to either
    //move onto the next member of their team, or if all on one side have called TurnEnd(), then
    //GameManager needs to swap to the other team.
    //Enemies determine their attacks with the DetermineAction() command. Party Members use a menu.
    //Ideally, also build out a system for keeping track of turns, so that in the future we can have
    //things like debuffs that expire after two turns for example

    //Good luck! Let me know if you have any questions about my PartyMember or Enemy scripts

    int turnCounter;

    //party members in current party
    List<GameObject> partyMembers;


    List<GameObject> enemies;

    // Start is called before the first frame update
    void Start()
    {
        turnCounter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void NextTurn()
    {
        turnCounter++;
        //start next turn code goes here
    }
}
