using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerPossessing : MonoBehaviour
{
    [Header("Possession Settings")]
    [SerializeField] private float possessionDuration = 5f;
    [SerializeField] private float possessionRange = 50f;
    [SerializeField] private float searchConeAngle = 30f;
    [SerializeField] private KeyCode possessKey = KeyCode.R;
    [SerializeField] private string possessButton = "Xbox Y Button";

    private PlayerController playerController;
    private Rigidbody playerRigidbody;
    private PossessedEnemyResisting possessedEnemyMovement;
    private PatrollingEnemy normalEnemyMovement;
    private NavMeshAgent enemyNavMeshAgent;
    private Rigidbody enemyRigidbody;
    private float possessionTimer;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(possessKey))
        {
            TryStartPossession(true);
            Debug.Log("Tried Possessing Keyboard");
        }
        else if (Input.GetButtonDown(possessButton))
        {
            TryStartPossession(false);
            Debug.Log("Tried Possessing Controller");
        }

        if (possessedEnemyMovement != null)
            {
                possessionTimer -= Time.deltaTime;

                Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                possessedEnemyMovement.UpdatePossession(input);

                if (possessionTimer <= 0f || Input.GetKeyUp(possessKey) || Input.GetButtonUp(possessButton))
                {
                    EndPossession();
                }
            }
    }

    private void TryStartPossession(bool IsUsingKeyboard)
    {
        PossessedEnemyResisting target = null;

        if (IsUsingKeyboard)
        {
            target = SelectEnemyMouse();
        }
        else
        {
            target = SelectEnemyController();
        }

        if (target != null)
        {
            StartPossession(target);
        }
    }

    private PossessedEnemyResisting SelectEnemyMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.TryGetComponent<PossessedEnemyResisting>(out var target))
            {
                normalEnemyMovement = hit.collider.GetComponent<PatrollingEnemy>();
                Debug.Log("Enemy found :" + hit.collider.gameObject.name);
                StartPossession(target);
            }
        }

        return null;
    }

    private PossessedEnemyResisting SelectEnemyController()
    {
        Vector3 rightStick = CalculateInputFromPOV();

        if (rightStick.sqrMagnitude < 0.1f)
        {
            return null;
        }
        Debug.DrawRay(transform.position, rightStick.normalized * possessionRange, Color.red, 0.5f);

        Collider[] hits = Physics.OverlapSphere(transform.position, possessionRange);
        Debug.Log("Found " + hits.Length + " colliders in range");
        
        foreach (var hit in hits)
        {
            Debug.Log("Collider: " + hit.name + " | Layer: " + LayerMask.LayerToName(hit.gameObject.layer) + " | Tag: " + hit.tag);
            if (hit.CompareTag("Enemy"))
            {
                Vector3 directionToEnemy = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(rightStick, directionToEnemy);
                if (angle < searchConeAngle)
                {
                    normalEnemyMovement = hit.GetComponent<PatrollingEnemy>();
                    Debug.Log("Enemy found: " + hit.name);
                    return hit.GetComponent<PossessedEnemyResisting>();
                }
            }
        }

        return null;
    }

    private Vector3 CalculateInputFromPOV()
    {
        Vector3 input = new Vector3(Input.GetAxis("Xbox RightStick X"), 0, Input.GetAxis("Xbox RightStick Y"));

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 relativeDirection = (camForward * input.x + camRight * input.z).normalized;
        return relativeDirection;
    }

    private void StartPossession(PossessedEnemyResisting target)
    {
        if (target == null)
        {
            return;
        }

        normalEnemyMovement = target.GetComponent<PatrollingEnemy>();
        enemyRigidbody = target.GetComponent<Rigidbody>();
        possessedEnemyMovement = target;
        

        possessionTimer = possessionDuration;

        if (playerController != null)
        {
            Debug.Log("Reference to player controller is null");

            Vector3 frozenPos = transform.position;
            Quaternion frozenRot = transform.rotation;

            playerController.MovementLocked = true;
            playerController.enabled = false;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;

            GetComponent<CharacterController>().enabled = false;
            transform.SetPositionAndRotation(frozenPos, frozenRot);
        }

        if (normalEnemyMovement != null)
        {
            Debug.Log("Reference to normal enemy movement is null");
            normalEnemyMovement.enabled = false;
        }

        if (enemyRigidbody != null)
        {
            //enemyRigidbody.useGravity = false;
        }

        enemyNavMeshAgent = target.GetComponent<NavMeshAgent>();
        if (enemyNavMeshAgent != null)
        {
            enemyNavMeshAgent.enabled = false;
        }

        if (!possessedEnemyMovement.enabled)
        {
            possessedEnemyMovement.enabled = true;
        }

        possessedEnemyMovement.BeginPossession();
        Debug.Log("Starting Possession of " + target.name);
    }

    private void EndPossession()
    {
        if (possessedEnemyMovement != null)
        {
            possessedEnemyMovement.EndPossession();
            if (normalEnemyMovement != null)
            {
                normalEnemyMovement.enabled = true;
            }
        }

        possessedEnemyMovement = null;
        normalEnemyMovement = null;

        if (enemyNavMeshAgent != null)
        {
            enemyNavMeshAgent.enabled = true;
        }

        if (enemyRigidbody != null)
        {
            //enemyRigidbody.useGravity = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.MovementLocked = false;
            GetComponent<CharacterController>().enabled = true;
        }
    }
}
