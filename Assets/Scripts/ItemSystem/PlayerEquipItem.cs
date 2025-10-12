using UnityEngine;

public class PlayerEquipItem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void EquipItem(int slotIndex)
    {
        Debug.Log($"Equipping item from slot {slotIndex}");
    }
}
