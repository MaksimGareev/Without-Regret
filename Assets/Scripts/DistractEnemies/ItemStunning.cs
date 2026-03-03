using UnityEngine;

public class ItemStunning : MonoBehaviour
{
    [Tooltip("How long the enemy remains stunned when hit by this item while its in motion")]
    [SerializeField] float stunDuration = 2f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (gameObject.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude > 0.001f)
            {
                gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                EnemyFieldOfView enemyFOV = other.GetComponent<EnemyFieldOfView>();
                if(enemyFOV != null)
                {
                    enemyFOV.GetStunned(stunDuration);
                }

            }
        }
    }
}
