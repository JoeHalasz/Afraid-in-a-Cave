using UnityEngine;
using System.Collections.Generic;

public class Insanity : MonoBehaviour
{
    
    PlayerStats playerStats;

    AudioSource audioSource;

    // first list is 20% chance, second list is 40% chance, third list is 60% chance
    List<List<AudioClip>> insanitySounds = new List<List<AudioClip>>();

    [SerializeField]
    List<AudioClip> insanitySounds20 = new List<AudioClip>();
    [SerializeField]
    List<AudioClip> insanitySounds40 = new List<AudioClip>();
    [SerializeField]
    List<AudioClip> insanitySounds60 = new List<AudioClip>();
    

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null) Debug.LogError("Insanity: PlayerStats component not found on " + gameObject.name);
        insanitySounds.Add(insanitySounds20);
        insanitySounds.Add(insanitySounds40);
        insanitySounds.Add(insanitySounds60);
        InvokeRepeating("outsideCheck", 1f, 1f);
        Invoke("insanitySoundEffect", 1f);
    }

    void outsideCheck()
    {
        if (transform.position.y > 0)
            playerStats.addInsanity(-1);
    }

    void insanitySoundEffect()
    {
        // if we are at 20% chance to play something from the 20percent List
        // if we are at 40% chance to play something from the 40percent list or the 20percent list
        // if we are at 60% chance to play something from the 60percent list or the 40percent list or the 20percent list
        int m = Mathf.Min(playerStats.getInsanity() / 20, 3);
        if (m != 0)
        {
            int r = Random.Range(0, m);
            if (insanitySounds[r].Count != 0)
            {
                int randomSound = Random.Range(0, insanitySounds[r].Count);
                audioSource.PlayOneShot(insanitySounds[r][randomSound]);
            }
        }
        //        min                                range size
        float t = 40f - ((playerStats.getInsanity() / 10f) * 2);
        Invoke("insanitySoundEffect", Random.Range(t - t/3.0f, t + t/2.0f));
    }
}
