using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedItem : MonoBehaviour
{
    public float PickupRange;
    private Transform player;

    public GameObject promptUI;

    private bool isInRange = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (promptUI != null)
        {
            promptUI.SetActive(false);
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
            if (Input.GetKeyDown(KeyCode.E))
            {
                
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
            }
        }
    }
}
