using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    CharacterController Controller;
    // Movement Variables
    public bool MovementLocked = false;    // locking the players movement when they interact with an item or NPC
    public float Speed = 1f;        
    public float SprintSpeed = 2f;  // Sprint Speed
    public float SprintDuration = 3f;   // How long the player can sprint
    public float sprintCooldown = 4f;   // How long it takes for the player to sprint again
    private float SprintTimer;
    private bool canSprint = true; // Can the player sprint

    // Extra variables to keep the player to the ground when interacting with items or NPC
    private float yVelocity = 0f;
    private float gravity = -9.81f; // gravity for when the player is locked so they don't fly away

    // Camera Variables
    public Camera PlayerCamera;  // Main Camera
    public Vector3 pickupOffset = new Vector3(3f, 2f, -5f); // Offset of camera to player when picking up an item
    public float zoomDuration = 3f; // how long the camera will be zoomed in
    public float transitionSpeed = 2f; // Speed the camera zooms in
    private bool isZooming = false; // is the camera zoomed in

    public static bool DialogueActive = false;

    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<CharacterController>();   // Find the Character controller
        SprintTimer = SprintDuration;   // Set sprint timer to sprint duration

    }

    // Update is called once per frame
    void Update()
    {
        if (DialogueActive == true)
        {
            return;
        }
            Movement();
    }

    void Movement()
    {
        if(Controller.isGrounded && yVelocity < 0f) // is the player on the ground
        {
            yVelocity = -2f;
        }

        if (MovementLocked == true) // keeping the player on the ground when locked in place
        {
            Controller.Move(new Vector3(0, yVelocity, 0) * Time.deltaTime);
            return;
        }

        // Get input axes of player
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Movement vector
        Vector3 move = new Vector3(x, 0f, z);
        float currentSpeed = Speed;

        // Check if the player is sprinting
        if (Input.GetKey(KeyCode.LeftShift) && canSprint)
        {
            if (SprintTimer > 0f) // if timer is greater than 0 the player can sprint
            {
                currentSpeed = SprintSpeed;
                SprintTimer -= Time.deltaTime;
                Debug.Log("player is sprinting");
            }
            else                  // if timer is 0 or less the player cannot sprint
            {
                canSprint = false;
                currentSpeed = Speed;
                StartCoroutine(SprintCooldown());
                Debug.Log("player cannot sprint any more");
            }
        }
        // if player is not holding shift the sprint timer will increase to sprint again
        else if (!Input.GetKey(KeyCode.LeftShift))
        {
            if (SprintTimer < SprintDuration)
            {
                SprintTimer += Time.deltaTime;
            }
        }

        // Apply Gravity to when player movement is locked
        yVelocity += gravity * Time.deltaTime;

        // Sprint cooldown
        IEnumerator SprintCooldown()
        {
            yield return new WaitForSeconds(sprintCooldown);
            SprintTimer = SprintDuration;
            canSprint = true;
            Debug.Log("Player can sprint again");
        }

        // Move the Player
        Controller.Move(move * currentSpeed * Time.deltaTime);

        // Rotate the player to face the way they are moving
        if(move.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    // called when an item is picked up by the player
    public void TriggerPickupCameraEffect(Transform item)
    {
        if(!isZooming && PlayerCamera != null)
        {
            StartCoroutine(PickupCameraRoutine(item));
        }
    }

    // Adjusting the camera and locking player movement when the player picks up an item
    IEnumerator PickupCameraRoutine(Transform item)
    {
        isZooming = true;

        // Position and rotation of the camera following the player
        Vector3 originalCamPos = PlayerCamera.transform.position;
        Quaternion originalCamRot = PlayerCamera.transform.rotation;

        // Position and rotation of the targeted item / NPC
        Vector3 targetPos = item.position + (transform.forward * 1f) + pickupOffset;
        Quaternion targetRot = Quaternion.LookRotation(item.position - PlayerCamera.transform.position);

        // Move camera to the offset position after picking up an item
        float t = 0;
        MovementLocked = true;
        while (t < 3f)
        {
            t += Time.deltaTime * transitionSpeed;
            PlayerCamera.transform.position = Vector3.Lerp(originalCamPos, targetPos + pickupOffset, t);
            PlayerCamera.transform.rotation = Quaternion.Slerp(originalCamRot, targetRot, t);
            yield return null;
        }

        MovementLocked = false; // allow the player to move again

        // Stay zoomed in for the time duration
        yield return new WaitForSeconds(zoomDuration);

        MovementLocked = false; // allow the player to move again
        // return to normal camera veiw
        isZooming = false;


    }
}
