using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    public string Box = "Box";
    public float PickupRange;
    private Transform player;

    public GameObject promptUI;

    private bool isInRange = false;

    private PlayerControls controls;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if(promptUI != null)
        {
            promptUI.SetActive(false);
        }

        controls = new PlayerControls();

        //controls.Player.Interact.performed += ctx => Pickup();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position); // Players position in relation to the pick up item
        if(dist <= PickupRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                if(promptUI != null)
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
                if(promptUI != null)
                {
                    promptUI.SetActive(false); // Remove prompt when moving out of range
                }
            }
        }
    }

    void Pickup()
    {
        Debug.Log("Picked up" + Box);

        PlayerController pc = player.GetComponent<PlayerController>();

        if (pc != null)
        {
            pc.TriggerPickupCameraEffect(transform); // allow camera to look at object being picked up
            pc.MovementLocked = true;
        }

        if(promptUI != null)
        {
            promptUI.SetActive(false);
        }
        pc.MovementLocked = false;
        pc.enabled = true;
        promptUI.SetActive(false);
        //Destroy(gameObject, 1f); // Remove the item with a small delay
    }
}
