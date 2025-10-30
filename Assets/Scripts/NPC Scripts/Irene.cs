using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Irene : MonoBehaviour
{
    public Transform player;            // the player to follow
    public float FollowDistance = 2f;   // how far behind the player
    public float FollowSpeed = 3f;      // movement speed
    public float RotationSpeed = 3f;    // how fast the NPC rotates

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Follow();
    }

    public void Follow()
    {
        if (player == null) return;

        // target behind the player
        Vector3 targetPosition = player.position - player.forward * FollowDistance;

        // Keep at the same height as the player
        targetPosition.y = transform.position.y;

        // smoothly move towards the player
        transform.position = Vector3.Lerp(transform.position, targetPosition, FollowSpeed * Time.deltaTime);

        // always face the player
        Vector3 LookDirection = player.position - transform.position;
        LookDirection.y = 0f;

        if (LookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(LookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }
}
