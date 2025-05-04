using UnityEngine;

public class StalagmiteTrigger : MonoBehaviour
{

    [SerializeField]
    public AudioClip sound; // the sound to play
    private AudioSource audioSource; // the audio source to play the sound on
    bool triggered = false;
    GameObject rock;

    void Start()
    {
        rock = transform.parent.Find("Rock").gameObject;
        audioSource = rock.GetComponent<AudioSource>();
    }

    // on player trigger enter make the Rock object fall
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("StalagmiteTrigger: OnTriggerEnter called, triggered: " + triggered);
        if (!triggered && other.CompareTag("Player"))
        {
            Debug.Log("Thats the player");
            triggered = true;
            rock.GetComponent<Rigidbody>().isKinematic = false;
            rock.GetComponent<Rigidbody>().useGravity = true;
            audioSource.PlayOneShot(sound);
        }
    }
}