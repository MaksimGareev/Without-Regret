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

    // Start is called before the first frame update
    void Start()
    {
        // orbit pivot at runtime
       /* OrbitPivot = new GameObject("OrbitPivot").transform;
        OrbitPivot.position = player.position;

        //BobObject = this.transform;
        //BobObject.SetParent(OrbitPivot);
        transform.SetParent(OrbitPivot);
        BobObject.localPosition = new Vector3(OrbitRadius, 0f, 0f);*/
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player == null) return;

        // Keep pivot centered on the player
        //OrbitPivot.position = player.position;

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

        //OrbitPivot.rotation = Quaternion.Euler(0f, OrbitAngle * Mathf.Rad2Deg, 0f);

        // Orbit position (X/Z circle around player)
        //float x = Mathf.Cos(OrbitAngle) * OrbitRadius;
        //float z = Mathf.Sin(OrbitAngle) * OrbitRadius;

        // Bobbing motion (sin wave up/down)
       // float y = Mathf.Sin(Time.time * BobSpeed) * BobHeight;
       // transform.localPosition = new Vector3(OrbitRadius, y + 1f, 0f);

       // Vector3 lookDir = (player.position - transform.position).normalized;
       // Quaternion targetRot = Quaternion.LookRotation(lookDir);
       // transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
    }
}
