using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;

public class EquipItem : NetworkBehaviour
{
    List<string> itemTypes = new List<string>();
    Dictionary<string, GameObject> equipItems = new Dictionary<string, GameObject>();
    GameObject cam;
    GameObject currentEquipItem;
    string currentEquipItemName;
    public bool hasItemEquipped => currentEquipItem != null;

    PickupItem pickupItemScript;
    SyncVars syncVars;

    MonoBehaviour currentEquipItemScript;

    void Start()
    {
        syncVars = GetComponent<SyncVars>();
        cam = transform.Find("Camera").gameObject;
        if (cam == null)
            Debug.LogError("equipItem is not attached to a camera object.");

        equipItems.Add("None", null);
        itemTypes.Add("None");
        // loop though everything in resources/Prefabs/Equipables
        foreach (GameObject item in Resources.LoadAll("Prefabs/Equipables", typeof(GameObject)))
        {
            equipItems.Add(item.name, item.gameObject);
            itemTypes.Add(item.name);
            item.gameObject.SetActive(false);
        }

        pickupItemScript = GetComponent<PickupItem>();
        unequipItem();
    }

    void unequipItem()
    {
        if (currentEquipItem != null)
        {
            // Debug.Log("Unequipping item: " + currentEquipItemName);
            currentEquipItem.GetComponent<Equipable>().OnUnequip();
            currentEquipItem.GetComponent<NetworkObject>().Despawn();
            Destroy(currentEquipItem);
            currentEquipItem = null;
            currentEquipItemScript = null;
            currentEquipItemName = null;
            if (HasAuthority)
                syncVars.playerEquippedItem.Value = -1;
        }
    }

    void equipItem(string itemName)
    {
        if (currentEquipItemName == itemName) return;
        // Debug.Log("Equipping item: " + itemName);
        unequipItem();
        // make a clone of the item
        GameObject item = Instantiate(equipItems[itemName], cam.transform);
        item.name = itemName;
        item.active = true;
        item.GetComponent<NetworkObject>().Spawn();
        // add an ExactFolllow script to the item and have it follow the camera
        ExactFollow exactFollow = item.AddComponent<ExactFollow>();
        exactFollow.target = cam;
        exactFollow.positionOffset = new Vector3(.6f, -.6f, .85f);
        item.GetComponent<Equipable>().OnEquip();
        currentEquipItem = item;
        currentEquipItemScript = item.GetComponent(item.name.Replace("(Clone)", "")) as MonoBehaviour;
        currentEquipItemName = itemName;
        if (HasAuthority)
            syncVars.playerEquippedItem.Value = itemTypes.IndexOf(item.name);
    }

    void Update()
    {
        if (!HasAuthority || !IsSpawned) return;
        if (Input.GetKeyDown(KeyCode.G) && !pickupItemScript.isHoldingItem)
        {
            if (currentEquipItemName == "Pickaxe")
                unequipItem();
            else
                equipItem("Pickaxe");
        }

        if (currentEquipItemScript != null)
            if (Input.GetMouseButton(0))
                currentEquipItemScript.Invoke("OnClick", 0);
    }

    void FixedUpdate()
    {
        int newEquipItemId = syncVars.playerEquippedItem.Value;
        // Debug.Log($"New equip item ID: {newEquipItemId} Current item: {currentEquipItemName}");
        if (newEquipItemId == -1)
        {
            unequipItem();
            return;
        }
        if (newEquipItemId < itemTypes.Count && itemTypes[newEquipItemId] != currentEquipItemName)
            equipItem(itemTypes[newEquipItemId]);
    }
}