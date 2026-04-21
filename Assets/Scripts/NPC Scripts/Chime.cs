using UnityEngine;

public class Chime : MonoBehaviour
{
    [SerializeField] private bool followPlayer = true;
    [SerializeField] private Transform player;
    [SerializeField] private bool facePlayer = true;
    [SerializeField] private float lookSmooth = 8f;
    [SerializeField] private float bobHeight = .5f;
    [SerializeField] private float bobSpeed = 2f;
    [Tooltip("Smoothing speed for following the player")]
    [SerializeField] private float followSmooth = 10f;

    [Header("Orbiting")]
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float orbitSpeed = 2f;

    private bool followPlayerCached = false;
    private float wayfindSpeed = 5f;
    private bool isInObjectiveMarkerMode = false;
    public bool IsGuiding => isInObjectiveMarkerMode;

    // When guiding, objectiveTargetPosition stores the world position Chime should move to
    private Vector3 objectiveTargetPosition = Vector3.zero;

    private float orbitAngle;
    //private Transform OrbitPivot;
    //private Transform BobObject;

    public static bool isInDialogue = false;

    [Header("Animator")]
    public Animator animator;

    private void Awake()
    {
        if (player == null)
        {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
            else Debug.LogError("Chime: Player not found in scene. Chime will not function without player reference.", this);
        }
    }

    private void Start()
    {
        followPlayerCached = followPlayer;
    }

    void LateUpdate()
    {
        if (player == null && !isInObjectiveMarkerMode) return;

        Vector3 targetPos = transform.position;

        if (isInObjectiveMarkerMode)
        {
            // When in objective marker mode, Chime goes to the objective's position (with bobbing)
            Vector3 bob = new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobHeight, 0f);
            targetPos = objectiveTargetPosition + bob;

            // Keep smoothing to avoid teleportation
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * wayfindSpeed);

            // Optionally still face the player while at the objective
            if (facePlayer && player != null)
            {
                Vector3 lookPoint = player.position;
                lookPoint.y = transform.position.y; // keep chime level
                Vector3 dir = lookPoint - transform.position;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * lookSmooth);
                }
            }

            return;
        }

        if (followPlayer && !isInDialogue)
        {
            // Orbit angle increases steadily
            orbitAngle += orbitSpeed * Time.deltaTime;
            if (orbitAngle > Mathf.PI * 2f) orbitAngle -= Mathf.PI * 2f;

            // Calculate orbit position relative to player
            Vector3 offset = new Vector3(Mathf.Cos(orbitAngle) * orbitRadius, Mathf.Sin(Time.time * bobSpeed) * bobHeight + 1f, Mathf.Sin(orbitAngle) * orbitRadius);

            targetPos = player.position + offset;
        }
        else if (isInDialogue)
        {
            // Dialogue Mode
            Vector3 dialogueOffset = player.right * 1.2f + new Vector3(0f, 1f, 0f);
            Vector3 bob = new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobHeight, 0f);

            targetPos = player.position + player.forward * 1.5f + dialogueOffset + bob;
        }

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSmooth);

        // smoothly rotate toward player
        if (facePlayer)
        {
            // look at players horizontal position only
            Vector3 lookPoint = player.position;
            lookPoint.y = transform.position.y; // keep chime level

            Vector3 dir = lookPoint - transform.position;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * lookSmooth);
            }
        }
    }

    // Called by ObjectiveMarker when an objective with chimeMovesToMarker is activated.
    // Chime will move to the supplied world position and remain there until ReturnToPlayer() is called.
    public void GoToMarker(Vector3 newPosition, float speed)
    {
        isInObjectiveMarkerMode = true;
        objectiveTargetPosition = newPosition;
        wayfindSpeed = speed;

        // temporarily stop following the player while guiding    
        followPlayer = false;
    }

    // Return Chime to normal following behavior
    public void ReturnToPlayer()
    {
        isInObjectiveMarkerMode = false;
        followPlayer = followPlayerCached;
    }

    public void SetFollow(bool shouldFollow)
    {
        followPlayer = shouldFollow;
        followPlayerCached = shouldFollow;
    }

    //Chime's Animation functions

    public void SetIdleAnimation()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);
    }
    public void setSpecialIdleAnimation()
    {
        animator.SetBool("isInSpecialIdle", true);
        animator.SetTrigger("specialIdle");
    }

    public void SetWalkingAnimation()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);
    }

    public void setFloatingAnimation()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isFloating", true);
    }


    public void ResetChimeAnimations()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isFloating", false);
    }
}
