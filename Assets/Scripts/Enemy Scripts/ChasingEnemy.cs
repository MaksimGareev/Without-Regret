using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
public class ChasingEnemy : MonoBehaviour
{
    // Movement
    public NavMeshAgent agent;
    public Transform target;
    //private Vector3 StoppingDistance;
    public float PursuitTimer;
    public bool Pursuiting = true;

    // Camera
    public Camera cam;
    public Vector3 offSet;
    public float smoothSpeed = 5f;

    // Check if enemy reached NPC
    public bool ReachedNPC = false;

    // Check if the enemy is possessed or distracted
    public bool Possessed = false;
    public bool Distracted = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //StoppingDistance = new Vector3(.7f, .7f, .7f);

    }

    // Update is called once per frame
    void Update()
    {
        /* PursuitTimer -= Time.deltaTime;
        PursuitCooldown();
        if (Possessed == false && Distracted == false)
        {
            if (PursuitTimer < 0)
            {
                agent.SetDestination(target.position);
                Pursuiting = true;
            }
        }*/

        agent.SetDestination(target.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("protectedNPC"))
        {
            ReachedNPC = true;
            // Destroy(target);

            // Lerp camera over to show enemy reaching NPC
           // cam.transform.position = Vector3.Lerp(transform.position, target.position + offSet, smoothSpeed * Time.deltaTime);
           // cam.transform.LookAt(target);

            // Change to the game over screen
            // SceneManager.LoadScene("GameOverScene");

            // Freeze all other objects
            // Implement a delayed game over here after camera has showed the enemy and NPC
            Debug.Log("The enemy has reached the NPC");
        }
    }

   /* IEnumerator PursuitCooldown()
    {
        yield return new WaitForSeconds(PursuitTimer);
        //SprintTimer = SprintDuration;
        Pursuiting = true;
    }*/
}
