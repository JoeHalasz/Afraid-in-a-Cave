using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;

public class EquiptItem : NetworkBehaviour
{
    
    Dictionary<string, GameObject> equiptItems = new Dictionary<string, GameObject>();
    GameObject cam;
    GameObject equpitedItem;
    public bool hasItemEquipted => equpitedItem != null;

    PickupItem pickupItemScript;

    void Start()
    {
        if (!HasAuthority || !IsSpawned) return;
        cam = transform.Find("Camera").gameObject;
        if (cam == null)
            Debug.LogError("EquiptItem is not attached to a camera object.");
        
        // go through this objects child object called Equipables and add them to the list
        foreach (Transform child in transform.Find("Equipables"))
        {
            equiptItems.Add(child.name, child.gameObject);
            child.gameObject.SetActive(false);
        }
        pickupItemScript = GetComponent<PickupItem>();
    }

    void Update()
    {
        if (!HasAuthority || !IsSpawned) return;
        if (Input.GetKeyDown(KeyCode.G) && !pickupItemScript.isHoldingItem)
        {
            if (equpitedItem != null)
            {
                equpitedItem.GetComponent<Equipable>().OnUnequipt();
                equpitedItem.SetActive(false);
                equpitedItem = null;
            }
            else
            {
                GameObject item = equiptItems["Pickaxe"];
                item.SetActive(true);
                item.transform.localPosition = new Vector3(.6f, -.6f, .85f);
                item.GetComponent<Equipable>().OnEquipt();
                equpitedItem = item;
            }
        }
    }
}