using UnityEngine;

public class FaceExpressionCaller : MonoBehaviour
{
    private EyeAtlasController eyeAtlasController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eyeAtlasController = gameObject.GetComponentInChildren<EyeAtlasController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Called Face 1");
            eyeAtlasController.SetEyeIndex(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Called Face 2");
            eyeAtlasController.SetEyeIndex(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Called Face 3");
            eyeAtlasController.SetEyeIndex(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Called Face 4");
            eyeAtlasController.SetEyeIndex(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("Called Face 5");
            eyeAtlasController.SetEyeIndex(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Debug.Log("Called Face 6");
            eyeAtlasController.SetEyeIndex(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Debug.Log("Called Face 7");
            eyeAtlasController.SetEyeIndex(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Debug.Log("Called Face 8");
            eyeAtlasController.SetEyeIndex(7);
        }

    }
}
