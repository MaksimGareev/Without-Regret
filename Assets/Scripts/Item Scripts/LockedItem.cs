using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class LockedItem : MonoBehaviour, IInteractable, ISaveable
{
   public InteractType interactType => InteractType.Lockpick;
    public float interactionPriority => 5f;

    public float LockpickRange = 1.5f;
    private Transform player;

    public GameObject promptUI;
    public GameObject LockPickUI;
    public bool isDoor = false;

    public AudioClip UnlockSound;
    public GameObject UnlockTop;
    public bool isChest = false;
    private AudioSource audioSource;
    public bool hasBeenLockpicked = false;
    public bool isInRange = false;

    private PlayerControls controls;

    public GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;
    public ObjectiveData linkedObjective;
    public bool needsObjective = true;
    public ItemData RewardItem;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        hasBeenLockpicked = false;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
        if (LockPickUI != null)
        {
            LockPickUI.SetActive(false);
        }

        controls = new PlayerControls();
        controls.Player.Interact.performed += ctx => TryInteract();

        // Load unlock state from SaveManager
        if (SaveManager.Instance != null)
        {
            hasBeenLockpicked = SaveManager.Instance.IsUnlocked(gameObject.name);
        }

        // Disable interaction if already unlocked
        if (hasBeenLockpicked)
        {
            //DisablePopupIcon();
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
        if (hasBeenLockpicked || player == null || ButtonIcons.Instance == null) // || !player.GetComponent<Inventory>().keyItems.Any(x => x.ItemName == "Lock Pick"))
        {
            return;
        }

        float dist = Vector3.Distance(player.position, transform.position); // Players position in relation to the pick up item

        if (dist <= LockpickRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                /*if (!hasBeenLockpicked && promptUI != null)
                {
                    promptUI.SetActive(true); // Show the prompt when the player is in range
                }*/
                 Debug.Log("Trying to highlight Lockpick icon");
                 ButtonIcons.Instance.Highlight(InteractType.Lockpick);
            }
        }
        else
        {
            if (isInRange)
            {
                isInRange = false;
                /*if (promptUI != null)
                {
                    promptUI.SetActive(false);
                }*/
                ButtonIcons.Instance.Clear();
                
            }
        }

        /*
        if (shouldShowIcon && popupInstance == null && iconPrefab != null && PopupManager.Instance != null && !hasBeenLockpicked)
        {
            EnablePopupIcon();
        }
        else if (hasBeenLockpicked && popupInstance != null)
        {
            DisablePopupIcon();
        }*/
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
        if (!isInRange || LockPickUI == null || hasBeenLockpicked) return;

        // Show LockPick UI
        LockPickUI.SetActive(true);
        LockPickUI.GetComponent<LockPicking>().NewLock(this);//(this.gameObject);
        LockPickUI.GetComponent<LockPicking>().RewardItem = RewardItem;

        /*if (promptUI != null)
            promptUI.SetActive(false);
        */
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

        if (isDoor == true)
        {
            audioSource.PlayOneShot(UnlockSound);
            Destroy(gameObject);
        }
        else
        {
            // add animation for chest or drawer
        }

        ButtonIcons.Instance?.Clear();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetUnlocked(gameObject.name, true);
            SaveManager.Instance.SaveGame(SaveSystem.activeSaveSlot);
        }

        //UnlockTop.transform.position = new Vector3(0f, 95f, -23f);
        //UnlockTop.transform.rotation = new Vector3(-39f, 0f, 0f);
        if (isChest)
        {
            StartCoroutine(MoveAndRotateTop(UnlockTop, new Vector3(0f, 95f, -23f), Quaternion.Euler(-39f, 0f, 0f), 1f));
        }

        // Disable interaction
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        DisablePopupIcon();

        // Save unlock state
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetUnlocked(gameObject.name, true);
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

            if (objectiveActive)
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

    public void EnablePopupIcon()
    {
        if (popupInstance == null && iconPrefab != null && PopupManager.Instance != null)
        {
            popupInstance = PopupManager.Instance.CreatePopup(this.transform, iconPrefab).gameObject;
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

    public void SaveTo(SaveData data)
    {
        throw new System.NotImplementedException();
    }

    public void LoadFrom(SaveData data)
    {
        throw new System.NotImplementedException();
    }
}
