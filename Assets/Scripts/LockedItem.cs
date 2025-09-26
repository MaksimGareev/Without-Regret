using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedItem : MonoBehaviour
{
    private Transform player;
    public GameObject promptUI;
    public float UnlockRange = 3f;

    public GameObject LockUIPrefab;     // The Canvas prefab containing LockFace + PickAnchor
    private GameObject activeLockUI;    // Instance of the UI when spawned
    public GameObject LockPickPrefab;
    private bool isUnlocked = false;
    private bool isInRange = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        if (isUnlocked) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= UnlockRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                if (promptUI != null)
                    promptUI.SetActive(true);
            }

            // Player presses E to start lockpicking
            if (Input.GetKeyDown(KeyCode.E))
            {
                OpenLockUI();
            }
        }
        else
        {
            if (isInRange)
            {
                isInRange = false;
                if (promptUI != null)
                    promptUI.SetActive(false);
            }
        }
    }

    private void OpenLockUI()
    {
        if (LockUIPrefab == null || activeLockUI != null) return;

        // Spawn the UI prefab (world space Canvas)
        activeLockUI = Instantiate(LockUIPrefab, Vector3.zero, Quaternion.identity);

        // Position it in front of the camera
        UnityEngine.Camera cam = UnityEngine.Camera.main;
        if (cam != null)
        {
            activeLockUI.transform.position = cam.transform.position + cam.transform.forward * 2f;
            activeLockUI.transform.rotation = cam.transform.rotation;
        }

        // Find the PickAnchor in the UI
        Transform pickAnchor = activeLockUI.transform.Find("PickAnchor");
        if (pickAnchor == null)
        {
            Debug.LogError("PickAnchor not found in LockUIPrefab!");
            return;
        }

        // Spawn the 3D lockpick model at the anchor
        GameObject lockPick3D = Instantiate(LockPickPrefab);
        lockPick3D.transform.position = pickAnchor.position;
        lockPick3D.transform.rotation = pickAnchor.rotation;

        // Assign LockPick script variables
        LockPick lockPick = lockPick3D.GetComponent<LockPick>();
        if (lockPick != null)
        {
            lockPick.Cam = cam;
            lockPick.PickPosition = pickAnchor;
            lockPick.OnUnlock += HandleUnlock;
        }
        else
        {
            Debug.LogError("LockPick script missing on LockPickModel prefab!");
        }

        // Disable player movement while lockpicking
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.enabled = false;

        // Hide the prompt
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void HandleUnlock()
    {
        isUnlocked = true;

        // Destroy UI and 3D lockpick
        if (activeLockUI != null)
            Destroy(activeLockUI);

        Debug.Log($"{gameObject.name} unlocked");

        // Re-enable player movement
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
            pc.enabled = true;
    }
}