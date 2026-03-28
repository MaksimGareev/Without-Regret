using System.Runtime.CompilerServices;
using UnityEngine;



public class PlatformCollision : MonoBehaviour
{
    private Rigidbody rb;
    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();

    }
    private void OnTriggerEnter(Collider other)
    {
        if(rb && rb.isKinematic)
        {
            return;
        }
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                other.GetComponent<PlayerController>().SetCurrentPlatform(this.GetComponentInParent<OrbitingPlatform>());
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerController>() != null)
            {
                other.GetComponent<PlayerController>().SetCurrentPlatform(null);
            }
        }
    }
}
