using System.Collections.Generic;
using UnityEngine;

public class FaceExpressionCaller : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<string, int> EyeTypes = new Dictionary<string, int>
    {
        {"Default", 0},
        {"Closed", 1},
        {"Angry", 2},
        {"Tired", 3},
        {"Worried", 4},
        {"Surprised", 5},
        {"Confused", 6},
        {"Sad", 7},
    };

    [HideInInspector]
    public Dictionary<string, int> MouthTypes = new Dictionary<string, int>
    {
        {"Default", 0},
        {"Frown", 1},
        {"Sad", 2},
        {"Sadder", 3},
        {"Swiggly", 4},
        {"OpenFrown", 5},
        {"SlightFrown", 6},
        {"Smirk", 7},
    };

    [Header("Debugging")]
    [SerializeField] private bool showDebugLogs = false;

    private EyeAtlasController eyeAtlasController;
    private MouthAtlasController mouthAtlasController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eyeAtlasController = gameObject.GetComponentInChildren<EyeAtlasController>();
        mouthAtlasController = gameObject.GetComponentInChildren<MouthAtlasController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Called Face 1");
            eyeAtlasController.SetEyeIndex(0);
            mouthAtlasController.SetMouthIndex(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Called Face 2");
            eyeAtlasController.SetEyeIndex(1);
            mouthAtlasController.SetMouthIndex(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Called Face 3");
            eyeAtlasController.SetEyeIndex(2);
            mouthAtlasController.SetMouthIndex(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Called Face 4");
            eyeAtlasController.SetEyeIndex(3);
            mouthAtlasController.SetMouthIndex(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("Called Face 5");
            eyeAtlasController.SetEyeIndex(4);
            mouthAtlasController.SetMouthIndex(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Debug.Log("Called Face 6");
            eyeAtlasController.SetEyeIndex(5);
            mouthAtlasController.SetMouthIndex(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            Debug.Log("Called Face 7");
            eyeAtlasController.SetEyeIndex(6);
            mouthAtlasController.SetMouthIndex(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            Debug.Log("Called Face 8");
            eyeAtlasController.SetEyeIndex(7);
            mouthAtlasController.SetMouthIndex(7);
        }
    }

    public void SetFace(string eye, string mouth)
    {
        if (EyeTypes.ContainsKey(eye))
        {
            eyeAtlasController.SetEyeIndex(EyeTypes[eye]);
        }
        else if (showDebugLogs)
        {
            Debug.Log($"Key \"{eye}\" not found in dictionary.");
        }

        if (MouthTypes.ContainsKey(mouth))
        {
            mouthAtlasController.SetMouthIndex(MouthTypes[mouth]);
        }
        else if (showDebugLogs)
        {
            Debug.Log($"Key \"{mouth}\" not found in dictionary.");
        }
    }
}
