using UnityEngine;
using System.Collections;

public class DeadlyFog : MonoBehaviour
{
    [Header("Reset Settings")]
    [Tooltip("Time in seconds the player can move around in the fog before being dealt damage")]
    public float timeTillDamage = 2f;

    [Tooltip("Time in seconds to wait after the player enters the fog before starting to move them to the reset point.")]
    public float delayBeforeReset = 0.5f;

    [Tooltip("Duration in seconds for the player to be moved from their current position to the reset point. During this time, the player's movement will be locked and they will be faded out.")]
    public float moveDuration = 0.8f;

    [Tooltip("Alpha value to set on the player's materials during the reset movement. This will make the player appear faded out while they are being moved to the reset point.")]
    public float fadedAlpha = 0.3f;

    [Tooltip("Number of rings to subtract from the player when they fall and trigger a reset. This will cause the Timer Ring UI to update and visually show the player losing \"Health\".")]
    public int amountOfRingsToSubtract = 1;
    [Tooltip("Cooldown time in seconds after a reset during which the trigger will be disabled to prevent multiple rapid resets if the player is still within the trigger area.")]
    public float triggerDisableCooldown = 1f;

    [Header("Reset Point(auto-assigned if child exists)")]
    public Transform resetPoint;

    [Header("Animator")]
    public Animator animator;
    private CharacterSwap characterSwap;


    [Header("Debugging")]
    [Tooltip("If true, debug logs will be printed to the console regarding this script. This can be helpful for troubleshooting and ensuring the reset logic is working as intended, but should be left false when not needed.")]
    public bool showDebugLogs = false;

    private bool isResetting = false;
    private float lastResetTime = -Mathf.Infinity;
    private Collider triggerCollider;
    [SerializeField] private float timeSinceEnter = 0;

    private void Awake()
    {
        characterSwap = FindObjectOfType<CharacterSwap>();

        if (characterSwap != null)
        {
            animator = characterSwap.GetAnimator();

            characterSwap.onAnimatorChanged += UpdateAnimator;
        }

        triggerCollider = GetComponent<Collider>();

        if (triggerCollider == null)
        {
            Debug.LogError("FallResetTrigger requires a Collider component set as Trigger.");
            enabled = false;
            return;
        }

        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning("Collider is not set as Trigger. Setting isTrigger to true.");
            triggerCollider.isTrigger = true;
        }

        if (resetPoint == null && transform.childCount > 0)
        {
            resetPoint = transform.GetChild(0);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        animator = player.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("FogDamage");
        }

        if (player == null)
        {
            return;
        }

        if (isResetting)
        {
            if (showDebugLogs)
            {
                Debug.Log($"{name}: Reset already in progress. Ignoring trigger.");
            }
            return;
        }

        if (Time.time - lastResetTime < triggerDisableCooldown)
        {
            if (showDebugLogs)
            {
                Debug.Log($"{name}: Trigger is on cooldown. Ignoring trigger.");
            }
            return;
        }

        if (resetPoint == null)
        {
            Debug.LogError($"{name}: No reset point assigned for DeadlyFog.");
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (timeSinceEnter >= timeTillDamage)
        {
            if(TimerRingUI.Instance != null)
            {
                TimerRingUI.Instance.SubtractRingSection(amountOfRingsToSubtract);
            }
            StartCoroutine(HandleReset(player));
        }
        else
        {
            if (timeSinceEnter >= timeTillDamage / 2)//currently set up to trigger half way through the player's allowed time in the fog, but can be modified here
            {
                //start disorienting effect here
            }
            timeSinceEnter += Time.deltaTime;
            return;
        }
        
    }

    private void OnTriggerExit(Collider other)
    {

        timeSinceEnter = 0;
    }

    private IEnumerator HandleReset(PlayerController player)
    {
        isResetting = true;
        lastResetTime = Time.time;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        player.SetResetLock(true);

        yield return new WaitForSeconds(delayBeforeReset);

        yield return StartCoroutine(LerpPlayerToPoint(player, resetPoint));

        yield return new WaitForSeconds(0.15f);

        player.SetResetLock(false);

        float waitStart = Time.time;
        float safetyTimeout = 2f;

        while (PlayerInsideTrigger(player) && Time.time - waitStart < safetyTimeout)
        {
            if (showDebugLogs)
            {
                Debug.Log($"{name}: Player still inside trigger after reset. Waiting to exit...");
            }
            yield return null;
        }

        float remaining = triggerDisableCooldown - (Time.time - lastResetTime);

        if (remaining > 0f)
        {
            yield return new WaitForSeconds(remaining);
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }

        if (showDebugLogs)
        {
            Debug.Log($"{name}: Reset sequence complete. Trigger re-enabled.");
        }
        timeSinceEnter = 0;
        isResetting = false;
    }

    private IEnumerator LerpPlayerToPoint(PlayerController player, Transform target)
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        bool controllerWasEnabled = controller == null ? false : controller.enabled;

        if (controller != null && controller.enabled)
        {
            controller.enabled = false;
        }

        Transform playerTransform = player.transform;
        Vector3 startPosition = playerTransform.position;
        Quaternion startRotation = playerTransform.rotation;

        Vector3 endPosition = target.position;
        Quaternion endRotation = target.rotation;

        SetPlayerAlpha(player, fadedAlpha);

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            playerTransform.position = Vector3.Lerp(startPosition, endPosition, t);
            playerTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = endPosition;
        playerTransform.rotation = endRotation;

        SetPlayerAlpha(player, 1f);

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private void SetPlayerAlpha(PlayerController player, float alpha)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = alpha;
                    mat.color = color;

                    if (alpha < 1f)
                    {
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    }
                    else
                    {
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        mat.SetInt("_ZWrite", 1);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.DisableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = -1;
                    }
                }
            }
        }
    }

    private bool PlayerInsideTrigger(PlayerController player)
    {
        Collider triggerCollider = GetComponent<Collider>();
        Collider playerCollider = player.GetComponent<Collider>();

        return triggerCollider.bounds.Intersects(playerCollider.bounds);
    }

    private void OnDrawGizmosSelected()
    {
        if (resetPoint == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(resetPoint.position, 0.15f);
        Gizmos.DrawLine(transform.position, resetPoint.position);
    }
    void UpdateAnimator(Animator newAnimator)
    {
        animator = newAnimator;
    }

}
