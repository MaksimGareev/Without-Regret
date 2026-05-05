using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerPossessing : MonoBehaviour
{
    [Header("Possession Settings")]
    [SerializeField] private float possessionDuration = 5f;
    [SerializeField] private LayerMask mask;
    
    [Tooltip("The time it takes to recharge between possessions")]
    [SerializeField] private float rechargeDelay = 1.5f;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    private InputAction possessAction;
    
    [Header("Animator")]
    public Animator animator;
    private CharacterSwap characterSwap;


    private GameObject popupInstance;
    private PlayerController playerController;
    private Rigidbody playerRigidbody;
    private PossessedEnemyResisting possessedEnemyMovement;
    private PatrollingEnemy normalEnemyMovement;
    private EnemyFieldOfView enemyPOV;
    private float possessionTimer;
    private float TimeSincePossession;
    private float rechargeSpeed = .5f;
    private PossessedEnemyResisting target = null;
    private Marker posessionIcon;
    private RaycastHit[] hit = new RaycastHit[20];
    private bool posessing = false;

    private int fov = 15;
    private int numRays = 15;

    private void Awake()
    {
        characterSwap = FindFirstObjectByType<CharacterSwap>();

        if (characterSwap != null)
        {
            animator = characterSwap.GetAnimator();

            characterSwap.onAnimatorChanged += UpdateAnimator;
        }

        playerController = GetComponent<PlayerController>();
        playerRigidbody = GetComponent<Rigidbody>();
        possessionTimer = possessionDuration;

        if (GameManager.Instance.possessionSlider != null)
        {
            GameManager.Instance.possessionSlider.value = 1;
            GameManager.Instance.possessionSlider.gameObject.SetActive(false);
            posessionIcon = GameManager.Instance.posessionIcon;

        }
        
        InitializeInputActions();
    }

    private void InitializeInputActions()
    {
        if (inputActions == null)
        {
            Debug.LogError("InputActionAsset reference is missing on " + gameObject.name);
            return;
        }
        
        possessAction = inputActions.FindAction("Possess");
        
        if (possessAction == null)
        {
            Debug.LogError("Possess action not found in InputActionAsset!");
        }
        
        possessAction?.Enable();
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0) return;
        
        if (possessAction != null && possessAction.triggered)
        {
            TryStartPossession();
        }

        //get directions around player
        var start = gameObject.transform.position;
        var forward = gameObject.transform.forward;
        var right = gameObject.transform.right;

        //get ray bounds
        float halfWidth = Mathf.Tan(fov / 2f * Mathf.Deg2Rad);
        float fullWidth = halfWidth * 2f;
        float rayStepSize = fullWidth / (float)(numRays);

        var rayStep = right * rayStepSize;

        var rayDir = forward - right * (halfWidth - rayStepSize * 0.5f);

        //goes from one angle to another and does a cast
        for (int i = 0; i < numRays; i++)
        {
            Debug.DrawRay(gameObject.transform.position, rayDir * 15f, Color.red);
            if (Physics.Raycast(gameObject.transform.position, rayDir, out hit[i], 15f, mask))
            {
                
                if (hit[i].collider.GetComponent<PossessedEnemyResisting>() != null)
                {
                    if (target != null)
                    {
                        //check if new enemy hit is closer than current target or if there is a target to begin with
                        if (Vector3.Distance(gameObject.transform.position, hit[i].collider.gameObject.transform.position) >
                            Vector3.Distance(gameObject.transform.position, target.gameObject.transform.position))
                        {
                            //if yes, set the target to the closer enemy
                            target = hit[i].collider.GetComponent<PossessedEnemyResisting>();
                            EnablePopupIcon(target.iconPoint);
                            target.ApplyHighlightColor();
                        }
                    }
                    else
                    {
                        target = hit[i].collider.GetComponent<PossessedEnemyResisting>();
                        target.ApplyHighlightColor();
                        EnablePopupIcon(target.iconPoint);
                    }
                }
            }
            
            rayDir += rayStep;
        }
        
        if (posessing != true && target != null)
        {
            CheckForClear();
        }

        if (possessedEnemyMovement != null)
        {
            possessionTimer -= Time.deltaTime;
            GameManager.Instance.possessionSlider.value = Mathf.InverseLerp(0, possessionDuration, possessionTimer);

            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
            possessedEnemyMovement.UpdatePossession(input);

            if (possessionTimer <= 0f || possessAction.WasReleasedThisFrame())
            {
                EndPossession();
            }
        }
        
        if (!posessing && possessionTimer < possessionDuration)
        {
            GameManager.Instance.possessionSlider.gameObject.SetActive(true);
            
            if (TimeSincePossession >= rechargeDelay)
            {
                possessionTimer += Time.deltaTime * rechargeSpeed;
                GameManager.Instance.possessionSlider.value = Mathf.InverseLerp(0, possessionDuration, possessionTimer);
            }
            else
            {
                TimeSincePossession += Time.deltaTime;
            }
        }
        else if (possessionTimer >= possessionDuration)
        {
            GameManager.Instance.possessionSlider.gameObject.SetActive(false);
        }
    }

    private void TryStartPossession()
    {
        if (target != null)
        {
            StartPossession(target);
        }
        else
        {
            Debug.Log("No Valid Target");
        }
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
        if (!target) return;

        posessing = true;

        animator.SetBool("isPosessing", true);
        GameManager.Instance.possessionSlider.gameObject.SetActive(true);
        normalEnemyMovement = target.GetComponent<PatrollingEnemy>();
        enemyPOV = target.GetComponent<EnemyFieldOfView>();
        possessedEnemyMovement = target;

        if (playerController)
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

        if (normalEnemyMovement)
        {
            Debug.LogError("Reference to normal enemy movement is null");
            normalEnemyMovement.enabled = false;
        }

        if (enemyPOV)
        {
            enemyPOV.enabled = false;
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
        if (possessedEnemyMovement)
        {
            possessedEnemyMovement.EndPossession();
            
            if (normalEnemyMovement)
            {
                normalEnemyMovement.enabled = true;
            }
        }
        
        animator.SetBool("isPosessing", false);

        GameManager.Instance.possessionSlider.gameObject.SetActive(false);

        if (enemyPOV)
        {
            enemyPOV.enabled = true;
        }

        if (playerController)
        {
            playerController.enabled = true;
            playerController.MovementLocked = false;
            GetComponent<CharacterController>().enabled = true;
        }
        
        posessing = false;
        TimeSincePossession = 0;
        ClearTargetInfo();
    }

    private void ClearTargetInfo()
    {
        DisablePopupIcon();
        target.RemoveHighlightColor();
        possessedEnemyMovement = null;
        normalEnemyMovement = null;
        enemyPOV = null;
        
        target = null;
    }

    private void EnablePopupIcon(Transform target)
    {
        if (posessionIcon)
        {
            posessionIcon.target = target;
        }
    }

    private void DisablePopupIcon()
    {
        if (posessionIcon)
        {
            posessionIcon.TurnOffMarker();
        }
    }

    void UpdateAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }

    private void CheckForClear()
    {
        for (int c = 0; c < hit.Length; c++)
        {
            if (hit[c].collider)
            {
                return;
            }
        }
        
        ClearTargetInfo();
        DisablePopupIcon();
    }
}
