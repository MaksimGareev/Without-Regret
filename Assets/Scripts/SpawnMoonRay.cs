using UnityEngine;

public class SpawnMoonRay : MonoBehaviour
{
    public GameObject MoonRay;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MoonRay.SetActive(true);
        }
    }
}
