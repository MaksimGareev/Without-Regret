using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    CharacterController Controller;

    private bool MovementLocked = false;
    public float Speed = 1f;        
    public float SprintSpeed = 2f;
    public float SprintDuration = 3f;
    public float sprintCooldown = 4f;

    private float yVelocity = 0f;
    private float gravity = -9.81f;

    private float SprintTimer;
    private bool canSprint = true;

    public Camera PlayerCamera;  // Main Camera
    public Vector3 pickupOffset = new Vector3(3f, 2f, -5f); // Offset of camera to player when picking up an item
    public float zoomDuration = 3f; // how long the camera will be zoomed in
    public float transitionSpeed = 2f; // Speed the camera zooms in

    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private bool isZooming = false;

    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<CharacterController>();
        SprintTimer = SprintDuration;

        if(PlayerCamera != null)
        {
            originalCamPos = PlayerCamera.transform.localPosition;
            originalCamRot = PlayerCamera.transform.localRotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    void Movement()
    {
        if(Controller.isGrounded && yVelocity < 0f)
        {
            yVelocity = -2f;
        }

        if (MovementLocked == true)
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

        // Apply Gravity
        yVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = (move * currentSpeed) + new Vector3(0, yVelocity, 0);
        Controller.Move(finalMove * Time.deltaTime);

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

    IEnumerator PickupCameraRoutine(Transform item)
    {
        isZooming = true;

        // Move camera closer to player and item
        Vector3 targetPos = transform.InverseTransformPoint(item.position + (transform.forward * 1f));
        Quaternion targetRot = Quaternion.LookRotation(item.position - PlayerCamera.transform.position);

        float t = 0;
        while(t < 3f)
        {
            MovementLocked = true;
            t += Time.deltaTime * transitionSpeed;
            PlayerCamera.transform.localPosition = Vector3.Lerp(originalCamPos, targetPos + pickupOffset, t);
            PlayerCamera.transform.rotation = Quaternion.Slerp(originalCamRot, targetRot, t);
            yield return null;
        }

        // Stay zoomed in for the time duration
        yield return new WaitForSeconds(zoomDuration);
        
        // return to normal camera veiw
        isZooming = false;
        MovementLocked = false; // allow the player to move again

    }
}
