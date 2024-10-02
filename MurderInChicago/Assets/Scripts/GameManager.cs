using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Presets;
using UnityEditor.Search;
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
    [SerializeField]
    List<PartyMemberScript> partyMembers;

    [SerializeField]
    List<EnemyScript> enemies;

    //A queue to manage turn order
    Queue<IEnumerator> turnQueue = new Queue<IEnumerator>();

    public List<PartyMemberScript> PartyMembers { get { return partyMembers; } set { partyMembers = value; } }
    public List<EnemyScript> Enemies { get { return enemies; } set { enemies = value; } }


    // Start is called before the first frame update
    void Start()
    {
        turnCounter = 0;

        PrepareTurnQueue();

        StartNextTurn();
        
        //partyMembers = new List<GameObject>();
        //enemies = new List<GameObject>();

        /* 
         * IF THE NAME OF THE GAME OBJECT OF ANY CHARACTER IS UPDTAED, IT MUST BE UPDATED HERE AS WELL
         * 
         * There isnt any better way to get the game object of the characters other than their name bc there
         * isnt any attribute to indicate that a specific character is a player or an enemy
        */
        //GameObject henry = GameObject.Find("henry");
        //GameObject lucine = GameObject.Find("lucine");

        //i commented this code out because you can just use serialize field and use the scripts 

        //partyMembers.Add(henry);
        //partyMembers.Add(lucine);

        /*
         * Once we have enemies, add them to the enemies list here using the same type of syntax as
         * used above for the party members
         */
        //GameObject enemy = GameObject.Find("E1");
        //enemies.Add(enemy);
    }

    //Prepare the turn queue for all party members and enemies
    void PrepareTurnQueue()
    {
        //Clear the current queue to prevent stacking turns
        turnQueue.Clear();

        //Add party members to the turn queue
        foreach (var member in partyMembers)
        {
            turnQueue.Enqueue(PartyMemberTurn(member));
        }

        //Add enemies to the turn queue
        foreach (var enemy in enemies)
        {
            turnQueue.Enqueue(EnemyTurn(enemy));
        }
    }

    //Process the next turn in the queue
    void StartNextTurn()
    {
        if (turnQueue.Count > 0)
        {
            StartCoroutine(turnQueue.Dequeue());
        }
        else
        {
            //All turns are done, start the next round
            PrepareTurnQueue();
            StartNextTurn();
        }
    }

    //Coroutine to handle a party member's turn
    IEnumerator PartyMemberTurn(PartyMemberScript member)
    {
        member.IsMyTurn = true;

        //Wait until the player completes their action
        yield return StartCoroutine(member.AwaitInputFromUI());

        member.TurnEnd();
        member.IsMyTurn = false; // Mark the member's turn as complete
        StartNextTurn(); //Proceed to the next turn after this one ends
    }

    //Coroutine to handle an enemy's turn
    IEnumerator EnemyTurn(EnemyScript enemy)
    {
        enemy.IsMyTurn = true;

        int target = Random.Range(0, partyMembers.Count);

        //Simulate enemy determining action
        enemy.DetermineAction(partyMembers[target], "Heal", 3);

        //Add a slight delay to simulate enemy thinking/action
        yield return new WaitForSeconds(1.0f);

        enemy.TurnEnd();
        enemy.IsMyTurn = false; // Mark the enemy's turn as complete
        StartNextTurn(); //Proceed to the next turn after this one ends
    }

    //Update is called once per frame
    void Update()
    {
        
    }
}
