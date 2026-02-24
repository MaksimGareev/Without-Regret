using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Header("Slow Settings")]
    public float slowMultiplier = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            playerController.Speed *= slowMultiplier;
            playerController.SprintSpeed *= slowMultiplier;
        }
        if (other.CompareTag("Enemy"))
        {
            PatrollingEnemy patrollingEnemy = other.GetComponent<PatrollingEnemy>();
            patrollingEnemy.baseSpeed *= slowMultiplier;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("NPC"))
        {
            //placeholder for comparing to each NPC, using Irene as example for now
            Irene irene = other.GetComponent<Irene>();
            {
                irene.FollowSpeed *= slowMultiplier;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            playerController.Speed /= slowMultiplier;
            playerController.SprintSpeed /= slowMultiplier;
        }
        if (other.CompareTag("Enemy") && other.TryGetComponent<PatrollingEnemy>(out var patrollingEnemy))
        {
            patrollingEnemy.baseSpeed /= slowMultiplier;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("NPC") && other.TryGetComponent<Irene>(out var irene)
        {
            //placeholder for comparing to each NPC, using Irene as example for now
            irene.FollowSpeed /= slowMultiplier;
        }

    }
}
