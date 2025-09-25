using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFloating : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float floatForce = 10f;
    [SerializeField] private float floatDuration = 5f;
    [SerializeField] private float floatLift = 50f;
    [SerializeField] private float horizontalSpeed = 5f;

    [Header("Input")]
    [SerializeField] private KeyCode floatKey = KeyCode.Space;

    private Rigidbody rb;
    private bool isFloating = false;
    private float floatTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        HandleFloatingInput();   
    }

    private void FixedUpdate()
    {
        if (isFloating)
        {
            ApplyFloatPhysics();
        }
    }

    private void HandleFloatingInput()
    {
        if (Input.GetKeyDown(floatKey) && floatTimer < floatDuration)
        {
            if (!isFloating)
            {
                isFloating = true;
                rb.AddForce(Vector3.up * floatLift, ForceMode.VelocityChange);
            }
        }

        if (Input.GetKeyUp(floatKey) || floatTimer >= floatDuration)
        {
            StopFloating();
        }
    }

    private void ApplyFloatPhysics()
    {
        floatTimer += Time.fixedDeltaTime;

        if (floatTimer >= floatDuration)
        {
            StopFloating();
            return;
        }

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontalInput, 0f, verticalInput) * horizontalSpeed;
        move = transform.TransformDirection(move);

        rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
    }

    private void StopFloating()
    {
        isFloating = false;
        floatTimer = 0f;
    }
}
