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
        if (!HasAuthority)
        {
            Debug.Log("Not the authority, not spawning objects.");
            return;
        }

        if(!NetworkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log("Not the session owner, not spawning objects.");
            return;
        }
        Debug.Log("Spawning objects...");

        Random.InitState(seed);

        List<Vector3> randPositions = new List<Vector3>();
        // rotation
        List<Quaternion> randRotations = new List<Quaternion>();
        // masses
        List<float> randMasses = new List<float>();
        for (int i = 0; i < numberOfObjects; i++)
        {
            randPositions.Add(new Vector3(Random.Range(-10, 10), 1, Random.Range(-10, 10)));
            randRotations.Add(Quaternion.Euler(0, Random.Range(0, 360), 0));
            randMasses.Add(Random.Range(.1f,1));
        }

        for (int i = 0; i < numberOfObjects; i++)
        {
            var obj = Instantiate(prefab, randPositions[i], randRotations[i]);
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.mass = randMasses[i];
            NetworkObject networkObject = obj.GetComponent<NetworkObject>();
            networkObject.Spawn();
        }
    }

}
