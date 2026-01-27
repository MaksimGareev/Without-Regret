using System;
using UnityEngine;

// helper component for the hook object; detects collisions with moveable objects
// and notifies the owner.
public class HookController : MonoBehaviour
{
    public MoveableObject HookedObject { get; private set; }
    public bool HasHit => HookedObject != null;

    private Action<GameObject, HookController> onHooked;
    private Action<HookController> onStopped;

    public void Initialize(Action<GameObject, HookController> onHookedCb, Action<HookController> onStoppedCb)
    {
        onHooked = onHookedCb;
        onStopped = onStoppedCb;
    }

    private void OnCollisionEnter(Collision collision)
    {
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
        onStopped?.Invoke(this);
    }
}
