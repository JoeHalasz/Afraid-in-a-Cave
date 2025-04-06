using UnityEngine;

public class ConnectConnectionPoints : MonoBehaviour
{

    [SerializeField]
    public GameObject connection = null;

    void OnTriggerStay(Collider collider)
    {
        if (connection == null)
        {
            connection = collider.gameObject;
            collider.gameObject.GetComponent<ConnectConnectionPoints>().connection = gameObject;
        }
        else
        {
            // if the connection is already set, check if its the same object. If it is, increment the counter
            if (!collider.gameObject == connection)
            {
                // if (connection.GetComponent<ConnectConnectionPoints>().connection == gameObject)
                connection.GetComponent<ConnectConnectionPoints>().connection = null;
                collider.gameObject.GetComponent<ConnectConnectionPoints>().connection = null;
                connection = collider.gameObject;
                collider.gameObject.GetComponent<ConnectConnectionPoints>().connection = gameObject;
            }
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject == connection)
        {
            connection.GetComponent<ConnectConnectionPoints>().connection = null;
            connection = null;
        }
    }

}
