using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockPickUI : MonoBehaviour
{
    public RectTransform LockIndicator;
    public RectTransform PickCursor;
    public GameObject lockPickUI;
    private Transform player;

    public float CursorSpeed = 100f;
    public float MaxRotation = 90f;
    public float UnlockAngle = 30f;
    public float UnlockTolerance = 5f;

    private float CurrentAngle = 0f;
    private bool isActive = false;

    public float ShakeDuration = 0.2f;
    public float ShakeStrength = 10f;

    private Vector3 originalPosition;

    private GameObject targetLockedItem;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (PickCursor != null)
        {
            originalPosition = PickCursor.localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActive) return;

        // Rotate pick cursor with horisontal input (A/D)
        float input = Input.GetAxis("Horizontal");
        float RotationAmount = -input * CursorSpeed * Time.deltaTime;
        //PickCursor.Rotate(0, 0, -input * CursorSpeed * Time.deltaTime);

        // Update and clamp rotation
        CurrentAngle += RotationAmount;
        CurrentAngle = Mathf.Clamp(CurrentAngle, -MaxRotation, MaxRotation);

        // Apply rotation to pick cursor
        PickCursor.localEulerAngles = new Vector3(0, 0, CurrentAngle);


        // Press space to attempt unlocking
        if (Input.GetKeyDown(KeyCode.Space))
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeactivateLockPick();
        }
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
        PickCursor.localEulerAngles = Vector3.zero;

        UnlockAngle = Random.Range(-MaxRotation, MaxRotation);  // Randomize unlocke aggle

        if (LockIndicator != null)
        {
            LockIndicator.localEulerAngles = Vector3.zero;  // Keep the lock straight up in UI
        }

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
}
