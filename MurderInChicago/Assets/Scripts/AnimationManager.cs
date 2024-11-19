using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField]
    string[] sourceNames;

    [SerializeField]
    GameObject[] sourceObjects;

    Dictionary<string, GameObject> objectLibrary = new Dictionary<string, GameObject>();

    Dictionary<string, GameObject> currentSprites = new Dictionary<string, GameObject>();

    private void Start()
    {
        for (int i = 0; i < sourceNames.Length; i++)
        {
            objectLibrary.Add(sourceNames[i], sourceObjects[i]);
        }
    }

    /// <summary>
    /// Adds an animated sprite to the 
    /// </summary>
    /// <param name="spriteType"></param>
    /// <param name="name"></param>
    /// <param name="position"></param>
    public void Add(string spriteType, string name, Vector3 position, bool flip)
    {
        GameObject animSprite = Instantiate(objectLibrary[spriteType]);
        animSprite.transform.position = position;
        if (flip)
            animSprite.transform.localScale = Vector3.Scale(animSprite.transform.localScale, new Vector3(-1, 1, 1));
        currentSprites.Add(name + spriteType, animSprite);
    }

    public void Delete(string name)
    {
        if (currentSprites.ContainsKey(name))
        {
            Destroy(currentSprites[name]);
            currentSprites.Remove(name);
        }
    }
}
