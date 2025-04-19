using UnityEngine;

public class RandomDeactivate : MonoBehaviour
{
    
    [SerializeField]
    float percentChange = 75f;

    void Start()
    {
        if (Random.Range(0f, 100f) < percentChange)
        {
            gameObject.SetActive(false);
        }
    }

}
