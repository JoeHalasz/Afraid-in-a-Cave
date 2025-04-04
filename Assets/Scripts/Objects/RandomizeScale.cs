using UnityEngine;

public class RandomizeScale : MonoBehaviour
{

    [SerializeField]
    float maxScale = 5f; // this is a percentage of the original scale

    void Start()
    {
        // Get the original scale of the object
        Vector3 originalScale = transform.localScale;

        // Generate a random scale factor between 0 and maxScale
        float randomScaleFactor = Random.Range(-maxScale, maxScale)/100f;

        // Apply the random scale factor to each axis of the original scale
        transform.localScale += new Vector3(originalScale.x * randomScaleFactor, originalScale.y * randomScaleFactor, originalScale.z * randomScaleFactor);
    }
}
