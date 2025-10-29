using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockPickUI : MonoBehaviour
{
    public RectTransform LockIndicator;
    public RectTransform InnerLock;
    public RectTransform PickCursor;
    public GameObject lockPickUI;
    private Transform player;

    public float CursorSpeed = 100f;
    public float MaxRotation = 90f;
    public float UnlockAngle = 30f;
    public float UnlockTolerance = 5f;
    public float LockSpeed = 10f;

    private float CurrentAngle = 0f;
    private bool isActive = false;

    public float ShakeDuration = 0.2f;
    public float ShakeStrength = 10f;

    private Vector3 originalPosition;

    private GameObject targetLockedItem;

    private PlayerControls controls;
    private float rotateInput;
    private float KeyPressTime = 0;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (PickCursor != null)
        {
            originalPosition = PickCursor.localPosition;
        }

        controls = new PlayerControls();

        // Rotation
        controls.LockPicking.Rotate.performed += ctx => rotateInput = ctx.ReadValue<float>();
        controls.LockPicking.Rotate.canceled += ctx => rotateInput = 0f;

        // Attempt unlock
        controls.LockPicking.Unlock.performed += ctx => TryUnlock();

        // Cancel Lockpick
        controls.LockPicking.Exit.performed += ctx => DeactivateLockPick();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        // Rotate pick cursor with horisontal input (A/D)
        float RotationAmount = -rotateInput * CursorSpeed * Time.deltaTime;

        // Update and clamp rotation
        CurrentAngle += RotationAmount;
        CurrentAngle = Mathf.Clamp(CurrentAngle, -MaxRotation, MaxRotation);

        // Apply rotation to pick cursor
        PickCursor.localEulerAngles = new Vector3(0, 0, CurrentAngle);

        // Press space to attempt unlocking
       /* if (Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("Submit"))
        {
            float angleDifference = Mathf.Abs(Mathf.DeltaAngle(PickCursor.localEulerAngles.z, UnlockAngle));

            if (angleDifference <= UnlockTolerance)
            {
                Debug.Log("Its Unlocked");
                if (targetLockedItem != null)
                {
                    LockedItem li = targetLockedItem.GetComponent<LockedItem>();
                    if (li != null)
                    {
                        li.OnUnlocked();
                    }
                    targetLockedItem = null;
                }
                DeactivateLockPick();
            }
            else
            {
                Debug.Log("Failed, try again");
                StartCoroutine(ShakePick());
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Xbox B Button"))
        {
            DeactivateLockPick();
        }*/
    }

    private void TryUnlock()
    {
        if (!isActive) return;

        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(PickCursor.localEulerAngles.z, UnlockAngle));

        if (angleDifference <= UnlockTolerance)
        {
            Debug.Log("Its Unlocked");
            if (targetLockedItem != null)
            {
                LockedItem li = targetLockedItem.GetComponent<LockedItem>();
                if (li != null)
                {
                    li.OnUnlocked();
                }
                targetLockedItem = null;
            }
            DeactivateLockPick();
        }
        else
        {
            Debug.Log("Failed, try again");
            StartCoroutine(ShakePick());
        }
        Debug.Log(angleDifference);
        Debug.Log(UnlockTolerance);
    }

    public void ActivateLockPick(GameObject item)
    {
        targetLockedItem = item;
        ActivateLockPick();
    }

    public void ActivateLockPick()
    {
        isActive = true;
        CurrentAngle = 0f;
        if (PickCursor != null)
        PickCursor.localEulerAngles = Vector3.zero;

        UnlockAngle = Random.Range(-MaxRotation, MaxRotation);  // Randomize unlocke aggle

        if (LockIndicator != null)
        {
            LockIndicator.localEulerAngles = Vector3.zero;  // Keep the lock straight up in UI
        }

        // Enable UI
        if (lockPickUI != null)
        {
            lockPickUI.SetActive(true);
        }
        Debug.Log(UnlockAngle);

    }

    public void DeactivateLockPick()
    {
        isActive = false;
        lockPickUI.SetActive(false);

        // Unlock player movement
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.MovementLocked = false;
            pc.enabled = true;
        }
    }

    private IEnumerator ShakePick()
    {
        float elapsed = 0f;

        while (elapsed < ShakeDuration)
        {
            elapsed += Time.deltaTime;

            float offsetX = Random.Range(-1f, 1f) * ShakeStrength;
            float offsetY = Random.Range(-1f, 1f) * ShakeStrength;

            PickCursor.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            yield return null;
 
        }

        PickCursor.localPosition = originalPosition;
    }

    public void RotateLock()
    {
        float Percentage = Mathf.Round(100 - Mathf.Abs((CurrentAngle - UnlockAngle) / 100) * 100);
        float LockRotation = ((Percentage / 100) * MaxRotation) * KeyPressTime;
        float MaxRotate = (Percentage / 100) * MaxRotation;

        float LockLerp = Mathf.Lerp(InnerLock.eulerAngles.z, LockRotation, Time.deltaTime * LockSpeed);
        InnerLock.eulerAngles = new Vector3(0, 0, LockLerp);
    }
}
