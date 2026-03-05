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
            if (playerController != null)
            {
                playerController.Speed *= slowMultiplier;
                playerController.SprintSpeed *= slowMultiplier;
            }
        }
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<PatrollingEnemy>(out var patrollingEnemy))
                patrollingEnemy.baseSpeed *= slowMultiplier;

            if (other.TryGetComponent<ChasingEnemy>(out var chasingEnemy))
                chasingEnemy.baseSpeed *= slowMultiplier;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("NPC"))
        {
            //placeholder for comparing to each NPC, using Irene as example for now
            if (other.TryGetComponent<Irene>(out var irene))
            {
                irene.FollowSpeed *= slowMultiplier;
            }
            if (other.TryGetComponent<ProtectedNPC>(out var protNPC))
            {
                Debug.Log("Protected NPC detected");
                protNPC.agent.speed *= slowMultiplier;
            }
            if (other.TryGetComponent<FriendlyNPC>(out var friendlyNPC))
            {
                friendlyNPC.agent.speed *= slowMultiplier;
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.Speed /= slowMultiplier;
                playerController.SprintSpeed /= slowMultiplier;
            }
        }
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<PatrollingEnemy>(out var patrollingEnemy))
                patrollingEnemy.baseSpeed /= slowMultiplier;

            if (other.TryGetComponent<ChasingEnemy>(out var chasingEnemy))
                chasingEnemy.baseSpeed /= slowMultiplier;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("NPC"))
        {
            //placeholder for comparing to each NPC, using Irene as example for now
            if (other.TryGetComponent<Irene>(out var irene))
            {
                irene.FollowSpeed /= slowMultiplier;
            }
            if (other.TryGetComponent<ProtectedNPC>(out var protNPC))
            {
                protNPC.agent.speed /= slowMultiplier;
            }
            if (other.TryGetComponent<FriendlyNPC>(out var friendlyNPC))
            {
                friendlyNPC.agent.speed /= slowMultiplier;
            }

        }

    }
}
