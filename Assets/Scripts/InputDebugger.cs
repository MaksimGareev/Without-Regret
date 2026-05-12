using UnityEngine;
using UnityEngine.InputSystem;

public class InputDebugger : MonoBehaviour
{
    private void Start()
    {
        foreach (var device in InputSystem.devices)
        {
            Debug.Log($"Device: {device.displayName}");
        }
    }
    
    void Update()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad gamepad)
            {
                Vector2 leftStick = gamepad.leftStick.ReadValue();

                if (leftStick.magnitude > 0.1f)
                {
                    Debug.Log($"INPUT FROM DEVICE: {device.displayName} : {leftStick}");
                }
            }
        }
    }
}
