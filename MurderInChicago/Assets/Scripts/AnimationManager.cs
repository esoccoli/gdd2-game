using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField]
    string[] sourceNames;

    [SerializeField]
    GameObject[] sourceObjects;

    // A library of animated sprites. Animations are instantiated from this library.
    Dictionary<string, GameObject> spriteLibrary = new Dictionary<string, GameObject>();

    // A list of currently looping sprites, used primarily for emotions.
    Dictionary<string, GameObject> currentSprites = new Dictionary<string, GameObject>();

    private void Start()
    {
        for (int i = 0; i < sourceNames.Length; i++)
        {
            spriteLibrary.Add(sourceNames[i], sourceObjects[i]);
        }
    }

    /// <summary>
    /// Adds an animated sprite if to the dictonary.
    /// </summary>
    /// <param name="spriteType"></param>
    /// <param name="name">Name of the character this sprite belongs to</param>
    /// <param name="position"></param>
    public void AddLoop(string spriteType, string name, Vector3 position, bool flip)
    {
        GameObject animSprite = Instantiate(spriteLibrary[spriteType]);
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

    /// <summary>
    /// Animates a sprite, then deletes it.
    /// </summary>
    /// <param name="spriteType"></param>
    /// <param name="position"></param>
    public IEnumerator AnimateSprite(string spriteType, Vector3 position)
    {
        GameObject animSprite = Instantiate(spriteLibrary[spriteType]);
        animSprite.transform.position = position;
        yield return new WaitForSeconds(animSprite.GetComponent<Animation>().clip.length);
        Destroy(animSprite);
    }
}
