using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

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
