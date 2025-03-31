using UnityEngine;

public class AddPartToConnection : MonoBehaviour
{
    [SerializeField]
    bool connectPart = false;
    [SerializeField]
    int partToConnectIndex = 0;
    [SerializeField]
    int partToConnectConnectionIndex = 0;

    CreateMap createMap;

    void Start()
    {
        createMap = GameObject.Find("MapManager").GetComponent<CreateMap>();
    }

    void FixedUpdate()
    {
        if (connectPart)
        {
            connectPart = false;
            createMap.addPartToConnection(gameObject, partToConnectIndex, partToConnectConnectionIndex);
        }
    }
}
