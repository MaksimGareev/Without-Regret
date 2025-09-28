using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chime : MonoBehaviour
{
    public Transform player;

    public float OrbitRadius = 2f;
    public float OrbitSpeed = 2f;
    public float OrbitAngle = 0f;

    public float BobHeight = .5f;
    public float BobSpeed = 2f;

    private Transform OrbitPivot;
    private Transform BobObject;

    // Start is called before the first frame update
    void Start()
    {
        // orbit pivot at runtime
        OrbitPivot = new GameObject("OrbitPivot").transform;
        OrbitPivot.position = player.position;

        BobObject = this.transform;
        BobObject.SetParent(OrbitPivot);

        BobObject.localPosition = new Vector3(OrbitRadius, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        // Orbit angle increases steadily
        OrbitAngle += OrbitSpeed * Time.deltaTime;

        // Orbit position (X/Z circle around player)
        float x = Mathf.Cos(OrbitAngle) * OrbitRadius;
        float z = Mathf.Sin(OrbitAngle) * OrbitRadius;

        // Bobbing motion (sin wave up/down)
        float y = Mathf.Sin(Time.time * BobSpeed) * BobHeight;

        // Final position = orbit circle + bobbing height
        Vector3 orbitPos = new Vector3(x, 1f + y, z); // "1f" keeps above the ground
        transform.position = player.position + orbitPos;

        // Face the player
         transform.LookAt(player);
    }
}
