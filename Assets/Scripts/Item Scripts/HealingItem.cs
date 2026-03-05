using UnityEngine;

public class HealingItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TimerRingUI.Instance.AddRingSection(1);
            Destroy(gameObject);
        }
    }
}
