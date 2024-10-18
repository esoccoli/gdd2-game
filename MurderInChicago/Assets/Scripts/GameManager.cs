using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.Presets;
//using UnityEditor.Search;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int turnCounter;

    // Party members in current party
    [SerializeField]
    List<PartyMember> partyMembers;

    [SerializeField]
    List<Enemy> enemies;

    // A queue to manage turn order
    Queue<IEnumerator> turnQueue = new Queue<IEnumerator>();

    public List<PartyMember> PartyMembers { get { return partyMembers; } set { partyMembers = value; } }
    public List<Enemy> Enemies { get { return enemies; } set { enemies = value; } }


    // Start is called before the first frame update
    void Start()
    {
        turnCounter = 0;
        PrepareTurnQueue();
        StartNextTurn();
    }

    //Prepare the turn queue for all party members and enemies
    void PrepareTurnQueue()
    {
        // Clear the current queue to prevent stacking turns
        turnQueue.Clear();

        // Add party members to the turn queue
        foreach (PartyMember member in partyMembers)
        {
            turnQueue.Enqueue(PartyMemberTurn(member));
        }

        // Add enemies to the turn queue
        foreach (Enemy enemy in enemies)
        {
            turnQueue.Enqueue(EnemyTurn(enemy));
        }
    }

    // Process the next turn in the queue
    void StartNextTurn()
    {
        if (turnQueue.Count > 0) { StartCoroutine(turnQueue.Dequeue()); }
        else
        {
            // All turns are done, start the next round
            PrepareTurnQueue();
            StartNextTurn();
        }
    }

    // Coroutine to handle a party member's turn
    IEnumerator PartyMemberTurn(PartyMember member)
    {
        member.IsMyTurn = true;

        // Wait until the player completes their action
        yield return StartCoroutine(member.AwaitInputFromUI());

        // Mark this character's turn as complete and move to the next one
        StartNextTurn();
    }

    // Coroutine to handle an enemy's turn
    IEnumerator EnemyTurn(Enemy enemy)
    {
        enemy.IsMyTurn = true;

        int target = Random.Range(0, partyMembers.Count);

        // Simulate enemy determining action
        enemy.DetermineAction(partyMembers[target], "Soulfire");

        // Add a slight delay to simulate enemy thinking/action
        yield return new WaitForSeconds(1.0f);

        // Mark the enemy's turn as complete and move to the next one
        StartNextTurn();
    }

    //Update is called once per frame
    void Update()
    {
        
    }
}
