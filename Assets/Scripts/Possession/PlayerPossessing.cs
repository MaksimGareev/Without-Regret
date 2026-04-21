using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerPossessing : MonoBehaviour
{
    [Header("Possession Settings")]
    [SerializeField] private float possessionDuration = 5f;
    //[SerializeField] private float possessionRange = 50f;
    //[SerializeField] private float searchConeAngle = 30f;
    [SerializeField] private KeyCode possessKey = KeyCode.R;
    [SerializeField] private KeyCode possessButton = KeyCode.JoystickButton9;
    [SerializeField] private Vector3 iconOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private LayerMask mask;

    private GameObject popupInstance;
    private PlayerController playerController;
    private Rigidbody playerRigidbody;
    private PossessedEnemyResisting possessedEnemyMovement;
    private PatrollingEnemy normalEnemyMovement;
    private EnemyFieldOfView enemyPOV;
    //private NavMeshAgent enemyNavMeshAgent;
    private Rigidbody enemyRigidbody;
    private float possessionTimer;
    private float TimeSincePossession;
    [Tooltip("The time it takes to recharge between possessions")]
    [SerializeField] private float rechargeDelay = 1.5f;
    private float rechargeSpeed = .5f;
    [SerializeField] private PossessedEnemyResisting target = null;

    private Marker posessionIcon;
    
    RaycastHit[] hit = new RaycastHit[20];
    private bool posessing = false;
    public bool shouldShowIcon = true;

    int fov = 15;
    int numRays = 15;

    [Header("Animator")]
    public Animator animator;
    private CharacterSwap characterSwap;


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
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0) return;
        
        if (Input.GetKeyDown(possessKey))
        {
            TryStartPossession();
            Debug.Log("Tried Possessing Keyboard");
        }
        else if (Input.GetKeyDown(possessButton))
        {
            TryStartPossession();
            Debug.Log("Tried Possessing Controller");
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

                if (possessionTimer <= 0f || Input.GetKeyUp(possessKey) || Input.GetKeyUp(possessButton))
                {
                    EndPossession();
                }
        }
        if(!posessing && possessionTimer < possessionDuration)
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
        if (target == null)
        {
            return;
        }

        posessing = true;

        animator.SetBool("isPosessing", true);
        GameManager.Instance.possessionSlider.gameObject.SetActive(true);
        normalEnemyMovement = target.GetComponent<PatrollingEnemy>();
        enemyRigidbody = target.GetComponent<Rigidbody>();
        enemyPOV = target.GetComponent<EnemyFieldOfView>();
        possessedEnemyMovement = target;

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

        if (enemyPOV != null)
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
        if (possessedEnemyMovement != null)
        {
            possessedEnemyMovement.EndPossession();
            if (normalEnemyMovement != null)
            {
                normalEnemyMovement.enabled = true;
            }
        }
        animator.SetBool("isPosessing", false);

        GameManager.Instance.possessionSlider.gameObject.SetActive(false);

        if (enemyPOV != null)
        {
            enemyPOV.enabled = true;
        }

        if (playerController != null)
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

    public void EnablePopupIcon(Transform target)
    {
        if (posessionIcon != null)
        {
            posessionIcon.target = target;
        }
    }

    public void DisablePopupIcon()
    {
        if (posessionIcon != null)
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
            if (hit[c].collider != null)
            {
                return;
            }
        }
        ClearTargetInfo();
        DisablePopupIcon();
    }
}
