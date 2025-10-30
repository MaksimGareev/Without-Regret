using UnityEngine;

public class FountainCollectable : MonoBehaviour
{
    /*
    When this object is hit by water
    push this object which hopefully goes into the bottom well so that the player can grab it
    Have it float on the surface of the water
    */

    private Rigidbody collectableRigidBody;


    void Awake()
    {
        collectableRigidBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            collectableRigidBody.AddForce(Vector3.forward, ForceMode.Impulse);
        }
    }
    
}
