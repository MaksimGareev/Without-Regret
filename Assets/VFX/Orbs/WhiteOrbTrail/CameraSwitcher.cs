using UnityEngine;

public class ObjectCameraSwitch : MonoBehaviour
{
    public Camera otherCamera;        // second camera to switch with
    public Transform lookTarget;      // object this camera will look at
    public float rotationSpeed = 5f;  // smooth rotation speed

    private Camera thisCamera;
    private bool isActive;

    void Start()
    {
        thisCamera = GetComponent<Camera>();
        isActive = thisCamera.enabled;
    }

    void Update()
    {
        // Press C → toggle between this camera and the other one
        if (Input.GetKeyDown(KeyCode.C))
        {
            isActive = !isActive;

            if (thisCamera != null)
                thisCamera.enabled = isActive;

            if (otherCamera != null)
                otherCamera.enabled = !isActive;
        }
    }

    void LateUpdate()
    {
        // Only rotate when this camera is active and has a target
        if (!isActive || lookTarget == null) return;

        // Smoothly rotate camera to face the target object
        Vector3 direction = lookTarget.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}