using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody rb;
    public Transform head;
    public Camera camera;

    [Header("Configurations")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;

    [Header("Runtime")]
    private Vector3 newVelocity;

    const float mouseSensitivity = 2f;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void LateUpdate()
    {
        HandleHeadLook();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleHeadLook()
    {
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        Vector3 currentRotation = head.eulerAngles;
        float restrictedX = RestrictAngle(currentRotation.x - mouseY, -85f, 85f);
        head.eulerAngles = new Vector3(restrictedX, currentRotation.y, currentRotation.z);
    }

    void HandleMovement()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        newVelocity = Vector3.up * rb.velocity.y; 
        newVelocity.x = Input.GetAxis("Horizontal") * speed;
        newVelocity.z = Input.GetAxis("Vertical") * speed;

        rb.velocity = transform.TransformDirection(newVelocity);
    }

    public static float RestrictAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;

        return Mathf.Clamp(angle, min, max);
    }
}