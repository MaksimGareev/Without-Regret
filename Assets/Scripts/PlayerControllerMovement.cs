using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerMovement : MonoBehaviour
{
    InputAction move;
    InputAction look;
    Rigidbody playerRigidBody;
    Vector3 moveValue;
    [SerializeField] float scalar;
    void Start()
    {
        playerRigidBody = this.gameObject.GetComponent<Rigidbody>();
        move = InputSystem.actions.FindAction("Move");
        look = InputSystem.actions.FindAction("Look");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        moveValue = move.ReadValue<Vector2>();
        playerRigidBody.AddForce(new Vector3(moveValue.x, 0, moveValue.y)*scalar*Time.fixedDeltaTime, ForceMode.Force);
    }
}
