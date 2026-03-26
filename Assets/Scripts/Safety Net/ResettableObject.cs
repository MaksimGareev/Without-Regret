using UnityEngine;
using System.Collections;
public class ResettableObject : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    private bool isResetting = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    public void TriggerReset(float delay)
    {
        if (!isResetting)
        {
            StartCoroutine(ResetAfterDelay(delay));
        }
    }

    private IEnumerator ResetAfterDelay(float delay)
    {
        isResetting = true;

        yield return new WaitForSeconds(delay);

        // reset position and rotation
        transform.position = startPosition;
        transform.rotation = startRotation;

        // reset physics
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        isResetting = false;

    }

}
