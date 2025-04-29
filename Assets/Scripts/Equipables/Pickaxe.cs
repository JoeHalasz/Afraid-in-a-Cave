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

    void Start()
    {
        if (!HasAuthority || !IsSpawned) return;
        cam = transform.parent.parent.Find("Camera").gameObject;
        layerMask = LayerMask.GetMask("Mineable");
    }

    private void OnEnable()
    {
        if (!HasAuthority || !IsSpawned) return;
        transform.localRotation = Quaternion.Euler(0, 0, 0);
        isSwinging = false;
    }

    void Update()
    {
        if (!HasAuthority || !IsSpawned) return;
        if (Input.GetMouseButton(0))
            OnClick();
    }

    private void OnClick()
    {
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
            && (hit.collider.gameObject != gameObject) )
                hit.collider.GetComponent<Mineable>().mine();
        // draw the ray
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
            transform.localRotation = Quaternion.Euler(rotation, 0, 0);
            yield return null;
        }

        elapsedTime = 0f;

        // Swing back
        while (elapsedTime < swingDuration / 2)
        {
            elapsedTime += Time.deltaTime;
            float rotation = Mathf.Lerp(endRotation, startRotation, elapsedTime / (swingDuration / 2));
            transform.localRotation = Quaternion.Euler(rotation, 0, 0);
            yield return null;
        }

        isSwinging = false;
    }
}
