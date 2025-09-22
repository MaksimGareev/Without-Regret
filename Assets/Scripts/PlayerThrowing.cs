using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrowing : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Transform throwOrigin;
    [SerializeField] private float throwforce = 10f;
    [SerializeField] private float upwardForce = 2f;
    [SerializeField] private Camera playerCamera;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }
    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryThrowItem();
        }
    }

    private void TryThrowItem()
    {
        foreach (var item in inventory.GetItems())
        {
            if (item.ItemType == ItemType.ThrowableItem && item.Prefab != null)
            {
                Throw(item);
                inventory.RemoveItem(item);
                break;
            }
        }
    }

    private void Throw(ItemData item)
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 target = hit.point;
            Vector3 direction = (target - throwOrigin.position).normalized;

            GameObject thrownObj = Instantiate(item.Prefab, throwOrigin.position, Quaternion.identity);

            Rigidbody rigidbody = thrownObj.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = thrownObj.AddComponent<Rigidbody>();
            }

            Vector3 force = direction * throwforce + Vector3.up * upwardForce;
            rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }
}
