using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chime : MonoBehaviour
{
    public Transform player;

    public float OrbitRadius = 2f;
    public float OrbitSpeed = 2f;

    public float BobHeight = .5f;
    public float BobSpeed = 2f;

    public bool facePlayer = true;
    public float lookSmooth = 8f;

    private float OrbitAngle;

    private Transform OrbitPivot;
    private Transform BobObject;
    public Transform model;

    public bool isInDialogue = false;

    // Update is called once per frame
    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos;

        if (!isInDialogue)
        {
            // Orbit angle increases steadily
            OrbitAngle += OrbitSpeed * Time.deltaTime;
            if (OrbitAngle > Mathf.PI * 2f) OrbitAngle -= Mathf.PI * 2f;

            // Calculate orbit position relative to player
            Vector3 offset = new Vector3(Mathf.Cos(OrbitAngle) * OrbitRadius, Mathf.Sin(Time.time * BobSpeed) * BobHeight + 1f, Mathf.Sin(OrbitAngle) * OrbitRadius);

            // Smoothly rotate toward player
            targetPos = player.position + offset;
        }
        else
        {
            // Dialogue Mode
            Vector3 offset = new Vector3(0f, Mathf.Sin(Time.time * BobSpeed) * BobHeight + 1f, OrbitRadius * 0.5f);

            targetPos = player.position + player.forward * 1.5f + offset;
        }

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);

        // smoothly rotate toward player
        if (facePlayer && model != null)
        {
            // look at players horizontal potiion only
            Vector3 lookPoint = player.position;
            lookPoint.y = model.position.y; // keep chime level

            Vector3 dir = lookPoint - model.position;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                model.rotation = Quaternion.Slerp(model.rotation, lookRot, Time.deltaTime * lookSmooth);
            }
        }
    }
}
