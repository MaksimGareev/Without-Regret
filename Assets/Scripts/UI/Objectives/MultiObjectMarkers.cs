using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiObjectMarkers : MonoBehaviour
{
    [SerializeField] List<Marker> markers;

    public void AssignMarkers(List<Transform> objects)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            markers[i].target = objects[i];
        }
    }
}
