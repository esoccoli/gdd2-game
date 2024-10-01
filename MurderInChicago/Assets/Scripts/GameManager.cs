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
        
        partyMembers = new List<GameObject>();
        enemies = new List<GameObject>();

        /* 
         * IF THE NAME OF THE GAME OBJECT OF ANY CHARACTER IS UPDTAED, IT MUST BE UPDATED HERE AS WELL
         * 
         * There isnt any better way to get the game object of the characters other than their name bc there
         * isnt any attribute to indicate that a specific character is a player or an enemy
        */
        GameObject henry = GameObject.Find("henry");
        GameObject lucine = GameObject.Find("lucine");
        partyMembers.Add(henry);
        partyMembers.Add(lucine);

        /*
         * Once we have enemies, add them to the enemies list here using the same type of syntax as
         * used above for the party members
         */
        // GameObject enemy = GameObject.Find("<INSERT NAME OF ENEMY GAME OBJECT HERE>");
        // enemies.Add(enemy);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void NextTurn()
    {
        turnCounter++;
        //start next turn code goes here
        ManageTurns(partyMembers, enemies);
    }

    void ManageTurns(List<GameObject> partyMembers, List<GameObject> enemies)
    {
        // Loops through the list of party members and allows each one to take their turn
        for (int i = 0; i < partyMembers.Count; i++)
        {
            /* 
             * AwaitInputFromUI DOES NOT EXIST YET!!!!
             * 
             * There should be a function with this name inside both PartyMemberScript and EnemyScript
             * This function should wait until it gets input from the UI, and then call the appropriate function(s)
             * Ie: If the attack button is clicked, call the necessary functions to have that character attack a target specified by a parameter
            */
            partyMembers[i].AwaitInputFromUI();

            /*
             * Once the TurnEnd() function has been called for this party member, the loop should 
             * move to the next iteration, allowing the next party member to take their turn
             * If this was the last party member in the list, the loop will exit and then the program will move to the enemies
            */
        }

        // Loops through the list of enemies and allows each one to take their turn
        for (int i = 0; i < enemies.Count; i++)
        {
            int target = Random.Range(0, partyMembers.Count);

            // Feel free to change the spell type or cost, I just picked arbitrary values
            enemies[i].GetComponent<EnemyScript>().DetermineAction(partyMembers[i], "Buff", 3);
            /*
             * Once the TurnEnd() function has been called for this enemy, the loop should 
             * move to the next iteration, allowing the next enemy to take their turn
             * 
             * If this was the last enemy in the list, the loop will exit, and then the function also exits,
             * moving to the next round of the battle (one round of the battle consists of every party member getting a turn, and then every enemy is getting a turn
             */
        }
    }
}
