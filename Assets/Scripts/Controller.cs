using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour  {
    
    [Header("Reference")]
    public Rigidbody rb;
    public Transform head;
    public Camera playerCamera;


    [Header("Config")]
    public float walkSpeed;
    public float runSpeed;

     private float verticalRotation =    0f;


    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; 
    }

    // Update is called once per frame
    void Update() {
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 2f);
    }

    void FixedUpdate() {
        Vector3 newVelocity = Vector3.up * rb.velocity.y;
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        newVelocity.x = Input.GetAxis("Horizontal") * speed;
        newVelocity.z = Input.GetAxis("Vertical") * speed;
        rb.velocity = newVelocity;
    }

    void LateUpdate() {
        verticalRotation -= Input.GetAxis("Mouse Y") * 2f;
        verticalRotation = RestrictAngle(verticalRotation, -85f, 85f);
        head.localEulerAngles = new Vector3(verticalRotation, 0, 0);


    }

    public static float RestrictAngle(float angle, float angleMin, float angleMax){
        if (angle>180)
            angle -=360;
        else if (angle < - 180)
            angle += 360;

        if (angle > angleMax)
            angle = angleMax;
        if(angle < angleMin)
            angle = angleMin;
        
        return angle;
    
    }



}
