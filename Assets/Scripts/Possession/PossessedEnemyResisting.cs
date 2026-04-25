using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class PossessedEnemyResisting : MonoBehaviour
{
    [Header("Possession Settings")]
    [SerializeField] private NavMeshAgent Agent;
    [SerializeField] private float moveSpeed = 20f;
    public Transform iconPoint;

    private bool isPossessed = false;
    private Vector3 playerInput;
    private float struggleTimer;
    private Camera PlayerCamera;

    private bool HighlightApplied =  false;

    private Vector3 struggleDirection;

    private Color highlightColor = Color.white;

    private Color baseColor = Color.red;

    private ChasingEnemy chasingEnemy;


    private void Awake()
    {
        if (PlayerCamera == null)
        {
            PlayerCamera = Camera.main;
        }

        if(chasingEnemy == null)
        {
            if (gameObject.GetComponent<ChasingEnemy>())
            {
                chasingEnemy = gameObject.GetComponent<ChasingEnemy>();
            }
        }
    }
    private void FixedUpdate()
    {
        if (!isPossessed)
        {
            return;
        }
        Vector3 move = Vector3.zero;
        if (PlayerCamera != null)
        {
            Vector3 camForward = PlayerCamera.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 camRight = PlayerCamera.transform.right;
            camRight.y = 0f;
            camRight.Normalize();
            move = camForward * playerInput.y + camRight * playerInput.x;

        }
        Vector3 finalMoveDirection = gameObject.transform.position + move.normalized * moveSpeed * Time.deltaTime;

        Agent.destination = finalMoveDirection;


    }

    public void BeginPossession()
    {
        isPossessed = true;
        if(chasingEnemy != null)
        {
            chasingEnemy.Posessed = true;
        }
    }

    public void UpdatePossession(Vector3 input)
    {
        playerInput = input;
    }

    public void EndPossession()
    {

        isPossessed = false;
        playerInput = Vector3.zero;
        
        
        if (chasingEnemy != null)
        {
            chasingEnemy.Posessed = false;
        }
        if (gameObject.GetComponent<PatrollingEnemy>())
        {
            Agent.enabled = false;
            StartCoroutine(PushEnemy());
        }
    }

    private IEnumerator PushEnemy()
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        CapsuleCollider collider = gameObject.GetComponent<CapsuleCollider>();
        rb.useGravity = true;
        collider.isTrigger = false;
        Vector3 direction = gameObject.transform.forward;
        

        rb.AddForce(direction * 6, ForceMode.Impulse);
        yield return new WaitForSecondsRealtime(1f);
        Agent.enabled = true;
        rb.useGravity = false;
        collider.isTrigger = true;
    }

    public void ApplyHighlightColor()
    {
        if (!HighlightApplied)
        {
            Renderer[] r = gameObject.GetComponentsInChildren<Renderer>();

            foreach (Renderer v in r)
            {
                v.material.SetColor("_BorderColor", highlightColor);
            }
            HighlightApplied = true;
        }
    }
    public void RemoveHighlightColor()
    {
        Renderer[] r = gameObject.GetComponentsInChildren<Renderer>();

        foreach( Renderer v in r)
        {
            v.material.SetColor("_BorderColor", baseColor);
        }
        HighlightApplied = false;
    }
}

