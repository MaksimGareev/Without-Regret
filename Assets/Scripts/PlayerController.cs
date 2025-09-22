using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    CharacterController Controller;
    public float Speed = 1f;        
    public float SprintSpeed = 2f;
    public float SprintDuration = 3f;
    public float sprintCooldown = 4f;

    private float SprintTimer;
    private bool canSprint = true;

    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponent<CharacterController>();
        SprintTimer = SprintDuration;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    void Movement()
    {
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

        // Move the Player
        Controller.Move(move * currentSpeed * Time.deltaTime);

        // Sprint cooldown
        System.Collections.IEnumerator SprintCooldown()
        {
            yield return new WaitForSeconds(sprintCooldown);
            SprintTimer = SprintDuration;
            canSprint = true;
            Debug.Log("Player can sprint again");
        }

        // Rotate the player to face the way they are moving
        if(move.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }
}
