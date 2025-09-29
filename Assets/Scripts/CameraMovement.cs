using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target; // Reference the player as the intended target of the camera
    public Vector3 OffSet = new Vector3(0, 8, -8); // Height and distance away from the player
    public float smoothSpeed = 5f; // Speed the camera moves to follow the player
    
    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null) return;

        // Position of the camera
        Vector3 Position = target.position + OffSet;

        // Smooth following of the player
        transform.position = Vector3.Lerp(transform.position, Position, smoothSpeed * Time.deltaTime);

        // Look at the Player
        transform.LookAt(target);
    }
}
