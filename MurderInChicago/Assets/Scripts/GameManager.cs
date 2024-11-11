using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
//using UnityEditor.Presets;
//using UnityEditor.Search;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : MonoBehaviour
{
    int turnCounter;

    // Party members in current party
    [SerializeField]
    List<PartyMember> partyMembers;

    [SerializeField]
    List<Enemy> enemies;

    [SerializeField]
    GameObject cursor;

    [SerializeField]
    SpriteRenderer turnIndicatorFront;

    [SerializeField]
    SpriteRenderer turnIndicatorBack;

    Vector3 mousePos;

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

    /// <summary>
    /// Prepare the turn queue for all party members and enemies
    /// </summary>
    void PrepareTurnQueue()
    {
        // Clear the current queue to prevent stacking turns
        turnQueue.Clear();

        // Add party members to the turn queue
        foreach (PartyMember member in partyMembers)
        {
            if (!member.IsAlive) continue;
            turnQueue.Enqueue(PartyMemberTurn(member));
        }

        // Add enemies to the turn queue
        foreach (Enemy enemy in enemies)
        {
            if (!enemy.IsAlive) continue;
            turnQueue.Enqueue(EnemyTurn(enemy));
        }
    }

    /// <summary>
    /// Process the next turn in the queue
    /// </summary>
    void StartNextTurn()
    {
        if (turnQueue.Count > 0){ StartCoroutine(turnQueue.Dequeue()); }
        else
        {
            // All turns are done, start the next round
            PrepareTurnQueue();
            StartNextTurn();
        }
    }

    /// <summary>
    /// Coroutine to handle a party member's turn
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    IEnumerator PartyMemberTurn(PartyMember member)
    {
        if (member.IsAlive)
        {
            member.IsMyTurn = true;
            member.IsUsingSpell = false;

            Vector3 newPositionFront = member.transform.position + new Vector3(0, -0.8f, -5);
            Vector3 newPositionBack = member.transform.position + new Vector3(0, -0.8f, 5);
            turnIndicatorFront.transform.position = newPositionFront;
            turnIndicatorBack.transform.position = newPositionBack;

            // Wait until the player completes their action
            yield return StartCoroutine(member.AwaitInputFromUI());
        }

        // Mark this character's turn as complete and move to the next one
        StartNextTurn();
    }

    /// <summary>
    /// Coroutine to handle an enemy's turn
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    IEnumerator EnemyTurn(Enemy enemy)
    {
        if (enemy.IsAlive)
        {
            enemy.IsMyTurn = true;

            Vector3 newPositionFront = enemy.transform.position + new Vector3(0, -1f, -5);
            Vector3 newPositionBack = enemy.transform.position + new Vector3(0, -1f, 5);
            turnIndicatorFront.transform.position = newPositionFront;
            turnIndicatorBack.transform.position = newPositionBack;

            int target = Random.Range(0, partyMembers.Count - 1);

            List<PartyMember> targetList = new();
            foreach (PartyMember partyMember in partyMembers) { 
                if (partyMember.IsAlive) targetList.Add(partyMember);
            }

            // Simulate enemy determining action
            enemy.DetermineAction(targetList, enemy.GetRandomSpell());

            // Add a slight delay to simulate enemy thinking/action
            yield return new WaitForSeconds(2.0f);

        }
        // Mark the enemy's turn as complete and move to the next one
        StartNextTurn();
    }

    //Update is called once per frame
    void Update()
    {
        mousePos = Input.mousePosition;
        mousePos.z = 10.0f;
        cursor.transform.position = Camera.main.ScreenToWorldPoint(mousePos);

    }
}
