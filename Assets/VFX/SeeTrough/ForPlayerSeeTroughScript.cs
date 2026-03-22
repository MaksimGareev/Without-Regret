using UnityEngine;

public class ForPlayerSeeTroughScript : MonoBehaviour
{
    public static int PlayerPosID = Shader.PropertyToID("_PlayerPosition");
    public static int SizeID = Shader.PropertyToID("_Size");

     Material WallMaterial;
    [SerializeField] Camera Cam;
    [SerializeField] LayerMask Mask;

    //[SerializeField] Material[] WallMaterials;

   

    private RaycastHit hit; //the raycast hit that returns information about the material

    void Update()
    {
        if (Cam == null)
        {
            return;
        }
        // Send player position to shader in viewport space
        var view = Cam.WorldToViewportPoint(transform.position);
        
        // Direction from player to camera
        var dir = Cam.transform.position - transform.position;

        // Ray from player toward camera
        var ray = new Ray(transform.position, dir.normalized);

        // Check if something blocks the player
        if (!Physics.Raycast(ray, out hit, 3000, Mask)){
          // Debug.Log("there's nothing in obstruction layer");
          if(WallMaterial != null)
            WallMaterial.SetFloat(SizeID, 0f);
           
           return;
        }
        else
        {
      // Debug.Log("This might be a wall! ");
          if (!hit.collider.gameObject.GetComponent<Renderer>())
          {
              return;
          }
          else
          {
              WallMaterial = hit.collider.gameObject.GetComponent<Renderer>().sharedMaterial;
           
              WallMaterial.SetFloat(SizeID, 0.75f);
              WallMaterial.SetVector(PlayerPosID, view);
          }
          
          }

        // Send player position to shader in viewport space
        
        //WallMaterial.SetVector(PlayerPosID, view);
        
        
        //WallMaterial.SetVector(PlayerPosID, view);    
        
    }
}