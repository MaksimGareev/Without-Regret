using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPossessing : MonoBehaviour
{
    [Header("Possession Settings")]
    [SerializeField] private float possessionDuration = 5f;
    [SerializeField] private float possessionRange = 10f;
    [SerializeField] private float searchConeAngle = 30f;
    [SerializeField] private KeyCode possessKey = KeyCode.R;
    [SerializeField] private string possessButton = "Xbox Y Button";

    private PlayerMovement playerMovement;
    private PossessedEnemyResisting possessedEnemyMovement;
    private Enemy normalEnemyMovement;
    private float possessionTimer;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(possessKey) || Input.GetButtonDown(possessButton))
        {
            TryStartPossession();
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

    private void TryStartPossession()
    {
        PossessedEnemyResisting target = null;

        if (IsUsingMouse())
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

    private bool IsUsingMouse()
    {
        return Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f;
    }

    private PossessedEnemyResisting SelectEnemyMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 20f))
        {
            if (hit.collider.TryGetComponent<PossessedEnemyResisting>(out var target))
            {
                StartPossession(target);
            }
        }

        return null;
    }

    private PossessedEnemyResisting SelectEnemyController()
    {
        Vector3 rightStick = new Vector3(Input.GetAxis("Xbox RightStick X"), 0, Input.GetAxis("Xbox RightStick Y"));

        if (rightStick.sqrMagnitude < 0.1f)
        {
            return null;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, possessionRange);

        foreach (var hit in hits)
        {
            Vector3 directionToEnemy = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(rightStick, directionToEnemy);
            if (angle < searchConeAngle)
            {
                return hit.GetComponent<PossessedEnemyResisting>();
            }
        }

        return null;
    }

    private void StartPossession(PossessedEnemyResisting target)
    {
        possessedEnemyMovement = target;
        possessionTimer = possessionDuration;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (normalEnemyMovement != null)
        {
            normalEnemyMovement.enabled = false;
        }

        possessedEnemyMovement.BeginPossession();
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

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }
}
