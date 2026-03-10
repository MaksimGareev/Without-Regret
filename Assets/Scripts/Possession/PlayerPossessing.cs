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
    [SerializeField] private GameObject iconPrefab;
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
    private float rechargeDelay = 1.5f;
    private float rechargeSpeed = .5f;
    private PossessedEnemyResisting target = null;
    
    RaycastHit hit;
    private bool posessing = false;
    public bool shouldShowIcon = true;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerRigidbody = GetComponent<Rigidbody>();
        possessionTimer = possessionDuration;

        if (GameManager.Instance.possessionSlider != null)
        {
            GameManager.Instance.possessionSlider.value = 1;
            GameManager.Instance.possessionSlider.gameObject.SetActive(false);
        }
    }

    private void Update()
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

        Debug.DrawRay(gameObject.transform.position, gameObject.transform.forward*15f, Color.red);
        if (Physics.Raycast(gameObject.transform.position, gameObject.transform.forward, out hit, 15f, mask))
        {
            if (hit.collider.GetComponent<PossessedEnemyResisting>() != null)
            {
                normalEnemyMovement = hit.collider.GetComponent<PatrollingEnemy>();
                target = hit.collider.GetComponent<PossessedEnemyResisting>();
                EnablePopupIcon();
            }
        }
        else if (posessing != true && target != null)
        {
            ClearTargetInfo();
            DisablePopupIcon();
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
        possessedEnemyMovement = null;
        normalEnemyMovement = null;
        enemyPOV = null;
        target = null;
    }

    public void EnablePopupIcon()
    {
        if (popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            popupInstance = PopupManager.Instance.CreatePopup(target.transform, iconPrefab).gameObject;
            target.gameObject.GetComponent<WorldPopup>().worldOffset = iconOffset;
            shouldShowIcon = true;
        }
    }

    public void DisablePopupIcon()
    {
        if (popupInstance != null)
        {
            Destroy(popupInstance);
            popupInstance = null;
            shouldShowIcon = false;
        }
    }
}
