using System;
using System.Collections.Generic;
using UnityEngine;

// helper component for the hook object; detects collisions with moveable objects
// and notifies the owner.
public class HookController : MonoBehaviour
{
    public MoveableObject HookedObject { get; private set; }
    public bool HasHit => HookedObject != null;

    private Action<GameObject, HookController> onHooked;
    private Action<HookController> onStopped;

    private GameObject ignoreCollisionWith;
    private readonly List<(Collider hookCol, Collider playerCol)> ignoredPairs = new();

    void Awake()
    {
        gameObject.SetActive(false);
    }

    // Initialize the controller.
    // Optionally pass a GameObject (usually the player) whose colliders will be ignored by the hook while active.
    public void Initialize(Action<GameObject, HookController> onHookedCb, Action<HookController> onStoppedCb, GameObject ignoreWith = null)
    {
        onHooked = onHookedCb;
        onStopped = onStoppedCb;
        ignoreCollisionWith = ignoreWith;

        if (ignoreCollisionWith != null)
        {
            var ignoreColliders = ignoreCollisionWith.GetComponentsInChildren<Collider>();
            var hookColliders = GetComponentsInChildren<Collider>();

            foreach (var hc in hookColliders)
            {
                if (hc == null) continue;
                foreach (var ic in ignoreColliders)
                {
                    if (ic == null) continue;
                    // ignore relevant collisions
                    Physics.IgnoreCollision(hc, ic, true);
                    ignoredPairs.Add((hc, ic));
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Freeze the hook on first contact so it stays where it lands, preventing any rolling
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Only attach to MoveableObject if it's grabbable, otherwise we just freeze the hook in place.
        if (HookedObject != null) return;

        if (collision.gameObject.TryGetComponent<MoveableObject>(out var moveable) && moveable.isGrabbable)
        {
            Debug.Log("Hooked onto moveable object: " + collision.gameObject.name);
            HookedObject = moveable;
            onHooked?.Invoke(collision.gameObject, this);
        }
    }

    private void OnDisable()
    {
        // restore any ignored collisions when the hook is disabled so future uses are clean
        if (ignoredPairs.Count > 0)
        {
            foreach (var (hookCol, playerCol) in ignoredPairs)
            {
                if (hookCol != null && playerCol != null)
                    Physics.IgnoreCollision(hookCol, playerCol, false);
            }
            ignoredPairs.Clear();
        }

        onStopped?.Invoke(this);
    }
}
