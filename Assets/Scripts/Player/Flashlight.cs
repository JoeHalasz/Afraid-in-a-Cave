using UnityEngine;

public class Flashlight : MonoBehaviour
{

    [SerializeField]
    GameObject flashLight;

    void Start()
    {
        if (flashLight != null)
        {
            flashLight.SetActive(false); // turn off the flashlight at start
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (flashLight != null)
            {
                flashLight.SetActive(!flashLight.activeSelf); // toggle the flashlight on/off
            }
        }
    }
}
