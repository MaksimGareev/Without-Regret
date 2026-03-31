using UnityEngine;

public class hideandshow : MonoBehaviour
{
    public GameObject ObjectToToggle;

    void Start()
    {
        ObjectToToggle.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            ObjectToToggle.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            ObjectToToggle.SetActive(false);
        }
    }
}
