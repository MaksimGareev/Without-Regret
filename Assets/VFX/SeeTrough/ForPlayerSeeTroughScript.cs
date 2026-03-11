using UnityEngine;

public class ForPlayerSeeTroughScript : MonoBehaviour
{
    public static int PlayerPosID = Shader.PropertyToID("_PlayerPosition");
    public static int SizeID = Shader.PropertyToID("_Size");

    // [SerializeField] Material WallMaterial;
    [SerializeField] Camera Cam;
    [SerializeField] LayerMask Mask;

    [SerializeField] Material[] WallMaterials;

   

    private RaycastHit hit; //the raycast hit that returns information about the material

    void Update()
    {
        // Send player position to shader in viewport space
        var view = Cam.WorldToViewportPoint(transform.position);
        
        
       
        
        // Direction from player to camera
        var dir = Cam.transform.position - transform.position;

        // Ray from player toward camera
        var ray = new Ray(transform.position, dir.normalized);

        // Check if something blocks the player
        if (Physics.Raycast(ray, out hit, 3000, Mask)){
           
           for(int i = 0; i<WallMaterials.Length; ++i) 
            WallMaterials[i].SetFloat(SizeID, 1f);
        
        }
        else
        {
            for(int i = 0; i<WallMaterials.Length; ++i) 
                 WallMaterials[i].SetFloat(SizeID, 0f);
        }

        // Send player position to shader in viewport space
        
        //WallMaterial.SetVector(PlayerPosID, view);
        
        for(int i = 0; i<WallMaterials.Length; ++i) 
            WallMaterials[i].SetVector(PlayerPosID, view);
        
           
        
    }
}