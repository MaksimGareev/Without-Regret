using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ForPlayerSeeTroughScript : MonoBehaviour
{
    public static int PlayerPosID = Shader.PropertyToID("_PlayerPosition");
    public static int SizeID = Shader.PropertyToID("_Size");

    
    [SerializeField] Camera Cam;
    [SerializeField] LayerMask Mask;

    //[SerializeField] Material[] WallMaterials;
    RaycastHit[] RayArray;
    RaycastHit[] CurrentObjects;
    RaycastHit[] UnSharedObjects;
   

    private RaycastHit hit; //the raycast hit that returns information about the material

    void LateUpdate()
    {
        // Send player position to shader in viewport space
        var view = Cam.WorldToViewportPoint(transform.position);
        
        // Direction from player to camera
        var dir = Cam.transform.position - transform.position;

        // Ray from player toward camera
        var ray = new Ray(transform.position, dir.normalized);

        RayArray = Physics.RaycastAll(ray, 3000, Mask);
        var unique1 = CurrentObjects.Except(RayArray);
        //UnSharedObjects = unique1.ToArray();

        if (UnSharedObjects.Length != 0)
        {
            for (int i = 0; i < UnSharedObjects.Length; ++i)
            {
                UnSharedObjects[i].transform.GetComponent<Renderer>().material.SetFloat(SizeID, 0);
            }
        }
        
        CurrentObjects = RayArray;

        for(int i = 0; i<CurrentObjects.Length; ++i)
        {
            hit = CurrentObjects[i];
            Renderer rend = hit.transform.GetComponent<Renderer>();

            if (rend)
            {
                rend.material.SetFloat(SizeID, 0.75f);
                rend.material.SetVector(PlayerPosID, view);
            }
        }
        
    }
}