using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockPickUI : MonoBehaviour
{
    public RectTransform LockIndicator;
    public RectTransform PickCursor;

    public float CursorSpeed = 100f;
    public float MaxRotation = 90f;
    public float UnlockAngle = 30f;
    public float UnlockTolerance = 5f;

    private float CurrentAngle = 0f;
    

    private bool isActive = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        // Rotate pick cursor with horisontal input (A/D)
        float input = Input.GetAxis("Horizontal");
        float RotationAmount = -input * CursorSpeed * Time.deltaTime;
        //PickCursor.Rotate(0, 0, -input * CursorSpeed * Time.deltaTime);

        // Update and clamp rotation
        CurrentAngle += RotationAmount;
        CurrentAngle = Mathf.Clamp(CurrentAngle, -MaxRotation, MaxRotation);

        // Apply rotation to pick cursor
        PickCursor.localEulerAngles = new Vector3(0, 0, CurrentAngle);

        if (LockIndicator != null)
        {
            LockIndicator.localEulerAngles = new Vector3(0, 0, UnlockAngle);
        }

        // Press space to attempt unlocking
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(PickCursor.localEulerAngles.z, UnlockAngle));
            if (angleDifference <= UnlockTolerance)
            {
                Debug.Log("Its Unlocked");
                DeactivateLockPick();
            }
            else
            {
                Debug.Log("Failed, try again");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeactivateLockPick();
        }
    }

    // Create a random unlock angle
    public void ActivateLockPick()
    {
        isActive = true;
        CurrentAngle = 0f;
        PickCursor.localEulerAngles = Vector3.zero;

        //PickCursor.localRotation = Quaternion.Euler(0, 0, 0);
        //LockIndicator.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public void DeactivateLockPick()
    {
        isActive = false;
        
    }
}
