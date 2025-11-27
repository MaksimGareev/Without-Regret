using UnityEngine;
using System.Collections;

public class FallResetTrigger : MonoBehaviour
{
    [Header("Reset Settings")]
    public float delayBeforeReset = 0.5f;
    public float moveDuration = 0.8f;
    public float fadedAlpha = 0.3f;

    [Header("Reset Point(auto-assigned if child exists)")]
    public Transform resetPoint;
    public float triggerDisableCooldown = 1f;
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
        PlayerController player = other.GetComponent<PlayerController>();

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
}
