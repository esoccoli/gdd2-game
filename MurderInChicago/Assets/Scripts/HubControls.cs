using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubControls : MonoBehaviour
{
    Vector3 objectPosition;
    Vector3 direction = Vector3.zero;
    Vector3 velocity = Vector3.zero;

    [SerializeField]
    float speed = 10.0f;


    // Start is called before the first frame update
    void Start()
    {
        objectPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        velocity = direction * speed * Time.deltaTime;

        objectPosition += velocity;



        transform.position = objectPosition;
    }

    /// <summary>
    /// sets the direction that the sprite faces based on what direction they are going in
    /// </summary>
    /// <param name="newDirection"></param>
    public void SetMoveDirection(Vector3 newDirection)
    {
        direction = newDirection.normalized;

        if (direction != Vector3.zero)
        {
            //TODO: Add rotating the sprite based on where it is moving

        }
    }
}
