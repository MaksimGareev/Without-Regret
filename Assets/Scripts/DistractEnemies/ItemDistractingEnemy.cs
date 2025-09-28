using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ItemDistractingEnemy : MonoBehaviour
{
    [Header("Distraction Settings")]
    [SerializeField] private float distractionRadius = 5f;
    [SerializeField] private float distractionDuration = 3f;
    [SerializeField] private LayerMask enemyLayer;

    private bool hasLanded = false;

    void OnCollisionEnter(Collision collision)
    {
        if (hasLanded)
        {
            return;
        }

        hasLanded = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, distractionRadius, enemyLayer);
        foreach (Collider hit in hits)
        {
            EnemyDistracted distractedEnemy = hit.GetComponent<EnemyDistracted>();
            if (distractedEnemy != null)
            {
                distractedEnemy.BeginDistraction(transform.position, distractionDuration);
            }
        }
    }
}
