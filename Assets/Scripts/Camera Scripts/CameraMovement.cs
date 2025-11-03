using System.Collections;
using System.Collections.Generic;
using Ink.Parsed;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target; // Reference the player as the intended target of the camera
    public Vector3 defaultOffset = new Vector3(0, 8, 8); // Height and distance away from the player
    public Vector3 defaultLookAtOffset = Vector3.zero;
    public float smoothSpeed = 5f; // Speed the camera moves to follow the player

    [Header("Camera Control Settings")]
    [SerializeField] private bool rotateCamera = false;
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float maxPitch = 45f;
    [SerializeField] private float maxYaw = 120f;
    [SerializeField] private float translateSpeed = 5f;
    [SerializeField] private float translateLimit = 4f;
    [SerializeField] private float returnSpeed = 4f;

    [Header("Switch Settings")]
    public float switchSpeed = 2f; // Speed of offset transition
    private bool isSwitching = false;   // prevent spam switching 

    private Vector3 currentOffset;
    private Vector3 currentLookAtOffset;
    private float yaw = 0f;
    private float pitch = 0f;
    private PlayerThrowing playerThrowing;
    private bool isThrowing;
    private ToggleInventoryUI toggleInventoryUI;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target not assigned!");
            enabled = false;
            return;
        }

        playerThrowing = target.GetComponent<PlayerThrowing>();
        toggleInventoryUI = target.GetComponent<ToggleInventoryUI>();

        currentOffset = defaultOffset;
        currentLookAtOffset = defaultLookAtOffset;

        yaw = 0f;
        pitch = 0f;

        transform.position = target.position + currentOffset;
        transform.LookAt(target.position + currentLookAtOffset);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) return;

        PlayerController pc = target.GetComponent<PlayerController>();
        if (pc != null)
        {
            if (pc.MovementLocked == true)
            {
                pc.enabled = false;
                if (Input.GetKeyDown(KeyCode.Space) && isSwitching == false)
                {
                    Debug.Log("Player movement is locked cannot rotate camera");
                }
            }
        }

        float horizontalInput = Input.GetAxis("Xbox RightStick X");
        float verticalInput = Input.GetAxis("Xbox RightStick Y");

        bool hasInput = Mathf.Abs(horizontalInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f;

        if (playerThrowing != null)
        {
            isThrowing = playerThrowing.GetIsCharging();
        }

        if (rotateCamera)
        {
            if (hasInput && !isThrowing && !toggleInventoryUI.isEnabled)
            {
                HandleRotation(horizontalInput, verticalInput);
            }
            else
            {
                ReturnRotation();
            }
            
        }
        else
        {
            if (hasInput && !isThrowing && !toggleInventoryUI.isEnabled)
            {
                HandleTranslation(horizontalInput, verticalInput);
            }
            else
            {
                ReturnTranslation();
            }
            
        }

        if (Input.GetKeyDown(KeyCode.R) && pc.MovementLocked == false && PlayerController.DialogueActive == false)
        {
            StartCoroutine(SwitchCameraZ());
        }

        // Position of the camera
        Vector3 Position = target.position + currentOffset;

        // Smooth following of the player
        transform.position = Vector3.Lerp(transform.position, Position, Mathf.Clamp01(smoothSpeed * Time.deltaTime));

        Vector3 lookAtPos = target.position + currentLookAtOffset;

        // Look at the Player
        transform.LookAt(lookAtPos);
    }

    private void HandleRotation(float horizontalInput, float verticalInput)
    {
        yaw += horizontalInput * rotateSpeed * Time.deltaTime;
        pitch += -verticalInput * rotateSpeed * Time.deltaTime;

        yaw = Mathf.Clamp(yaw, -Mathf.Abs(maxYaw), Mathf.Abs(maxYaw));
        pitch = Mathf.Clamp(pitch, -Mathf.Abs(maxPitch), Mathf.Abs(maxPitch));

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        currentOffset = rotation * defaultOffset;

        currentLookAtOffset = defaultLookAtOffset;
    }
    
    private void ReturnRotation()
    {
        yaw = Mathf.Lerp(yaw, 0f, Mathf.Clamp01(returnSpeed * Time.deltaTime));
        pitch = Mathf.Lerp(pitch, 0f, Mathf.Clamp01(returnSpeed * Time.deltaTime));

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        currentOffset = rotation * defaultOffset;

        currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, defaultLookAtOffset, Mathf.Clamp01(returnSpeed * Time.deltaTime));
    }

    private void HandleTranslation(float horizontalInput, float verticalInput)
    {
        Vector3 delta = (Vector3.right * horizontalInput + Vector3.forward * verticalInput) * translateSpeed * Time.deltaTime;

        currentOffset += delta;
        currentLookAtOffset += delta;

        Vector3 offsetFromDefault = currentOffset - defaultOffset;

        if (offsetFromDefault.magnitude > translateLimit)
        {
            offsetFromDefault = offsetFromDefault.normalized * translateLimit;
            currentOffset = defaultOffset + offsetFromDefault;
            currentLookAtOffset = defaultLookAtOffset + offsetFromDefault;
        }
    }
    
    private void ReturnTranslation()
    {
        currentOffset = Vector3.Lerp(currentOffset, defaultOffset, Mathf.Clamp01(returnSpeed * Time.deltaTime));
        currentLookAtOffset = Vector3.Lerp(currentLookAtOffset, defaultLookAtOffset, Mathf.Clamp01(returnSpeed * Time.deltaTime));
    }

    private IEnumerator SwitchCameraZ()
    {
        //isSwitching = true;

        float startZ = defaultOffset.z;
        float endZ = -startZ;
        float elapsed = 0f;

        while (elapsed < 0f)
        {
            isSwitching = true;
            elapsed += Time.deltaTime * switchSpeed;
            float newZ = Mathf.Lerp(startZ, endZ, Mathf.SmoothStep(0, 1, elapsed));
            defaultOffset = new Vector3(defaultOffset.x, defaultOffset.y, newZ);
            yield return null;
        }

        defaultOffset = new Vector3(defaultOffset.x, defaultOffset.y, endZ);
        isSwitching = false;
    }
}
