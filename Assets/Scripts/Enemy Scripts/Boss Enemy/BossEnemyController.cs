using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossEnemyController : MonoBehaviour
{
    #region Phase Data Structure
    [Serializable, Tooltip("Stores information related to what occurs during a particular phase of the fight")]
    public class Phase
    {
        public string Name = "New Phase";
        public int Health;
        [Tooltip("A delay before initiating the first action of this phase")]
        public float DelayBeforeStarting = 1.5f;
        [Tooltip("Whether the actions in this phase should be repeated until the phase ends, or if they should just be performed once in sequence. If false, will automatically transition to the next phase after the final action is completed")]
        public bool LoopActions = false;
        public BossAction[] Actions;

        public BossAction GetNextAction(int currentIndex)
        {
            // Return null if current index is the last one and we're not looping, otherwise return the action at the current index (looping if necessary)
            if (!LoopActions && (currentIndex < 0 || currentIndex >= Actions.Length - 1))
            {
                return null;
            }

            currentIndex++;
            int index = LoopActions ? (currentIndex % Actions.Length) : currentIndex;
            return Actions[index];
        }
    }
    #endregion

    [Header("Phase Sequence Setup")]
    [Tooltip("Defines the health value and actions taken during each phase of the boss fight to be performed sequentially")]
    [SerializeField] Phase[] phases;

    [Header("Void Attack Settings")]
    [SerializeField, Min(0.1f)] float projectileSpeed = 5f;
    [Tooltip("Percentage chance that a health pickup drops when an enemy created from a void pool despawns")]
    [SerializeField, Range(0, 1f)] float healthDropChance = 0.7f;
    [Tooltip("Layer mask used for raycasting the position of the void projectile's warning shadow. Should include any layers that shouldn't obstruct the shadow's position on the ground (e.g. Target, Enemy)")]
    [SerializeField] LayerMask shadowLayerMask;
    [SerializeField] VoidPoolSettings voidPoolSettings = new(5f, 1f, 1, 2, 6f);

    private Rigidbody voidProjectileRigidbody;

    [Header("References")]
    [SerializeField] Transform player;
    [Tooltip("The enemy prefab used for spawning")]
    [SerializeField] GameObject enemyPrefab;
    [Tooltip("The projectile prefab used for the void projectile attack")]
    [SerializeField] GameObject voidProjectileObject;
    [Tooltip("A shadow object that appears on the ground to indicate where the void projectile will land")]
    [SerializeField] GameObject voidProjectileShadow;
    [Tooltip("The prefab used for the void pool created by the void projectile")]
    [SerializeField] GameObject voidPoolPrefab;
    [Tooltip("The transform from which the void projectile will be launched. Position is used, rotation is ignored.")]
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

    private int currentPhaseNumber = 1;
    private int currentActionIndex = 0;
    private Vector3 projectileSpawnPoint;
    private Action[] actions;
    private BossAction currentAction;
    private ObjectPool enemyPooler;
    private ObjectPool voidPooler;
    private Coroutine phaseStartRoutine;
    private Coroutine actionStartRoutine;

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
            voidPoolSettings.healthDropChance = healthDropChance;
        }

        if (shadowLayerMask == 0)
            shadowLayerMask = LayerMask.GetMask("Target", "Enemy", "Ignore Raycast"); // Used for raycasting the shadow position and detecting projectile collisions

        // set up the array of actions the boss can perform
        actions = new Action[] { VoidProjectile, ArmSweep, DropPillars };

        InitializeHealthUI();
    }

    private void Start()
    {
        // Start the first phase after an initial delay
        if (phases != null && phases.Length > 0 && phases[0].Actions != null && phases[0].Actions.Length > 0)
        {
            currentPhaseNumber = 1;
            currentActionIndex = 0;
            phaseStartRoutine = StartCoroutine(StartPhaseAfterDelay(phases[0]));
        }
        else
        {
            Debug.LogError("Boss phases and/or actions are not properly configured.");
        }
    }

    // Initiates the first action for the provided phase
    private void StartPhase(Phase phase)
    {
        if (phase == null || phase.Actions == null || phase.Actions.Length == 0)
        {
            Debug.LogError("Attempted to start a boss phase that is not properly configured.");
            return;
        }

        currentActionIndex = 0;
        currentAction = phase.Actions[currentActionIndex];
        currentAction.Initiate(this, showDebugLogs);

        if (showDebugLogs) Debug.Log($"Started phase {currentPhaseNumber} and initiated action {currentAction.Name}");
    }

    // Sets up the next phase by activating the appropriate UI and starting the first action of the next phase
    public void StartNextPhase()
    {
        if (phaseStartRoutine != null)
        {
            StopCoroutine(phaseStartRoutine);
            phaseStartRoutine = null;
        }
        if (actionStartRoutine != null)
        {
            StopCoroutine(actionStartRoutine);
            actionStartRoutine = null;
        }

        int nextPhaseIndex = currentPhaseNumber; // currentPhaseNumber is 1-indexed, so next phase index is the same as currentPhaseNumber
        currentPhaseNumber++;
        if (nextPhaseIndex < 0 || nextPhaseIndex >= phases.Length)
        {
            Debug.LogError("Attempted to start a boss phase that is out of range.");
            return;
        }

        // Ensure next slider max/value are set (in case inspector values differ)
        if (nextPhaseIndex >= 0 && nextPhaseIndex < healthSliders.Count)
        {
            Slider next = healthSliders[nextPhaseIndex];
            if (next != null)
            {
                next.maxValue = Mathf.Max(1, phases[nextPhaseIndex].Health);
                next.value = Mathf.Clamp(phases[nextPhaseIndex].Health, 0, next.maxValue);
            }
        }

        // Update visuals: previous greyed out, new active
        ActivateNextPhaseUI(nextPhaseIndex - 1, nextPhaseIndex);

        if (showDebugLogs) Debug.Log("Transitioning to phase " + currentPhaseNumber);
        phaseStartRoutine = StartCoroutine(StartPhaseAfterDelay(phases[nextPhaseIndex]));
    }

    private void InitializeHealthUI()
    {
        healthSliders.Clear();

        int parts = phases != null ? phases.Length : 0;
        if (parts == 0) return;

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
            float totalSpacing = sliderSpacing * (parts - 1);
            float widthPer = Mathf.Max(1f, (containerWidth - totalSpacing) / parts);

            for (int i = 0; i < parts; i++)
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
                slider.maxValue = Mathf.Max(1, phases[i].Health);
                slider.value = Mathf.Clamp(phases[i].Health, 0, slider.maxValue);

                healthSliders.Add(slider);

                // Set visual active state for the current phase only
                SetSliderActiveVisual(i, i == (currentPhaseNumber - 1));
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
        int phaseIndex = currentPhaseNumber - 1;
        if (phaseIndex < 0 || phaseIndex >= healthSliders.Count) return;
        Slider slider = healthSliders[phaseIndex];
        if (slider == null) return;

        slider.value = phases[phaseIndex].Health;
    }

    private void ActivateNextPhaseUI(int previousPhaseIndex, int newPhaseIndex)
    {
        if (previousPhaseIndex >= 0 && previousPhaseIndex < healthSliders.Count)
            SetSliderActiveVisual(previousPhaseIndex, false);
        if (newPhaseIndex >= 0 && newPhaseIndex < healthSliders.Count)
            SetSliderActiveVisual(newPhaseIndex, true);
    }

    // Called by a BossAction when initiated
    public void PerformAction(BossActionType actionType)
    {
        switch (actionType)
        {
            case BossActionType.VoidProjectile:
                VoidProjectile();
                break;
            case BossActionType.ArmSweep:
                ArmSweep();
                break;
            case BossActionType.DropPillars:
                DropPillars();
                break;
            case BossActionType.Random:
                RandomAction();
                break;
            default:
                Debug.LogWarning("Attempted to perform undefined boss action: " + actionType);
                break;
        }
    }

    void RandomAction()
    {
        // Pick an action at random
        int choice = UnityEngine.Random.Range(0, actions.Length);
        actions[choice]();
    }

    void EndAction()
    {
        if (showDebugLogs) Debug.Log("Action ended. Going to next action if possible");

        if (voidProjectileShadow != null)
        {
            voidProjectileShadow.SetActive(false);
        }

        // Call the current action's FinishAction to trigger any events tied to the end of the action
        currentAction?.FinishAction(showDebugLogs);

        // Get the next action for the current phase, if there is one, and initiate it
        BossAction nextAction = phases[currentPhaseNumber - 1].GetNextAction(currentActionIndex);
        if (nextAction != null)
        {
            if (showDebugLogs) Debug.Log("Next action in phase " + currentPhaseNumber + " is " + nextAction.Name + ". Initiating after delay of " + nextAction.DelayUntilNextAction + " seconds.");
            actionStartRoutine = StartCoroutine(InitiateActionAfterDelay(nextAction));
        }
        else 
        {
            // Reached the final action in the phase, attempt to transition to next phase
            int nextPhaseIndex = currentPhaseNumber; // currentPhase is 1-indexed, so next phase index is the same as currentPhase
            if (nextPhaseIndex < 0 || nextPhaseIndex >= phases.Length)
            {
                if (showDebugLogs) Debug.Log("No more phases or actions to go through.");
                Die();
                return;
            }
            if (showDebugLogs) Debug.Log("Phase " + currentPhaseNumber + " has no more actions remaining. Transitioning to phase " + (currentPhaseNumber + 1));
            StartNextPhase();
        }
    }

    IEnumerator StartPhaseAfterDelay(Phase phase)
    {
        yield return new WaitForSeconds(phase.DelayBeforeStarting);

        StartPhase(phase);
    }

    IEnumerator InitiateActionAfterDelay(BossAction nextAction)
    {
        yield return new WaitForSeconds(currentAction.DelayUntilNextAction);

        if (showDebugLogs) Debug.Log($"Initiating action {nextAction.Name} of Phase {currentPhaseNumber}");
        currentActionIndex++;
        currentAction = nextAction;
        currentAction.Initiate(this, showDebugLogs);
    }

    #region Void Projectile
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

        voidProjectile.Initialize(EndAction, voidPooler, enemyPooler, voidPoolSettings, showDebugLogs); // End action when projectile hits something

        // Launch the void projectile from the spawnpoint toward the player's position
        voidProjectileObject.transform.SetPositionAndRotation(projectileSpawnPoint, Quaternion.identity);
        voidProjectileObject.SetActive(true);

        if (player != null)
        {
            Vector3 origin = projectileSpawnPoint;
            Vector3 target = player.position;

            if (voidProjectileShadow != null)
            {
                // Position the shadow on the ground at the target location
                if (Physics.Raycast(target + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, ~shadowLayerMask))
                {
                    voidProjectileShadow.transform.position = hit.point + Vector3.up * 0.01f; // Slightly above ground to avoid z-fighting
                    voidProjectileShadow.SetActive(true);
                }
                else
                {
                    voidProjectileShadow.SetActive(false);
                }
            }

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
    #endregion

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
        Phase currentPhase = phases[currentPhaseNumber - 1];
        currentPhase.Health -= value;

        if (showDebugLogs) Debug.Log($"Boss took {value} damage. Current phase: {currentPhaseNumber}, Current health: {currentPhase.Health}");

        // Update UI for current phase
        UpdateHealthUIForCurrentPhase();

        if (currentPhase.Health <= 0)
        {
            if (currentPhaseNumber >= phases.Length)
            {
                // Final part has been depleted
                Die();
            }
            else
            {
                // Transition to the next phase
                StartNextPhase();
            }
        }
    }

    // Used to change how many enemies are spawned from void pools created by the void projectile
    public void SetNumEnemiesToSpawn(int minValue, int maxValue)
    {
        voidPoolSettings.minEnemiesToSpawn = minValue;
        voidPoolSettings.maxEnemiesToSpawn = maxValue;
    }
}

