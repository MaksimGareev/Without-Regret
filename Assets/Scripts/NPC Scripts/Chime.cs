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

    // Update is called once per frame
    void LateUpdate()
    {
        if (player == null) return;

        // Orbit angle increases steadily
        OrbitAngle += OrbitSpeed * Time.deltaTime;
        if (OrbitAngle > Mathf.PI * 2f) OrbitAngle -= Mathf.PI * 2f;

        // Calculate orbit position relative to player
        Vector3 offset = new Vector3(Mathf.Cos(OrbitAngle) * OrbitRadius, Mathf.Sin(Time.time * BobSpeed) * BobHeight + 1f, Mathf.Sin(OrbitAngle) * OrbitRadius);

        // Smoothly rotate toward player
        Vector3 targetPos = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);

        // smoothly rotate toward player
        if (facePlayer)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * lookSmooth);
            }
        }
    }
}
