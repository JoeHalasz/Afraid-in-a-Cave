using UnityEngine;
using UnityEngine.VFX;

public class SlenderParticles : MonoBehaviour
{
    GameObject player;
    VisualEffect visualEffect;

    void Start()
    {
        visualEffect = GetComponentInChildren<UnityEngine.VFX.VisualEffect>();
    }

    int numTimes = 0;
    void findPlayer()
    {
        numTimes++;
        if (numTimes > 50)
            Debug.LogWarning("SlenderParticles: Could not find player after 50 tries.");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            GameObject cam = p.transform.Find("Camera").gameObject;
            if (cam.transform.parent == transform.parent) // we are the player
                break;
            if (cam != null && cam.activeSelf)
            {
                player = p;
                break;
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null)
            findPlayer();
        if (player != null)
        {
            visualEffect.SetVector3("PlayerPosition", player.transform.position);
            visualEffect.SetVector3("SlenderPosition", transform.parent.position);
        }
    }
}
