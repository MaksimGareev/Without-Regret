using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossEnemyController : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] int[] healthPerPart = new int[] { 3, 3, 3 };
    [SerializeField] float timeBetweenActions = 3f;
    [SerializeField, Tooltip("A delay before the boss starts acting")] float startDelay = 1.5f;

    [Header("Void Attack Settings")]
    [SerializeField, Min(0.1f)] float projectileSpeed = 5f;
    [SerializeField] VoidPoolSettings voidPoolSettings = new(5f, 1f, 1, 2, 6f);

    private Rigidbody voidProjectileRigidbody;

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject voidProjectileObject;
    [SerializeField] GameObject voidPoolPrefab;
    [SerializeField] Transform projectileSpawn;
    [SerializeField] GameObject healthPickup;

    [Header("Boss Health UI")]
    [Tooltip("Base slider prefab used to create one slider per phase at runtime.")]
    [SerializeField] private Slider baseSliderPrefab;
    [Tooltip("Container RectTransform under which the generated sliders will be placed.")]
    [SerializeField] private RectTransform slidersContainer;
    [Tooltip("Spacing in pixels between generated sliders.")]
    [SerializeField] private float sliderSpacing = 4f;
    [SerializeField] private Color activeFillColor = Color.white;
    [SerializeField] private Color inactiveFillColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);

    [Header("Debugging")]
    [SerializeField] bool showDebugLogs = false;

    private int currentPart = 1;
    private Vector3 projectileSpawnPoint;
    private Action[] actions;
    private float timeSinceLastAction = -3f;
    private bool actionInProgress = false;

    // Pools
    private ObjectPool enemyPooler;
    private ObjectPool voidPooler;

    // Runtime list of sliders used by UI logic (either generated or the fallback `phaseSliders`)
    private readonly List<Slider> healthSliders = new List<Slider>();

    private void Awake()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>().transform;
            Debug.LogWarning("Player reference for Boss Enemy is null, had to Find manually");
        }

        if (voidProjectileObject != null)
        {
            voidProjectileRigidbody = voidProjectileObject.GetComponent<Rigidbody>();
            voidProjectileObject.SetActive(false);
        }

        if (projectileSpawn != null)
        {
            projectileSpawnPoint = projectileSpawn.position;
        }

        if (enemyPrefab != null)
        {
            enemyPooler = new ObjectPool(enemyPrefab, 10, showDebugLogs, transform);
        }
        else
        {
            Debug.LogError("Enemy prefab for the void pool is missing");
        }

        if (voidPoolPrefab != null)
        {
            voidPooler = new ObjectPool(voidPoolPrefab, 3, showDebugLogs);
            voidPoolSettings.healthPickup = healthPickup;
        }

        // set up the array of actions the boss can perform
        actions = new Action[] { VoidProjectile, ArmSweep, DropPillars };

        InitializeHealthUI();
    }

    private void InitializeHealthUI()
    {
        healthSliders.Clear();

        int phases = healthPerPart != null ? healthPerPart.Length : 0;
        if (phases == 0) return;

        // Use runtime generation if both base prefab and container are provided
        if (baseSliderPrefab != null && slidersContainer != null)
        {
            // Clear existing children in the container
            for (int i = slidersContainer.childCount - 1; i >= 0; --i)
            {
                var child = slidersContainer.GetChild(i);
                Destroy(child.gameObject);
            }

            float containerWidth = slidersContainer.rect.width;
            if (containerWidth <= 0)
            {
                Debug.LogError("Sliders container has non-positive width. Cannot initialize health UI.");
                return;
            }
            float totalSpacing = sliderSpacing * (phases - 1);
            float widthPer = Mathf.Max(1f, (containerWidth - totalSpacing) / phases);

            for (int i = 0; i < phases; i++)
            {
                GameObject sliderObject = Instantiate(baseSliderPrefab.gameObject, slidersContainer);
                sliderObject.name = $"HealthBar - Phase {i + 1}";
                if (!sliderObject.TryGetComponent<Slider>(out var slider))
                {
                    Debug.LogError("Base slider prefab does not contain a Slider component.");
                    Destroy(sliderObject);
                    continue;
                }

                // Adjust RectTransform width so sliders fit neatly inside the container
                if (sliderObject.TryGetComponent<RectTransform>(out var rect))
                {
                    // Set anchors to left-center so anchoredPosition.x is measured from the left edge of the container.
                    rect.anchorMin = new Vector2(0f, 0.5f);
                    rect.anchorMax = new Vector2(0f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);

                    rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, widthPer);

                    // Position sliders horizontally with spacing so they don't overlap
                    float xPos = (widthPer * 0.5f) + i * (widthPer + sliderSpacing);
                    rect.anchoredPosition = new Vector2(xPos, 0f);
                }

                // Configure slider values
                slider.interactable = false;
                slider.maxValue = Mathf.Max(1, healthPerPart[i]);
                slider.value = Mathf.Clamp(healthPerPart[i], 0, slider.maxValue);

                healthSliders.Add(slider);

                // Set visual active state for the current phase only
                SetSliderActiveVisual(i, i == (currentPart - 1));
            }
        }
        else
        {
            Debug.LogError("Base slider prefab and/or sliders container reference is missing. Boss health UI will not be generated.");
        }
    }

    // Sets the visual appearance of a slider to indicate whether the corresponding phase is active or not
    private void SetSliderActiveVisual(int index, bool active)
    {
        if (index < 0 || index >= healthSliders.Count) return;
        Slider slider = healthSliders[index];
        if (slider == null) return;

        if (slider.fillRect.TryGetComponent<Image>(out var fillImage))
        {
            fillImage.color = active ? activeFillColor : inactiveFillColor;
        }
    }

    private void UpdateHealthUIForCurrentPhase()
    {
        int phase = currentPart - 1;
        if (phase < 0 || phase >= healthSliders.Count) return;
        Slider slider = healthSliders[phase];
        if (slider == null) return;

        slider.value = healthPerPart[phase];
    }

    private void ActivateNextPhaseUI(int previousPhaseIndex, int newPhaseIndex)
    {
        if (previousPhaseIndex >= 0 && previousPhaseIndex < healthSliders.Count)
            SetSliderActiveVisual(previousPhaseIndex, false);
        if (newPhaseIndex >= 0 && newPhaseIndex < healthSliders.Count)
            SetSliderActiveVisual(newPhaseIndex, true);
    }

    private void Update()
    {
        // Do an action every (timeBetweenActions) seconds if an action is not currently in progress
        if (!actionInProgress && Time.time > (startDelay + timeSinceLastAction + timeBetweenActions))
        {
            actionInProgress = true;
            RandomAction();

            if (startDelay > 0) startDelay = 0;
        }
    }

    void RandomAction()
    {
        // Will pick an action at random once every action is fully implemented
        /*
        int choice = UnityEngine.Random.Range(0, actions.Length);
        actions[choice]();
        */

        VoidProjectile();
    }

    void EndAction()
    {
        if (showDebugLogs) Debug.Log("Action ended. Restarting timer");

        timeSinceLastAction = Time.time;
        actionInProgress = false;
    }

    void VoidProjectile()
    {
        if (voidProjectileObject == null)
        {
            Debug.LogError("Void Projectile Prefab reference is missing.");
            return;
        }

        if (showDebugLogs) Debug.Log("Performing Void Projectile action");

        // Initialize projectile to know which pools to use
        if (!voidProjectileObject.TryGetComponent<VoidProjectile>(out var voidProjectile))
        {
            Debug.LogError("VoidProjectile component missing on projectile prefab.");
            return;
        }

        voidProjectile.Initialize(EndAction, voidPooler, enemyPooler, voidPoolSettings); // End action when projectile hits something

        // Launch the void projectile from the spawnpoint toward the player's position
        voidProjectileObject.transform.SetPositionAndRotation(projectileSpawnPoint, Quaternion.identity);
        voidProjectileObject.SetActive(true);

        if (player != null)
        {
            Vector3 origin = projectileSpawnPoint;
            Vector3 target = player.position;

            Vector3 toTarget = target - origin;
            // compute time-to-target based on horizontal distance and projectileSpeed
            Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
            float horizontalDistance = toTargetXZ.magnitude;
            float time = Mathf.Clamp(horizontalDistance / projectileSpeed, 0.25f, 3f);

            // required initial velocity:
            Vector3 initialVelocity = toTarget / time - 0.5f * time * Physics.gravity;

            // apply velocity directly so the projectile follows physics and lands near target
            voidProjectileRigidbody.linearVelocity = initialVelocity;
            voidProjectileRigidbody.angularVelocity = Vector3.zero;
        }
        else
        {
            Debug.LogError("Player reference is null.");
        }
    }

    void ArmSweep()
    {

    }

    void DropPillars()
    {

    }

    void Die()
    {
        Debug.Log("Boss' health has depleted");
        Destroy(gameObject); // Replace with death sequence later
    }

    public void TakeDamage(int value = 1)
    {
        // Take damage to the current health part
        healthPerPart[currentPart - 1] -= value;

        if (showDebugLogs) Debug.Log($"Boss took {value} damage. Current phase: {currentPart}, Current health: {healthPerPart[currentPart - 1]}");

        // Update UI for current phase
        UpdateHealthUIForCurrentPhase();

        if (healthPerPart[currentPart - 1] <= 0)
        {
            if (currentPart >= healthPerPart.Length)
            {
                // Final part has been depleted
                Die();
            }
            else
            {
                // Transition to the next part
                int previousIndex = currentPart - 1;
                currentPart++;
                int newIndex = currentPart - 1;

                // Ensure next slider max/value are set (in case inspector values differ)
                if (newIndex >= 0 && newIndex < healthSliders.Count)
                {
                    Slider next = healthSliders[newIndex];
                    if (next != null)
                    {
                        next.maxValue = Mathf.Max(1, healthPerPart[newIndex]);
                        next.value = Mathf.Clamp(healthPerPart[newIndex], 0, next.maxValue);
                    }
                }

                // Update visuals: previous greyed out, new active
                ActivateNextPhaseUI(previousIndex, newIndex);

                if (showDebugLogs) Debug.Log("Transitioned to phase " + currentPart);
            }
        }
    }
}
