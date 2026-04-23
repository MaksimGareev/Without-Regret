using UnityEngine;

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
    public bool IsActive => isActive;

    public float ShakeDuration = 0.2f;
    public float ShakeStrength = 10f;

    private Vector3 originalPosition;

    private GameObject targetLockedItem;

    private PlayerControls controls;
    private Vector2 rotateInput;
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
        controls.LockPicking.Rotate.performed += ctx => rotateInput = ctx.ReadValue<Vector2>();
        controls.LockPicking.Rotate.canceled += ctx => rotateInput = Vector2.zero;

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
        //float RotationAmount = -rotateInput * CursorSpeed * Time.deltaTime;

        CurrentAngle = Mathf.Atan2(rotateInput.y, rotateInput.x) * Mathf.Rad2Deg;

        // Update and clamp rotation
        //CurrentAngle += RotationAmount;
        //CurrentAngle = Mathf.Clamp(CurrentAngle, -MaxRotation, MaxRotation);

        // Apply rotation to pick cursor
        PickCursor.rotation = Quaternion.Euler(0, 0, CurrentAngle);
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
            ShakePick();
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
            DisableOtherCanvases();
        }
        Debug.Log(UnlockAngle);

    }

    public void DeactivateLockPick()
    {
        isActive = false;
        lockPickUI.SetActive(false);
        EnableOtherCanvases();

        // Unlock player movement
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.MovementLocked = false;
            pc.enabled = true;
        }
    }
    
    private void EnableOtherCanvases()
    {
        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.journalUI != null && !GameManager.Instance.journalUICanvas.activeSelf)
        {
            GameManager.Instance.journalUICanvas.SetActive(true);
        }

        if (GameManager.Instance.interactionIconsCanvas != null && !GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(true);
        }

        if (GameManager.Instance.playerUICanvas != null && !GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(true);
        }

        if (GameManager.Instance.gameOverCanvas != null && !GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(GameOverManager.Instance.IsGameOver);
        }

        if (GameManager.Instance.objectivePanel != null && !GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(GameManager.Instance.objectiveCanvas.IsVisible());
        }
    }

    private void DisableOtherCanvases()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.journalUICanvas != null && GameManager.Instance.journalUICanvas.activeSelf)
        {
            GameManager.Instance.journalUICanvas.SetActive(false);
        }

        if (GameManager.Instance.interactionIconsCanvas != null && GameManager.Instance.interactionIconsCanvas.activeSelf)
        {
            GameManager.Instance.interactionIconsCanvas.SetActive(false);
        }

        if (GameManager.Instance.playerUICanvas != null && GameManager.Instance.playerUICanvas.activeSelf)
        {
            GameManager.Instance.playerUICanvas.SetActive(false);
        }

        if (GameManager.Instance.gameOverCanvas != null && GameManager.Instance.gameOverCanvas.activeSelf)
        {
            GameManager.Instance.gameOverCanvas.SetActive(false);
        }

        if (GameManager.Instance.objectivePanel != null && GameManager.Instance.objectivePanel.activeSelf)
        {
            GameManager.Instance.objectivePanel.SetActive(false);
        }
    }

    private void ShakePick()
    {
        float elapsed = 0f;

        while (elapsed < ShakeDuration)
        {
            elapsed += Time.deltaTime;

            float offsetX = Random.Range(-1f, 1f) * ShakeStrength;
            float offsetY = Random.Range(-1f, 1f) * ShakeStrength;

            PickCursor.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            
 
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
