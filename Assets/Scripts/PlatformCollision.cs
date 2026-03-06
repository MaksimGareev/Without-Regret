using UnityEngine;

public class PlatformCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // if (!other.GetComponent<PlayerFloating>().IsFloating)
            // {
            //     other.transform.SetParent(transform);
            // }

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
            //other.transform.SetParent(null);
            if (other.GetComponent<PlayerController>() != null)
            {
                other.GetComponent<PlayerController>().SetCurrentPlatform(null);
            }
        }
    }
}
