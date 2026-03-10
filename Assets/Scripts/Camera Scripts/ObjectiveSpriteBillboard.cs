using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveSpriteBillboard : MonoBehaviour
{
    [SerializeField] private Camera theCamera;

    void Start()
    {
        theCamera = Camera.main;
    }
    void LateUpdate()
    {
        transform.LookAt(theCamera.transform);

        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    public void FindCamera()
    {
        theCamera = Camera.main;
    }
}
