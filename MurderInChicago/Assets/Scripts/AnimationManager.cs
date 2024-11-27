using System;
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

    bool isActive;
    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }

    // A library of animated sprites. Animations are instantiated from this library.
    Dictionary<string, GameObject> spriteLibrary = new Dictionary<string, GameObject>();

    // A list of currently looping sprites, used primarily for emotions.
    Dictionary<string, GameObject> currentSprites = new Dictionary<string, GameObject>();

    // A "queue" that holds temporary animated sprites to be played, used for multi-target spells.
    List<Tuple<string, Vector3>> animationQueue = new List<Tuple<string, Vector3>>();

    public int animQueueCount
    {
        get { return animationQueue.Count; }
    }
    
    private void Start()
    {
        for (int i = 0; i < sourceNames.Length; i++)
        {
            spriteLibrary.Add(sourceNames[i], sourceObjects[i]);
        }
    }

    /// <summary>
    /// Adds a looping animated sprite to the dictonary.
    /// </summary>
    /// <param name="spriteType">The name of the sprite to animate.</param>
    /// <param name="name">Name of the character this sprite belongs to.</param>
    /// <param name="position"></param>
    public void AddLoop(string spriteType, string name, Vector3 position, bool flip)
    {
        GameObject animSprite = Instantiate(spriteLibrary[spriteType]);
        animSprite.transform.position = position;
        if (flip)
            animSprite.transform.localScale = Vector3.Scale(animSprite.transform.localScale, new Vector3(-1, 1, 1));
        currentSprites.Add(name + spriteType, animSprite);
    }

    /// <summary>
    /// Deletes a looping sprite in the dictionary.
    /// </summary>
    /// <param name="name">The name of the sprite to delete.</param>
    public void Delete(string name)
    {
        if (currentSprites.ContainsKey(name))
        {
            Destroy(currentSprites[name]);
            currentSprites.Remove(name);
        }
    }

    /// <summary>
    /// Determines the type of buff or nerf passed in. 
    /// </summary>
    /// <param name="buffArray">The array of stat changes.</param>
    /// <param name="isDebuff">Whether this is a buff or a debuff.</param>
    /// <returns>The stat the buff raises, as a string.</returns>
    public string DetermineBuff(int[] buffArray, bool isDebuff)
    {
        string suffix = isDebuff ? "Nerf" : "Buff"; 
        int buffIndex = 0;
        int buffCount = 0;
        for (int i = 0; i < buffArray.Length; i++)
        {
            if (buffArray[i] != 0)
            {
                buffIndex = i;
                buffCount++;
            }
        }
        if (buffCount > 1) return "Multi" + suffix;
        switch (buffIndex)
        {
            case 1:
                return "Strength" + suffix;
            case 2:
                return "Resolve" + suffix;
            case 3:
                return "Fortitude" + suffix;
            case 4:
                return "Fortune" + suffix; 
            default:
                return "";
        }
    }

    /// <summary>
    /// Animates Multi-Target spell sprites using the queue methods.
    /// </summary>
    /// <param name="spellName">Name of the spell to animate.</param>
    /// <param name="positions">All positions to animate the sprite on.</param>
    public IEnumerator AnimateMultiTarget(string spellName, Vector3[] positions)
    {
        switch (spellName) {
            case "Soulfire":
                spellName = "Fire";
                goto default;
            case "Dreadcall":
                spellName = "Curse";
                goto default;
            default:
                foreach (Vector3 position in positions)
                {
                    Enqueue(spellName, position);
                }
                yield return PlayQueue();
                break;
        }
    }

    /// <summary>
    /// Adds the specified animation to the "queue".
    /// </summary>
    public void Enqueue(string animName, Vector3 position)
    {
        animationQueue.Add(new Tuple<string, Vector3>(animName, position));
    }

    /// <summary>
    /// Plays every animation in the queue at once.
    /// </summary>
    public IEnumerator PlayQueue()
    {
        if (animationQueue.Count == 0)
        {
            yield break;
        }
        foreach (var anim in animationQueue)
        {
            StartCoroutine(AnimateSprite(anim.Item1, anim.Item2));
        }
        yield return new WaitUntil(() => isActive == false);
        animationQueue.Clear();
    }


    /// <summary>
    /// Animates a sprite, then deletes it.
    /// </summary>
    /// <param name="spriteType">Name of the sprite to animate.</param>
    /// <param name="position">Where the sprite is displayed.</param>
    public IEnumerator AnimateSprite(string spriteType, Vector3 position)
    {
        if (!spriteLibrary.ContainsKey(spriteType)) yield break;
        isActive = true;
        GameObject animSprite = Instantiate(spriteLibrary[spriteType]);
        animSprite.transform.position = position;
        yield return new WaitForSeconds(animSprite.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length - 0.04f);
        Destroy(animSprite);
        isActive = false;
    }
}
