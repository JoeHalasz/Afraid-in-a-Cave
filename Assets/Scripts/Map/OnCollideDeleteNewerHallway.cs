using UnityEngine;

public class OnCollideDeleteNewerHallway : MonoBehaviour
{
    // when collision detected with another hallway or room, delete the one with the newer number in the name
    void OnTriggerStay(Collider collider)
    {
        // Debug.Log($"Collision detected with {collider.gameObject.name} and {gameObject.name}.");
        if (collider.gameObject.name.Contains("Hallway") || collider.gameObject.name.Contains("Room") || collider.gameObject.name.Contains("Extra"))
        {
            // remove the "Hallway" or "Room" from the name to get the number
            string thisName = gameObject.name.Replace("Hallway", "").Replace("Room", "").Replace("(Clone)","").Replace("Extra", "");
            string otherName = collider.gameObject.name.Replace("Hallway", "").Replace("Room", "").Replace("(Clone)","").Replace("Extra", "");
            int thisNumber = int.Parse(thisName);
            int otherNumber = int.Parse(otherName);
            if (thisNumber > otherNumber)
            {
                // Debug.Log($"Collision detected with {collider.gameObject.name} and {gameObject.name}. Deleting {gameObject.name}.");
                // if this object has children that have the word "Hallway" or "Room" in their name, print a warning
                foreach (Transform child in transform)
                    if (child.name.Contains("Hallway") || child.name.Contains("Room") || child.name.Contains("Extra"))
                        Debug.LogWarning($"Child {child.name} of {gameObject.name} has the word 'Hallway', 'Room' or 'Extra' in its name. This may cause issues.");
                Destroy(gameObject);
            }
            else if (thisNumber < otherNumber)
            {
                // Debug.Log($"Collision detected with {collider.gameObject.name} and {gameObject.name}. Deleting {collider.gameObject.name}.");
                foreach (Transform child in collider.transform)
                    if (child.name.Contains("Hallway") || child.name.Contains("Room") || child.name.Contains("Extra"))
                        Debug.LogWarning($"Child {child.name} of {collider.gameObject.name} has the word 'Hallway', 'Room' or 'Extra' in its name. This may cause issues.");
                Destroy(collider.gameObject);
            }
        }
    }
}
