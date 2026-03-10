using System.Collections;
using UnityEngine;

// This class is responsible for handling the lifetime of the object passed into it
public class Lifetime: MonoBehaviour
{
    [SerializeField] private GameObject obj;
    [SerializeField] private ObjectPool pool;
    [SerializeField] private float lifetimeDuration;
    [SerializeField] private GameObject objectToDrop;

    private Coroutine destroyRoutine;

    private void OnDisable()
    {
        // Stop the destroy routine if the object is disabled before the lifetime expires
        if (destroyRoutine != null)
        {
            StopCoroutine(destroyRoutine);
        }
    }

    public void Initialize(GameObject obj, float lifetime, ObjectPool pool)
    {
        this.obj = obj;
        this.pool = pool;
        lifetimeDuration = lifetime;

        destroyRoutine = StartCoroutine(DestroyAfterDelay());
    }

    public void Initialize(GameObject obj, float lifetime, ObjectPool pool, GameObject objectToDrop)
    {
        this.obj = obj;
        this.pool = pool;
        lifetimeDuration = lifetime;
        this.objectToDrop = objectToDrop;

        destroyRoutine = StartCoroutine(DestroyAfterDelay());
    }


    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(lifetimeDuration);

        if (pool != null && obj != null)
        {
            if (objectToDrop != null)
            {
                Instantiate(objectToDrop, obj.transform.position, Quaternion.identity);
            }
            pool.Return(obj); // Return the object to the pool
        }
        else if (obj != null)
        {
            Destroy(obj);
        }
    }
}
