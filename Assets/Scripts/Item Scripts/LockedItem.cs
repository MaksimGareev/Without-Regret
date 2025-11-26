using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockedItem : MonoBehaviour
{
    public float LockpickRange = 1.5f;
    private Transform player;

    public GameObject promptUI;
    public GameObject LockPickUI;
    public bool isDoor = false;

    public AudioClip UnlockSound;
    private AudioSource audioSource;
    [HideInInspector] public bool hasBeenLockpicked = false;
    private bool isInRange = false;

    private PlayerControls controls;

    public GameObject iconPrefab;
    public bool shouldShowIcon = true;
    private GameObject popupInstance;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        player = GameObject.FindGameObjectWithTag("Player").transform;
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
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position); // Players position in relation to the pick up item

        if (dist <= LockpickRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                if (promptUI != null)
                {
                    promptUI.SetActive(true); // Show the prompt when the player is in range
                }
            }
        }
        else
        {
            if (isInRange)
            {
                isInRange = false;
                if (promptUI != null)
                {
                    promptUI.SetActive(false);
                }
            }
        }

        if (shouldShowIcon && popupInstance == null)
        {
            EnablePopupIcon();
        }
        else if (!shouldShowIcon && popupInstance != null)
        {
            DisablePopupIcon();
        }
    }


    private void TryInteract()
    {
        if (!isInRange || LockPickUI == null || hasBeenLockpicked) return;

        // Show LockPick UI
        LockPickUI.SetActive(true);
        LockPickUI.GetComponent<LockPicking>().NewLock();//(this.gameObject);

        if (promptUI != null)
            promptUI.SetActive(false);

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

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }

        DisablePopupIcon();
    }

    public void EnablePopupIcon()
    {
        if (popupInstance == null)
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
}
