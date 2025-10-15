using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target; // Reference the player as the intended target of the camera
    public Vector3 OffSet = new Vector3(0, 8, -8); // Height and distance away from the player
    public float smoothSpeed = 5f; // Speed the camera moves to follow the player
    public float SwitchThreshold = 0.06f;
    public float SwitchSpeed = 2f;

    private bool IsSwitching = false;
    private bool OnLeftSide = false;
    
    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) return;

        // Detect if player is facing towards the camera
        Vector3 ToCamera = (target.position - target.position).normalized;
        float dot = Vector3.Dot(target.forward, ToCamera);

        // If the player starts moving toward the camera and not already switching
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SwitchCameraSide());
        }

        // Smooth following of the player
        Vector3 DesiredPosition = target.position + target.TransformDirection(OffSet);
        transform.position = Vector3.Lerp(transform.position, DesiredPosition, smoothSpeed * Time.deltaTime);
        
        // Look at the Player
        transform.LookAt(target);
    }

    private IEnumerator SwitchCameraSide()
    {
        IsSwitching = true;

        Vector3 startOffset = OffSet;
        Vector3 endOffset = new Vector3(-OffSet.x, OffSet.y, -OffSet.z);
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * SwitchSpeed;
            OffSet = Vector3.Lerp(startOffset, endOffset, Mathf.SmoothStep(0, 1, elapsed));
            yield return null;
        }

        OffSet = endOffset;
        OnLeftSide = !OnLeftSide;
        IsSwitching = false;
    }
}
