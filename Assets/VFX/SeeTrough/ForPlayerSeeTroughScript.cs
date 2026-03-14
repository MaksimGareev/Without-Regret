using UnityEngine;
using System.Collections.Generic;
public class ForPlayerSeeTroughScript : MonoBehaviour
{
    public static int PlayerPosID = Shader.PropertyToID("_PlayerPosition");
    public static int SizeID = Shader.PropertyToID("_Size");

    
    [SerializeField] Camera Cam;
    [SerializeField] LayerMask Mask;

    //[SerializeField] Material[] WallMaterials;
    RaycastHit[] RayList;
   

    private RaycastHit hit; //the raycast hit that returns information about the material

    void LateUpdate()
    {
        // Send player position to shader in viewport space
        var view = Cam.WorldToViewportPoint(transform.position);
        
        // Direction from player to camera
        var dir = Cam.transform.position - transform.position;

        // Ray from player toward camera
        var ray = new Ray(transform.position, dir.normalized);

        if(RayList.Length != 0)
        {
           // for(int i = 0; i<Ray)
        }
        RayList = Physics.RaycastAll(ray, 3000, Mask);

        for(int i = 0; i<RayList.Length; ++i)
        {
            hit = RayList[i];
            Renderer rend = hit.transform.GetComponent<Renderer>();

            if (rend)
            {
                rend.material.SetFloat(SizeID, 0.75f);
                rend.material.SetVector(PlayerPosID, view);
            }
        }
        
    }
}