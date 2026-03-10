using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class LockedItem : MonoBehaviour, IInteractable
{
    [HideInInspector] public InteractType interactType => InteractType.Lockpick;
    [HideInInspector] public float interactionPriority => 5f;

    [Header("Lockpick Settings")]
    [Tooltip("Distance at which the player can interact with the locked item.")]
    [SerializeField] private float LockpickRange = 1.5f;
    private Transform player;

    [Header("Type of Locked Item")]
    [SerializeField] private bool isChest = true;

    [Header("Unlock VFX and SFX")]
    [Tooltip("Sound that will play when the player successfully lockpicks this item.")]
    [SerializeField] private AudioClip UnlockSound;

    [Tooltip("Object that will play an animation or effect when the item is unlocked. For example, this could be a chest lid that opens when a chest is unlocked.")]
    [SerializeField] private GameObject UnlockTop;
    private AudioSource audioSource;

    [Header("Objective and Reward Settings")]
    [Tooltip("Objective that must be ACTIVE to allow the player to interact with this locked item. If the player does not have the linked objective ACTIVE, they will not be able to interact with the locked item.")]
    [SerializeField] private ObjectiveData linkedObjective;

    [Tooltip("Whether or not the linked objective needs to be ACTIVE for the player to interact with this locked item.")]
    [SerializeField] private bool needsObjective = true;

    [Tooltip("If true, interacting with the locked item will add progress to the linked objective.")]
    [SerializeField] private bool addProgress = true;

    [Tooltip("Item that will be rewarded to the player upon successfully lockpicking this item. This is optional and can be left null if no reward is desired.")]
    [SerializeField] private ItemData RewardItem;

    [HideInInspector] public bool hasBeenLockpicked = false; // Saving and loading this value is already handled by the SaveableWorldObject component
    [HideInInspector] public bool isInRange = false;
    private PlayerControls controls;

    private bool turorialShown = false;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        hasBeenLockpicked = false;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (GameManager.Instance != null && GameManager.Instance.LockPickUI != null)
        {
            GameManager.Instance.LockPickUI.SetActive(false);
        }

        controls = new PlayerControls();
        controls.Player.Interact.performed += ctx => TryInteract();

        // Disable interaction if already unlocked
        if (hasBeenLockpicked)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            ButtonIcons.Instance?.Clear();
        }
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    // Update is called once per frame
    void Update()
    {
        if (hasBeenLockpicked || player == null || ButtonIcons.Instance == null)
        {
            return;
        }

        float dist = Vector3.Distance(player.position, transform.position); // Players position in relation to the pick up item

        if (dist <= LockpickRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                 Debug.Log("Trying to highlight Lockpick icon");
                 ButtonIcons.Instance.Highlight(InteractType.Lockpick);
            }
        }
        else
        {
            if (isInRange)
            {
                isInRange = false;
                ButtonIcons.Instance.Clear();
                
            }
        }
    }

    public bool CanInteract(GameObject player)
    {
        // Can interact only if player is in range, not lockpicked, and not mantling
        if (hasBeenLockpicked || !isInRange || player == null)
            return false;

        PlayerMantling mantling = player.GetComponent<PlayerMantling>();
        if (mantling != null && mantling.isMantling)
            return false;

        return true;
    }

    public void OnPlayerInteraction(GameObject player)
    {
        // Only try interaction if player is in range
        if (isInRange && !hasBeenLockpicked)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (!isInRange || GameManager.Instance.LockPickUI == null || hasBeenLockpicked || !player.gameObject.GetComponent<Inventory>().keyItems.Any(x => x.ItemName == "Lock Pick")) return;

        // Show tutorial for first time interaction
        if (!turorialShown && InteractionTutorialUI.Instance != null)
        {
            turorialShown = true;

            InteractionTutorialUI.Instance.ShowTutorial(
                "Rotate the lockpick into the correct position and match the correct inputs to open the locked item.",
                StartLockPick
                );
            return;
        }

        StartLockPick();
    }


    private void StartLockPick()
    {
       // if (!isInRange || GameManager.Instance.LockPickUI == null || hasBeenLockpicked || !player.gameObject.GetComponent<Inventory>().keyItems.Any(x=> x.ItemName == "Lock Pick")) return;

        // Show LockPick UI
        GameManager.Instance.LockPickUI.SetActive(true);
        GameManager.Instance.LockPickUI.GetComponent<LockPicking>().NewLock(this);//(this.gameObject);
        GameManager.Instance.LockPickUI.GetComponent<LockPicking>().RewardItem = RewardItem;
        // Disable player movement
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.MovementLocked = true;
            pc.enabled = false;
        }

        PlayerFloating pf = player.GetComponent<PlayerFloating>();
        if (pf != null)
            pf.enabled = false;
        ButtonIcons.Instance?.Clear();
    }

    public void OnUnlocked()
    {
        Debug.Log(gameObject.name + "unlocked!");
        hasBeenLockpicked = true;

        PlayerFloating playerFloating = player.GetComponent<PlayerFloating>();
        if (playerFloating != null)
        {
             playerFloating.enabled = true;
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.MovementLocked = false;
            pc.enabled = true;
        }

        // if (isDoor == true)
        // {
        //     audioSource.PlayOneShot(UnlockSound);
        //     Destroy(gameObject);
        // }

        ButtonIcons.Instance?.Clear();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        if (isChest)
        {
            StartCoroutine(MoveAndRotateTop(UnlockTop, new Vector3(0f, 95f, -23f), Quaternion.Euler(-39f, 0f, 0f), 1f));
        }

        // Disable interaction
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Save unlock state
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        if (needsObjective && linkedObjective != null && ObjectiveManager.Instance != null)
        {
            bool objectiveActive = false;

            var activeObjectives = ObjectiveManager.Instance.GetActiveObjectives();
            foreach (var obj in activeObjectives)
            {
                if (obj.data == linkedObjective)
                {
                    objectiveActive = true;
                    break;
                }
            }

            if (objectiveActive && addProgress)
            {
                ObjectiveManager.Instance.AddProgress(linkedObjective.objectiveID, 1);
            }
        }
    }

    private IEnumerator MoveAndRotateTop(GameObject target, Vector3 endPos, Quaternion endRot, float duration)
    {
        Vector3 startPos = target.transform.localPosition;
        Quaternion startRot = target.transform.localRotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // smooth movement
            target.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            target.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);

            yield return null;
        }

        target.transform.localPosition = endPos;
        target.transform.localRotation = endRot;
    }
}
