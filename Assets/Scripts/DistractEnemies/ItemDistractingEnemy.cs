using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ItemDistractingEnemy : MonoBehaviour
{
    [Header("Distraction Settings")]
    [SerializeField] private float distractionRadius = 15f;
    [SerializeField] private float distractionDuration = 3f;

    private bool hasLanded = false;

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Enemy"))// if the object collides with anything other than the player, execute behavoir
        {
            if (hasLanded)
            {
                return;
            }

            BreakableObject breakableObject = collision.gameObject.GetComponent<BreakableObject>();

            if (breakableObject != null)
            {
                breakableObject.Break();
            }

            hasLanded = true;

            Collider[] hits = Physics.OverlapSphere(transform.position, distractionRadius);// find enemies within radius and distract them
            foreach (Collider hit in hits)
            {
                if (hit.gameObject.CompareTag("Enemy"))
                {
                    EnemyDistracted distractedEnemy = hit.GetComponent<EnemyDistracted>();
                    if (distractedEnemy != null)
                    {
                        distractedEnemy.BeginDistraction(transform.position, distractionDuration, gameObject);
                    }
                }
            }
        }
    }
}
