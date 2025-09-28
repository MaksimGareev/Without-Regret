using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraRotate : MonoBehaviour
{
    InputAction RotateCamera;
    [SerializeField] GameObject cameraPivot;
    void Start()
    {
        RotateCamera = InputSystem.actions.FindAction("RotateCamera");
       //Debug.Log(RotateCamera.name);
       // Debug.Log(cameraPivot.name);

    }

    // Update is called once per frame
    void Update()
    {
       // Vector3 cameraRotateValue = RotateCamera.ReadValue<Vector2>(); //read the input of the control
        if(RotateCamera.WasPressedThisFrame())
            cameraPivot.transform.Rotate(0,90,0, Space.World);
    }
}
