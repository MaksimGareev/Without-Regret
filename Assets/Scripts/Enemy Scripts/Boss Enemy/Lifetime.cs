using System.Collections;
using UnityEngine;

// This class is responsible for handling the lifetime of the object passed into it
public class Lifetime: MonoBehaviour
{
    private GameObject obj;
    private float lifetimeDuration;

    public void Initialize(GameObject obj, float lifetime)
    {
        this.obj = obj;
        lifetimeDuration = lifetime;

        StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(lifetimeDuration);

        if (obj != null)
        {
            Destroy(obj);
        }
    }
}
