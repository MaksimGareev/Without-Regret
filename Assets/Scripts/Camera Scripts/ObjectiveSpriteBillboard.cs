using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveSpriteBillboard : MonoBehaviour
{
    [SerializeField] private Camera theCamera;
    void LateUpdate()
    {
        if (theCamera != null)
        {
            transform.LookAt(theCamera.transform);

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        else
        {
            FindCamera();
        }
    }

    public void FindCamera()
    {
        theCamera = Camera.main;
    }
}
