using UnityEngine;

public class CameraLookTrigger : MonoBehaviour
{
    [SerializeField] private Transform target;
    [Tooltip("Time in seconds for the camera to rotate towards the target.")]
    [SerializeField] private float rotateDuration = 1.5f;
    [Tooltip("Time in seconds to hold the camera looking at the target before allowing it to return to normal.")]
    [SerializeField] private float holdDuration = 2.0f;
    [SerializeField] private bool disableCameraInputWhileLooking = true;
    [SerializeField] private bool disablePlayerInputWhileLooking = true;
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
            if (target == null)
            {
                Debug.LogError("CameraLookTrigger: No target assigned.", this);
                return;
            }

            cam.LookAtSubject(target, 
                rotateDuration: rotateDuration, holdDuration: holdDuration, 
                disableCameraInputWhileLooking: disableCameraInputWhileLooking, disablePlayerInputWhileLooking: disablePlayerInputWhileLooking);

            if (onlyTriggerOnce)
            {
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}
