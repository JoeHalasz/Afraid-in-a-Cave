using UnityEngine;
using Unity.Netcode;

public class Stalagmite : NetworkBehaviour
{
    [SerializeField]
    public AudioClip sound;
    private AudioSource audioSource;
    bool triggered = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // on collision enter play the sound and destroy this object
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Stalagmite: OnTriggerEnter called, triggered: " + triggered);
        if (!triggered && other.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Thats the ground");
            triggered = true;
            gameObject.GetComponent<Renderer>().enabled = false;
            audioSource.PlayOneShot(sound);
            Destroy(transform.parent.gameObject, 1f);
        }
    }
}
