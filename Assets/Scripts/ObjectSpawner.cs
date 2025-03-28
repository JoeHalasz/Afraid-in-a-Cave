using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class ObjectSpawner : NetworkBehaviour
{

    public GameObject prefab;
    [SerializeField]
    [Range(0, 100)]
    int numberOfObjects = 10;

    int seed = 1234567;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!HasAuthority) return;

        if(!NetworkManager.LocalClient.IsSessionOwner) return;

        Random.InitState(seed);

        List<Vector3> randPositions = new List<Vector3>();
        // rotation
        List<Quaternion> randRotations = new List<Quaternion>();
        for (int i = 0; i < numberOfObjects; i++)
        {
            randPositions.Add(new Vector3(Random.Range(-10, 10), Random.Range(1,10), Random.Range(-10, 10)));
            randRotations.Add(Quaternion.Euler(0, Random.Range(0, 360), 0));
        }

        for (int i = 0; i < numberOfObjects; i++)
        {
            var obj = Instantiate(prefab, randPositions[i], randRotations[i]);
            NetworkObject networkObject = obj.GetComponent<NetworkObject>();
            networkObject.Spawn();
        }
    }

}
