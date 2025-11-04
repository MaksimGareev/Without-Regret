using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class ProtectedNPC : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] CheckPoints;
    public int point;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (point >= CheckPoints.Length)
        {
            point = 0;
        }

        Transform target = CheckPoints[point];
        agent.SetDestination(target.position);
    }

    private void OnTriggerEnter(Collider other)
    {
       if (other.gameObject.CompareTag("Point"))
        {
            point ++;
        }
    }
}
