using UnityEngine;

[RequireComponent(typeof(PlayerController), typeof(PlayerFloating), typeof(PlayerMovingObjects))]
[RequireComponent(typeof(PlayerPossessing), typeof(PlayerThrowing), typeof(ToggleInventoryUI))]
[RequireComponent(typeof(Rigidbody), typeof(CharacterController))]
public class PlayerComponents : MonoBehaviour
{
    public PlayerController playerController;
    public PlayerFloating playerFloating;
    public PlayerMovingObjects playerMovingObjects;
    public PlayerPossessing playerPossessing;
    public PlayerThrowing playerThrowing;
    public Camera playerCamera;
    public Rigidbody rb;
    public CharacterController controller;
    public ToggleInventoryUI inventoryToggle;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerFloating = GetComponent<PlayerFloating>();
        playerMovingObjects = GetComponent<PlayerMovingObjects>();
        playerPossessing = GetComponent<PlayerPossessing>();
        playerThrowing = GetComponent<PlayerThrowing>();
        playerCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        inventoryToggle = GetComponent<ToggleInventoryUI>();
    }

    public void SetComponents(bool enable)
    {
        playerController.enabled = enable;
        playerFloating.enabled = enable;
        playerMovingObjects.enabled = enable;
        playerPossessing.enabled = enable;
        playerThrowing.enabled = enable;
        inventoryToggle.enabled = enable;
        controller.enabled = enable;
    }
}
