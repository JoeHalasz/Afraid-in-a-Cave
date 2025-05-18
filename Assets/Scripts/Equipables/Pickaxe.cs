using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;

public class Pickaxe : NetworkBehaviour
{
    public float swingDuration = 0.5f; // Duration of the swing animation

    private bool isSwinging = false;

    GameObject cam;
    LayerMask layerMask;

    [SerializeField]
    float range = 4f;

    ExactFollow exactFollow;

    void Start()
    {
        if (!HasAuthority || !IsSpawned) return;
        // find all the player objects and find the one that has a camera enabled 
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in players)
        {
            GameObject camera = p.transform.Find("Camera").gameObject;
            if (camera != null && camera.activeSelf)
            {
                cam = camera;
                break;
            }
        }

        layerMask = LayerMask.GetMask("Mineable");
        exactFollow = GetComponent<ExactFollow>();
    }

    private void OnEnable()
    {
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        isSwinging = false;
    }

    public void OnClick()
    {
        if (!HasAuthority || !IsSpawned) return;
        if (!isSwinging)
        {
            StartCoroutine(SwingPickaxe());
            tryMine();
        }
        Debug.DrawRay(cam.transform.position, cam.transform.forward * range, Color.blue);
    }

    void tryMine()
    {
        // cast a ray from the camera straight forward and see if it hits something with the "Mineable" tag
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, range, layerMask)
            && (hit.collider.gameObject != gameObject))
            hit.collider.GetComponent<Mineable>().mine();
    }

    private IEnumerator SwingPickaxe()
    {
        isSwinging = true;

        float elapsedTime = 0f;
        float startRotation = 0f;
        float endRotation = 90f;

        // Swing forward
        while (elapsedTime < swingDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float rotation = Mathf.Lerp(startRotation, endRotation, elapsedTime / (swingDuration / 2));
            exactFollow.rotationOffset = new Vector3(rotation, 0, 0);
            // transform.localRotation = Quaternion.Euler(rotation, 0, 0);
            yield return null;
        }

        elapsedTime = 0f;

        // Swing back
        while (elapsedTime < swingDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float rotation = Mathf.Lerp(endRotation, startRotation, elapsedTime / (swingDuration / 2));
            exactFollow.rotationOffset = new Vector3(rotation, 0, 0);
            // transform.localRotation = Quaternion.Euler(rotation, 0, 0);
            yield return null;
        }

        isSwinging = false;
    }
}
