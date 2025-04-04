using UnityEngine;

public class RandomDeactivate : MonoBehaviour
{
    
    [SerializeField]
    float percentChagne = 75f;

    void Start()
    {
        if (Random.Range(0f, 100f) < percentChagne)
        {
            gameObject.SetActive(false);
        }
    }

}
