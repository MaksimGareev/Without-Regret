using UnityEngine;

public class PlayerMantling : MonoBehaviour
{
    [Header("Mantling Settings")]
    //[SerializeField] private float mantleRange = 2f;
    [SerializeField] private float mantleSpeed = 6f;
    
    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    [Header("Animator")]
    public Animator animator;

    private CharacterController controller;
    private PlayerController playerController;
    private PlayerFloating playerFloating;
    private PlayerMovingObjects playerMovingObjects;
    private PlayerPossessing playerPossessing;
    private PlayerThrowing playerThrowing;

    public bool isMantling = false;
    private Vector3 mantleStartPos;
    private Vector3 mantleEndPos;
    private float mantleProgress = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        playerFloating = GetComponent<PlayerFloating>();
        playerMovingObjects = GetComponent<PlayerMovingObjects>();
        playerPossessing = GetComponent<PlayerPossessing>();
        playerThrowing = GetComponent<PlayerThrowing>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMantling)
        {
            PerformMantle();
            return;
        }   
    }

    public void StartMantle(MantleableObject point)
    {
        isMantling = true;
        if (animator)
            animator.SetBool("isMantling", true);
        mantleStartPos = transform.position;
        mantleEndPos = point.GetMantlePosition();
        mantleProgress = 0f;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerFloating != null)
        {
            playerFloating.enabled = false;
        }

        if (playerMovingObjects != null)
        {
            playerMovingObjects.enabled = false;
        }

        if (playerPossessing != null)
        {
            playerPossessing.enabled = false;
        }

        if (playerThrowing != null)
        {
            playerThrowing.enabled = false;
        }

        if (controller != null)
        {
            controller.enabled = false;
        }
    }

    private void PerformMantle()
    {
        mantleProgress += Time.deltaTime * mantleSpeed;
        if (mantleProgress < 1)
        {
            transform.position = Vector3.Lerp(mantleStartPos, new Vector3(mantleStartPos.x, mantleEndPos.y, mantleStartPos.z), mantleProgress);
        }
        else if (mantleProgress >= 1)
        {
            transform.position = Vector3.Lerp(new Vector3(mantleStartPos.x, mantleEndPos.y, mantleStartPos.z), mantleEndPos, mantleProgress-1);
        }
        if (mantleProgress >= 2f)
        {
            EndMantle();
        }
    }

    private void EndMantle()
    {
        if (showDebugLogs)
        {
            Debug.Log("Mantle complete!");
        }

        isMantling = false;

        if (animator)
            animator.SetBool("isMantling", false);


        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (playerFloating != null)
        {
            playerFloating.enabled = true;
        }

        if (playerMovingObjects != null)
        {
            playerMovingObjects.enabled = true;
        }

        if (playerPossessing != null)
        {
            playerPossessing.enabled = true;
        }

        if (playerThrowing != null)
        {
            playerThrowing.enabled = true;
        }

        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}
