using UnityEngine;

public class LockPicking : MonoBehaviour
{
    public Camera cam;
    public RectTransform InnerLock;
    public RectTransform PickCursor;
    public GameObject LockPickUi;
    public float MaxAngle = 90;
    public float LockSpeed = 10;
    public float CursorSpeed = 100f;
    private float CurrentAngle = 0f;
    public float RotationAmount;

    [Min(1)]
    [Range(1, 25)]
    public float LockRange = 10;

    private float EulerAngle;
    private float UnlockAngle;
    private Vector2 UnlockRange;
    private LockedItem currentLockedItem;

    private float KeyPressTime = 0;
    private Vector3 originalPosition;
    private bool MovePick = true;
    private Transform player;
    private PlayerControls controls;
    private Vector2 rotateInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        PickCursor.eulerAngles = new Vector3(0, 0, 0);
    }
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
        controls.LockPicking.Unlock.canceled += ctx => CancelUnlock();

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
        //Debug.Log(UnlockRange);
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (MovePick == true)
        {
            // Rotate pick cursor with horisontal input (A/D)
            //RotationAmount = -rotateInput * CursorSpeed * Time.deltaTime;
            CurrentAngle = Mathf.Atan2(rotateInput.y, rotateInput.x) * Mathf.Rad2Deg;

            // Update and clamp rotation
            //CurrentAngle += RotationAmount;
            //CurrentAngle = Mathf.Clamp(CurrentAngle, -MaxAngle, MaxAngle);

            // Apply rotation to pick cursor
            PickCursor.localEulerAngles = new Vector3(0, 0, CurrentAngle-90);
        }

        KeyPressTime = Mathf.Clamp(KeyPressTime, 0, 1);

        float Percentage = Mathf.Round(100 - Mathf.Abs((CurrentAngle - UnlockAngle) / 100) * 100);

        if (Percentage <= 0)
        {
            Percentage = 1;
        }

        float LockRotation = ((Percentage / 100) * MaxAngle) * KeyPressTime;
        float MaxRotation = (Percentage / 100) * MaxAngle;

        float LockLerp = Mathf.Lerp(InnerLock.eulerAngles.z, LockRotation, Time.deltaTime * LockSpeed);
        InnerLock.eulerAngles = new Vector3(0, 0, LockLerp);

        if (MovePick == false)
        {
            PickCursor.eulerAngles = new Vector3(0, 0, CurrentAngle + LockLerp);
        }

        //Debug.Log(Percentage);
        //PickCursor.eulerAngles = new Vector3(0, 0, LockLerp);

        if (LockLerp >= MaxRotation - 1)
        {
            if(CurrentAngle < UnlockRange.y && CurrentAngle > UnlockRange.x)
            {
                Debug.Log("Unlocked");
                // NewLock();

                MovePick = true;
                KeyPressTime = 0;
                LockPickUi.SetActive(false);

                // Unlock player movement
                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.MovementLocked = false;
                    pc.enabled = true;
                }

                if (currentLockedItem != null)
                {
                    currentLockedItem.OnUnlocked();
                    currentLockedItem = null;
                }
            }
            else
            {
                //PickCursor.eulerAngles = new Vector3(0, 0, LockLerp);
                float RandomRotation = Random.insideUnitCircle.x;
                PickCursor.eulerAngles += new Vector3(0, 0, Random.Range(-RandomRotation, RandomRotation));
            }

        }
    }

    public void TryUnlock()
    {
        MovePick = false;
        KeyPressTime = 1;
    }

    public void CancelUnlock()
    {
        MovePick = true;
        KeyPressTime = 0;
    }

    public void DeactivateLockPick()
    {
        LockPickUi.SetActive(false);
        Rigidbody rb = player.GetComponent<Rigidbody>();

        // Unlock player movement
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.MovementLocked = false;
            pc.enabled = true;
        }
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

    }

    public void NewLock(LockedItem lockedItem)
    {
        currentLockedItem = lockedItem;
        LockPickUi.SetActive(true);
        Rigidbody rb = player.GetComponent<Rigidbody>();
        PickCursor.eulerAngles = new Vector3(0, 0, 0);
        UnlockAngle = Random.Range(-MaxAngle + LockRange, MaxAngle - LockRange);
        UnlockRange = new Vector2(UnlockAngle - LockRange, UnlockAngle + LockRange);
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
