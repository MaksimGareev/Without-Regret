using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    private Camera theCamera;
    // Update is called once per frame
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
