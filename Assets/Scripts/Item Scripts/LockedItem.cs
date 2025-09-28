using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedItem : MonoBehaviour
{
    public float PickupRange;
    private Transform player;

    public GameObject promptUI;
    public GameObject LockPickUI;
    public bool isDoor = false;


    private bool isInRange = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
        if (LockPickUI != null)
        {
            LockPickUI.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position); // Players position in relation to the pick up item

        if (dist <= PickupRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                if (promptUI != null)
                {
                    promptUI.SetActive(true); // Show the prompt when the player is in range
                }
            }
            if (Input.GetKeyDown(KeyCode.E) && LockPickUI != null)
            {
                LockPickUI.SetActive(true);
                LockPickUI.GetComponent<LockPickUI>().ActivateLockPick(this.gameObject);
                promptUI.SetActive(false);

                PlayerController pc = player.GetComponent<PlayerController>();
                if (pc != null)
                    pc.MovementLocked = true;
            }
        }
        else
        {
            if (isInRange)
            {
                isInRange = false;
                if (promptUI != null)
                {
                    promptUI.SetActive(false); // Remove prompt when moving out of range
                }
                if (LockPickUI != null)
                {
                    LockPickUI.SetActive(false);
                }
            }
        }
    }

    public void OnUnlocked()
    {
        Debug.Log(gameObject.name + "unlocked!");

        if (isDoor == true)
        {
            Destroy(gameObject);
        }
        else
        {
            // add animation for chest or drawer
        }
    }
}
