using System.Collections.Generic;
using UnityEngine;

// This component is added to the player's grab point and detects if the grab point is colliding with anything.
// This is used to prevent the player from grabbing an object if the grab point is colliding with something
[RequireComponent(typeof(Collider))]
public class GrabPointCollisionCheck : MonoBehaviour
{
    private readonly HashSet<Collider> collisions = new HashSet<Collider>();

    private void Start()
    {
        // Ensure collisions with player arent detected
        Physics.IgnoreCollision(GetComponent<Collider>(), GetComponentInParent<Collider>());
    }

    private void OnTriggerEnter(Collider other)
    {
        collisions.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        collisions.Remove(other);
    }

    public bool CollidingWithSomethingExcept(Collider collider)
    {
        // Check if colliding with anything other than the specified collider
        // Either the collider is in the set and there's more than one collision, or
        // the collider is not in the set and there's still at least one collision
        return (collisions.Contains(collider) && collisions.Count > 1) || (!collisions.Contains(collider) && collisions.Count > 0);
    }
}
