using UnityEngine;
using System.Collections;

public class FallResetTrigger : MonoBehaviour
{
    [Header("Reset Settings")]
    [SerializeField, Tooltip("Time in seconds to wait after the player enters the trigger before starting to move them to the reset point.")]
    private float delayBeforeReset = 0.5f;
    [SerializeField, Tooltip("Duration in seconds for the player to be moved from their current position to the reset point. During this time, the player's movement will be locked and they will be faded out.")]
    private float moveDuration = 0.8f;
    [SerializeField, Tooltip("Alpha value to set on the player's materials during the reset movement. This will make the player appear faded out while they are being moved to the reset point.")]
    private float fadedAlpha = 0.3f;
    [SerializeField, Tooltip("Number of rings to subtract from the player when they fall and trigger a reset. This will cause the Timer Ring UI to update and visually show the player losing \"Health\".")]
    private int amountOfRingsToSubtract = 1;
    [SerializeField, Tooltip("Cooldown time in seconds after a reset during which the trigger will be disabled to prevent multiple rapid resets if the player is still within the trigger area.")]
    private float triggerDisableCooldown = 1f;
    [SerializeField, Header("Reset Point(auto-assigned if child exists)")]
    private Transform resetPoint;
    
    [Header("Debugging")]
    [Tooltip("If true, debug logs will be printed to the console regarding this script. This can be helpful for troubleshooting and ensuring the reset logic is working as intended, but should be left false when not needed.")]
    public bool showDebugLogs = false;

    private bool isResetting = false;
    private float lastResetTime = -Mathf.Infinity;
    private Collider triggerCollider;

    private void Awake()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerController>(out var player))
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
            Debug.LogError($"{name}: No reset point assigned for FallResetTrigger.");
            return;
        }

        if (showDebugLogs)
        {
            Debug.Log($"{name}: Player entered reset trigger. Starting reset sequence.");
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (TimerRingUI.Instance != null)
        {
            TimerRingUI.Instance.SubtractRingSection(amountOfRingsToSubtract);
        }
        
        StartCoroutine(HandleReset(player));
    }

    private IEnumerator HandleReset(PlayerController player)
    {
        isResetting = true;
        lastResetTime = Time.time;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
        // Notify PlayerController to lock movement
        player.SetResetLock(true);

        yield return new WaitForSeconds(delayBeforeReset);
        // Wait until player moves to the resetPoint
        yield return StartCoroutine(LerpPlayerToPoint(player, resetPoint));
        
        yield return new WaitForSeconds(0.15f);
        // Reset has completed, wait for timeout to ensure player is outside of trigger
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

        isResetting = false;
    }

    private IEnumerator LerpPlayerToPoint(PlayerController player, Transform target)
    {
        // Smoothly move player to the reset point
        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null && controller.enabled)
        {
            controller.enabled = false;
        }

        Transform playerTransform = player.transform;
        playerTransform.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);
        target.GetPositionAndRotation(out Vector3 endPosition, out Quaternion endRotation);
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

        playerTransform.SetPositionAndRotation(endPosition, endRotation);
        SetPlayerAlpha(player, 1f);

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private void SetPlayerAlpha(PlayerController player, float alpha)
    {
        // Make the player appear more transparent
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
        // Check if the player is within the trigger collider
        Collider triggerCollider = GetComponent<Collider>();
        Collider playerCollider = player.GetComponent<Collider>();

        return triggerCollider.bounds.Intersects(playerCollider.bounds);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the collider box and direction to the resetPoint in editor
        if (resetPoint == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(resetPoint.position, 0.15f);
        Gizmos.DrawLine(transform.position, resetPoint.position);
    }
}
