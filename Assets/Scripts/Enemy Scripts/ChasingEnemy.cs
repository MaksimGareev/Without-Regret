using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class ChasingEnemy : MonoBehaviour
{
    // Movement
    public NavMeshAgent agent;
    public Transform target;
    private Vector3 StoppingDistance;

    // Camera
    public Camera cam;
    public Vector3 offSet;
    public float smoothSpeed = 5f;

    // Check if enemy reached NPC
    public bool ReachedNPC = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //StoppingDistance = new Vector3(.7f, .7f, .7f);
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position - StoppingDistance);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("protectedNPC"))
        {
            ReachedNPC = true;
            // Destroy(target);

            // Lerp camera over to show enemy reaching NPC
            cam.transform.position = Vector3.Lerp(transform.position, target.position + offSet, smoothSpeed * Time.deltaTime);
            cam.transform.LookAt(target);

            // Freeze all other objects
            // Implement a delayed game over here after camera has showed the enemy and NPC
            Debug.Log("The enemy has reached the NPC");
        }
    }
}
