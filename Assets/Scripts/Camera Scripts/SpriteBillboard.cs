using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [SerializeField] private Camera theCamera;

    void Start()
    {
        theCamera = Camera.main;
    }
    void LateUpdate()
    {
        transform.LookAt(theCamera.transform);

        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
    }
}
