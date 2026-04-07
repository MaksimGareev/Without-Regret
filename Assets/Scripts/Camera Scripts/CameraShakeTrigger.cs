using UnityEngine;

public class CameraShakeTrigger : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    [SerializeField] private float shakeFrequency = 30f;
    [SerializeField] private bool onlyTriggerOnce = true;

    private CameraMovement cam;

    private void Start()
    {
        cam = Camera.main.GetComponent<CameraMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cam.Shake(shakeDuration, shakeMagnitude, shakeFrequency);

            if (onlyTriggerOnce)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}
