using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ForPlayerSeeTroughScript : MonoBehaviour
{
    public static int PlayerPosID = Shader.PropertyToID("_PlayerPosition");
    public static int SizeID = Shader.PropertyToID("_Size");
    
    [SerializeField] Camera Cam;
    [SerializeField] LayerMask Mask;

    private float LeafSizeValue = 1.25f;
    private float OtherSizeValue = 0.75f;

    //[SerializeField] Material[] WallMaterials;
    RaycastHit[] RayArray;
    [SerializeField] RaycastHit[] CurrentObjects;
    List<RaycastHit> UnSharedObjects;
   

    private RaycastHit hit; //the raycast hit that returns information about the material

    void LateUpdate()
    {
        if (Cam != null)
        {
            // Send player position to shader in viewport space
            var view = Cam.WorldToViewportPoint(transform.position);

            // Direction from player to camera
            var dir = Cam.transform.position - transform.position;
            
            // Offset starting point to account for a sphere cast instead of raycast
            Vector3 rayOrigin = transform.position + (dir.normalized * 2.0f);

            // Ray from player toward camera
            var ray = new Ray(rayOrigin, dir.normalized);

            RayArray = Physics.SphereCastAll(ray,1.1f, 3000, Mask);

            if (CurrentObjects != null)
            {
                UnSharedObjects = GetNonSharedValues(CurrentObjects, RayArray);
            }

            if (UnSharedObjects != null)
            {
                for (int i = 0; i < UnSharedObjects.Count; ++i)
                {
                    if (UnSharedObjects[i].transform.GetComponent<Renderer>() != null)
                    {
                        UnSharedObjects[i].transform.GetComponent<Renderer>().material.SetFloat(SizeID, 0);
                    }
                }
            }

            CurrentObjects = RayArray;

            for (int i = 0; i < CurrentObjects.Length; ++i)
            {
                hit = CurrentObjects[i];
                Renderer rend = hit.transform.GetComponent<Renderer>();

                if (rend)
                {
                    if (rend.material.shader == Shader.Find("Shader Graphs/LeavesSeeTrough 1"))
                    {
                        rend.material.SetFloat(SizeID, LeafSizeValue);
                    }
                    else
                    {
                        rend.material.SetFloat(SizeID, OtherSizeValue);
                    }
                    
                    rend.material.SetVector(PlayerPosID, view);
                }
            }
        }
    }

    List<T> GetNonSharedValues<T>(T[] arr1, T[] arr2)
    {
        if (arr1.Length != 0)
        {
            // Use HashSet for O(1) lookups
            HashSet<T> set1 = new HashSet<T>(arr1);
            HashSet<T> set2 = new HashSet<T>(arr2);

            List<T> result = new List<T>();

            // Add items in arr1 not in arr2
            foreach (T item in arr1)
            {
                if (!set2.Contains(item))
                {
                    result.Add(item);
                }
            }

            // Add items in arr2 not in arr1
            foreach (T item in arr2)
            {
                if (!set1.Contains(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }
        else
        {
            return null;
        }
    }

}