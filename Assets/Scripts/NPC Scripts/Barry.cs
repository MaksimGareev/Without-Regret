using UnityEngine;

public class Barry : MonoBehaviour
{
    public float Speed = 3f;      // movement speed
    public float RotationSpeed = 3f;    // how fast the NPC rotates

    public Transform targetSpot;
    public Transform lookAtTarget;
    public bool isTraveling;
    public bool arrived = false;
    public float stopDistance = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isTraveling)
        {
            TravelToTarget();
        }
        /*else if (arrived && lookAtTarget != null)
        {
            LookAtObject();
        }*/
    }

    public void StartTravel()
    {
        //IsFollowing = false;
        isTraveling = true;
        Debug.Log("Barry is now traveling to her destination");
    }

    public void TravelToTarget()
    {
        if (targetSpot == null)
        {
            return;
        }

        Vector3 direction = targetSpot.position - transform.position;
        direction.y = 0f;

        // Movement
        transform.position = Vector3.MoveTowards(transform.position, targetSpot.position, Speed * Time.deltaTime);

        // Rotate towards target
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
        }

        // Stop when close to target destination
        if (Vector3.Distance(transform.position, targetSpot.position) < stopDistance)
        {
            isTraveling = false;
            arrived = true;
            Debug.Log("Irene reached the destination.");
        }
    }
}
