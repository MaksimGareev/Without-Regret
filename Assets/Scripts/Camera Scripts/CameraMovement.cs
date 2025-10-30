using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target; // Reference the player as the intended target of the camera
    public Vector3 OffSet = new Vector3(0, 8, 8); // Height and distance away from the player
    public float smoothSpeed = 5f; // Speed the camera moves to follow the player
    public float switchSpeed = 2f; // Speed of offset transition

    private bool isSwitching = false;   // prevent spam switching 

    private void Start()
    {
        transform.position = target.position + OffSet;
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

        if (Input.GetKeyDown(KeyCode.R) && pc.MovementLocked == false && PlayerController.DialogueActive == false)
        {
            StartCoroutine(SwitchCameraZ());
        }

        // Position of the camera
        Vector3 Position = target.position + OffSet;

        // Smooth following of the player
        transform.position = Vector3.Lerp(transform.position, Position, smoothSpeed * Time.deltaTime);

        // Look at the Player
        transform.LookAt(target);
    }

    private IEnumerator SwitchCameraZ()
    {
        //isSwitching = true;

        float startZ = OffSet.z;
        float endZ = -startZ;
        float elapsed = 0f;

        while (elapsed < 0f)
        {
            isSwitching = true;
            elapsed += Time.deltaTime * switchSpeed;
            float newZ = Mathf.Lerp(startZ, endZ, Mathf.SmoothStep(0, 1, elapsed));
            OffSet = new Vector3(OffSet.x, OffSet.y, newZ);
            yield return null;
        }

        OffSet = new Vector3(OffSet.x, OffSet.y, endZ);
        isSwitching = false;
    }
}
