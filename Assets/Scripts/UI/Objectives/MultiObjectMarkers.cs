using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultiObjectMarkers : MonoBehaviour
{
    [SerializeField] List<Marker> markers;

    public void AssignMarkers(List<Transform> objects)
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogWarning("No objects provided to assign to markers.");
            return;
        }

        if (markers == null || markers.Count == 0)
        {
            Debug.LogWarning("No markers configured on MultiObjectMarkers.");
            return;
        }

        int assignCount = Mathf.Min(markers.Count, objects.Count);
        if (objects.Count > markers.Count)
        {
            Debug.LogWarning($"Received {objects.Count} objects but only {markers.Count} marker slots exist. Extra targets will be ignored.");
        }
        
        for (int i = 0; i < assignCount; i++)
        {
            Marker marker = markers[i];
            if (marker == null)
            {
                Debug.LogWarning($"Marker slot {i} is unassigned in MultiObjectMarkers.");
                continue;
            }

            marker.target = objects[i];
            // string targetName = objects[i] ? objects[i].name : "<null target>";
            // Debug.Log(targetName + " assigned to marker " + i);
        }

        for (int i = assignCount; i < markers.Count; i++)
        {
            if (markers[i] != null)
            {
                markers[i].TurnOffMarker();
            }
        }
    }
}
