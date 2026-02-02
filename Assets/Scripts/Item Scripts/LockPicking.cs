using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class LockPicking : MonoBehaviour
{
    public Camera cam;
    public RectTransform InnerLock;
    public RectTransform PickCursor;
    public GameObject LockPickUi;
    public GameObject StageTwoUI;

    public float MaxAngle = 90;
    public float LockSpeed = 10;
    public float CursorSpeed = 100f;
    private float CurrentAngle = 0f;
    public float RotationAmount;
    public List<Sprite> ArrowImages;
    public List<int> DirectionAssignments; //0 = up, 1 = left, 2 = down, 3 = right in terms of layout on the d-pad and arrow keys
    public List<RawImage> Arrows;

    [Min(1)]
    [Range(1, 25)]
    public float LockRange = 10;

    public AudioSource Source;
    public AudioClip UnlockSound;
    public AudioClip FailSound;

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
    private int KeyBoardInputValue;
    public ItemData RewardItem;
    private bool SecondStageActive = false;
    private Rigidbody rb;
    private int ArrowIndex = 0;
    private bool ControlsLocked;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Source.volume = 300f;
        PickCursor.eulerAngles = new Vector3(0, 0, 0);
        for (int i = 0; i < Arrows.Count; i++)//assigns sprites to the ui images based off directional inputs given
        {
            if (DirectionAssignments[i] == 0)
            {
                Arrows[i].texture = ArrowImages[0].texture;
            }
            if (DirectionAssignments[i] == 1)
            {
                Arrows[i].texture = ArrowImages[1].texture;
            }
            if (DirectionAssignments[i] == 2)
            {
                Arrows[i].texture = ArrowImages[2].texture;
            }
            if (DirectionAssignments[i] == 3)
            {
                Arrows[i].texture = ArrowImages[3].texture;
            }
        }
        StageTwoUI.SetActive(false);
    }
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = player.GetComponent<Rigidbody>();
        if (PickCursor != null)
        {
            originalPosition = PickCursor.localPosition;
        }

        controls = new PlayerControls();

        // Rotation With GamePad
        controls.LockPicking.Rotate.performed += ctx => rotateInput = ctx.ReadValue<Vector2>();
        controls.LockPicking.Rotate.canceled += ctx => rotateInput = Vector2.zero;

        // Rotation with KeyBoard
        controls.LockPicking.RotateKeyboardLeft.performed += ctx => KeyBoardInputValue = -1;
        controls.LockPicking.RotateKeyboardRight.performed += ctx => KeyBoardInputValue = 1;

        controls.LockPicking.RotateKeyboardLeft.canceled += ctx => KeyBoardInputValue = 0;
        controls.LockPicking.RotateKeyboardRight.canceled += ctx => KeyBoardInputValue = 0;

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
        if (!SecondStageActive)// if second stage isn't active, recieves rotation inputs
        {
            if (MovePick == true)
            {
                if (rotateInput.magnitude > 0)//if receiving controller stick input, uses this method
                {
                    // Rotate pick cursor with horisontal input (A/D)
                    //RotationAmount = -rotateInput * CursorSpeed * Time.deltaTime;
                    CurrentAngle = Mathf.Atan2(rotateInput.y, rotateInput.x) * Mathf.Rad2Deg;

                    // Update and clamp rotation
                    //CurrentAngle += RotationAmount;
                    //CurrentAngle = Mathf.Clamp(CurrentAngle, -MaxAngle, MaxAngle);

                    // Apply rotation to pick cursor
                    PickCursor.localEulerAngles = new Vector3(0, 0, CurrentAngle - 90);
                }
                else if (KeyBoardInputValue != 0)// if detecting A and D input, uses this method
                {
                    RotationAmount = -KeyBoardInputValue * CursorSpeed * Time.deltaTime;
                    CurrentAngle += RotationAmount;
                    PickCursor.localEulerAngles = new Vector3(0, 0, CurrentAngle - 90);
                }
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
                PickCursor.eulerAngles = new Vector3(0, 0, CurrentAngle - 90 + LockLerp);
            }

            //Debug.Log(Percentage);
            //PickCursor.eulerAngles = new Vector3(0, 0, LockLerp);

            if (LockLerp >= MaxRotation - 1)
            {
                if (CurrentAngle < UnlockRange.y && CurrentAngle > UnlockRange.x)
                {
                    // NewLock();
                    Source.Stop();
                    SecondStageActive = true;
                    StageTwoUI.SetActive(true);//switches controls to stage two, locks pick rotation
                }
                else if (MovePick == false)
                {
                    //PickCursor.eulerAngles = new Vector3(0, 0, LockLerp);
                    float RandomRotation = Random.insideUnitCircle.x;
                    PickCursor.eulerAngles += new Vector3(0, 0, Random.Range(-RandomRotation, RandomRotation));
                    if (!Source.isPlaying)
                    {
                        Source.PlayOneShot(FailSound);
                    }
                }

            }
        }
        else if (SecondStageActive)
        {
            if (ArrowIndex < DirectionAssignments.Count && !ControlsLocked)
            {
                if (controls.LockPicking.ArrowUp.triggered)
                {
                    CheckDirection(0);
                }
                else if (controls.LockPicking.ArrowRight.triggered)
                {
                    CheckDirection(1);
                }
                else if (controls.LockPicking.ArrowDown.triggered)
                {
                    CheckDirection(2);
                }
                else if (controls.LockPicking.ArrowLeft.triggered)
                {
                    CheckDirection(3);   
                }
            }
            else if (ArrowIndex >= DirectionAssignments.Count)
            {
                Unlock();
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
        if (!SecondStageActive)
        {
            PickCursor.localEulerAngles = new Vector3(0, 0, CurrentAngle - 90);
        }
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

    public void Unlock()//resets and deactivates Ui
    {
        if (!Source.isPlaying)
        {
            Source.PlayOneShot(UnlockSound);
        }
        MovePick = true;
        KeyPressTime = 0;
        ArrowIndex = 0;
        StageTwoUI.SetActive(false);
        LockPickUi.SetActive(false);

        if (RewardItem != null)//if thing being unlocked has a reward, put it in the player's inventory
        {
            player.GetComponent<Inventory>().AddItem(RewardItem);
            RewardItem = null;
        }

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

    private void CheckDirection(int input)
    {
        if (input == DirectionAssignments[ArrowIndex])
        {
            Arrows[ArrowIndex].gameObject.SetActive(false);
            ArrowIndex++;
        }
        else
        {
            ArrowIndex = 0;
            ControlsLocked = true;
            Source.PlayOneShot(FailSound);
            StartCoroutine(WrongDirection());
            
        }
    }

    IEnumerator WrongDirection()
    {
        for (int i = 0; i < Arrows.Count; i++)
        {
            Arrows[i].color = Color.red;
        }
        yield return new WaitForSecondsRealtime(.25f);
        for (int i = 0; i < Arrows.Count; i++)
        {
            Arrows[i].gameObject.SetActive(true);
            Arrows[i].color = Color.white;
        }
        ControlsLocked = false;

    }
}
