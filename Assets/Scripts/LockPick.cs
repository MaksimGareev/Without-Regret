using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockPick : MonoBehaviour
{
    public UnityEngine.Camera Cam;       // Main camera used for screen/world conversions
    public Transform InnerLock;          // 2D lock image or indicator to rotate
    public Transform PickPosition;       // Anchor in front of the UI where the 3D pick sits

    public float MaxAngle = 90f;
    public float LockSpeed = 10f;

    [Min(1)]
    [Range(1, 25)]
    public float LockRange = 10f;

    private float eulerAngle;
    private float UnlockAngle;
    private Vector2 UnlockRange;

    private float KeyPressTime = 0f;
    private bool MovePick = true;

    public event System.Action OnUnlock;  // Event called when lock is solved

    void Start()
    {
        // Initialize a new lock range at the start
        NewLock();
    }

    void Update()
    {
        if (PickPosition == null || Cam == null)
        {
            Debug.LogWarning("PickPosition or Cam not assigned in LockPick.");
            return;
        }

        // Keep the pick at the anchor position
        transform.position = PickPosition.position;

        if (MovePick)
        {
            // Convert mouse position to a direction relative to the pick anchor
            Vector3 dir = Input.mousePosition - Cam.WorldToScreenPoint(PickPosition.position);

            // Calculate angle from vertical
            eulerAngle = Vector3.Angle(dir, Vector3.up);

            Vector3 cross = Vector3.Cross(Vector3.up, dir);
            if (cross.z < 0f)
                eulerAngle = -eulerAngle;

            eulerAngle = Mathf.Clamp(eulerAngle, -MaxAngle, MaxAngle);

            // Rotate pick around Z axis
            transform.rotation = Quaternion.AngleAxis(eulerAngle, Vector3.forward);
        }

        // Handle "F" press for moving the pick into the lock
        if (Input.GetKeyDown(KeyCode.F))
        {
            MovePick = false;
            KeyPressTime = 1f;
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            MovePick = true;
            KeyPressTime = 0f;
        }

        KeyPressTime = Mathf.Clamp(KeyPressTime, 0f, 1f);

        // Calculate lock rotation based on how close pick is to the unlock angle
        float percentage = Mathf.Clamp01(1f - Mathf.Abs((eulerAngle - UnlockAngle) / MaxAngle));
        float lockRotation = percentage * MaxAngle * KeyPressTime;
        float maxRotation = percentage * MaxAngle;

        // Smoothly rotate the inner lock
        float lockLerp = Mathf.LerpAngle(InnerLock.eulerAngles.z, lockRotation, Time.deltaTime * LockSpeed);
        InnerLock.eulerAngles = new Vector3(0f, 0f, lockLerp);

        // Check if lock is successfully unlocked
        if (lockLerp >= maxRotation - 1f)
        {
            if (eulerAngle > UnlockRange.x && eulerAngle < UnlockRange.y)
            {
                Debug.Log("Unlocked!");
                OnUnlock?.Invoke();  // Notify LockedItem
                NewLock();

                MovePick = true;
                KeyPressTime = 0f;
            }
            else
            {
                // Apply small random rotation to simulate lock resistance
                float randomRotation = Random.Range(-Random.insideUnitCircle.x, Random.insideUnitCircle.x);
                transform.Rotate(0f, 0f, randomRotation);
            }
        }
    }

    // Generate a new unlock angle and range
    void NewLock()
    {
        UnlockAngle = Random.Range(-MaxAngle + LockRange, MaxAngle - LockRange);
        UnlockRange = new Vector2(UnlockAngle - LockRange, UnlockAngle + LockRange);
    }
}