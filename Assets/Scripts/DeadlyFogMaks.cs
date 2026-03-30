using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DeadlyFogMaks : MonoBehaviour
{
    [Header("Reset Settings")]
    public float timeTillDamage = 2f;
    public float delayBeforeReset = 0.5f;
    public float moveDuration = 0.8f;
    public float fadedAlpha = 0.3f;
    public int amountOfRingsToSubtract = 1;
    public float triggerDisableCooldown = 1f;

    [Header("Reset Point(auto-assigned if child exists)")]
    public Transform resetPoint;

    [Header("Local Volume")]
    // from maks: reference to a local Volume used to control Post Exposure
    [SerializeField] private Volume localVolume;

    [Header("Debugging")]
    public bool showDebugLogs = false;

    private bool isResetting = false;
    private float lastResetTime = -Mathf.Infinity;
    private Collider triggerCollider;
    [SerializeField] private float timeSinceEnter = 0;
    private bool canDamage = true;

    // from maks: ColorAdjustments override from the assigned Volume 
    private ColorAdjustments colorAdjustments;
    private float exposureLerpSpeed = 2f;

    // from maks: true = darken screen, false = return exposure to 0
    private bool shouldDarken = false;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider == null)
        {
            Debug.LogError("DeadlyFogMaks requires a Collider component set as Trigger.");
            enabled = false;
            return;
        }

        if (!triggerCollider.isTrigger)
        {
            triggerCollider.isTrigger = true;
        }

        if (resetPoint == null && transform.childCount > 0)
        {
            resetPoint = transform.GetChild(0);
        }

        // from maks: if no Volume assigned manually, try to get one on this GameObject
        if (localVolume == null)
        {
            localVolume = GetComponent<Volume>();
        }

        // from maks: retrieve ColorAdjustments override so  can modify Post Exposure
        if (localVolume != null && localVolume.profile != null)
        {
            localVolume.profile.TryGet(out colorAdjustments);
        }
    }

    private void Update()
    {
        // from maks: if no ColorAdjustments exists, do nothing
        if (colorAdjustments == null) return;

        // from maks: smoothly build exposure based on time spent in fog 
        float targetExposure = 0f;

        if (timeSinceEnter > 0f && timeTillDamage > 0f)
        {
            float normalized = Mathf.Clamp01(timeSinceEnter / timeTillDamage);
            targetExposure = Mathf.Lerp(0f, -10f, normalized);
        }

        colorAdjustments.postExposure.value = targetExposure;
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (isResetting) return;
        if (Time.time - lastResetTime < triggerDisableCooldown) return;
        if (resetPoint == null) return;
        if (!other.CompareTag("Player")) return;

        if (timeSinceEnter >= timeTillDamage)
        {
            // from maks: stop darkening when damage happens
            shouldDarken = true;

            if (TimerRingUI.Instance != null && canDamage)
            {
                TimerRingUI.Instance.SubtractRingSection(amountOfRingsToSubtract);
            }
                StartCoroutine(HandleReset(player));
                canDamage = false;
        }
        else
        {
            if (timeSinceEnter >= timeTillDamage / 2f)
            {
                // from maks: start darkening screen halfway to damage
                shouldDarken = true;
            }

            timeSinceEnter += Time.deltaTime;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        timeSinceEnter = 0;

        // from maks: restore brightness when leaving fog
        if (!isResetting)
        {
            shouldDarken = false;
        }
    }

    private IEnumerator HandleReset(PlayerController player)
    {
        isResetting = true;
        lastResetTime = Time.time;

        if (triggerCollider != null)
            triggerCollider.enabled = false;

        player.SetResetLock(true);

        yield return new WaitForSeconds(delayBeforeReset);
        yield return StartCoroutine(LerpPlayerToPoint(player, resetPoint));
        yield return new WaitForSeconds(0.15f);

        shouldDarken = false;

        player.SetResetLock(false);

        float waitStart = Time.time;
        float safetyTimeout = 2f;

        while (PlayerInsideTrigger(player) && Time.time - waitStart < safetyTimeout)
        {
            yield return null;
        }

        float remaining = triggerDisableCooldown - (Time.time - lastResetTime);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        if (triggerCollider != null)
            triggerCollider.enabled = true;

        timeSinceEnter = 0;
        isResetting = false;
        canDamage = true;
    }

    private IEnumerator LerpPlayerToPoint(PlayerController player, Transform target)
    {
        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null && controller.enabled)
            controller.enabled = false;

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
            controller.enabled = true;
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
                }
            }
        }
    }

    private bool PlayerInsideTrigger(PlayerController player)
    {
        Collider triggerCol = GetComponent<Collider>();
        Collider playerCol = player.GetComponent<Collider>();
        return triggerCol.bounds.Intersects(playerCol.bounds);
    }

    private void OnDrawGizmosSelected()
    {
        if (resetPoint == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(resetPoint.position, 0.15f);
        Gizmos.DrawLine(transform.position, resetPoint.position);
    }
}