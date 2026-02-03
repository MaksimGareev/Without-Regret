using System.Linq;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(PlayerController), typeof(PlayerFloating), typeof(PlayerMovingObjects))]
[RequireComponent(typeof(PlayerPossessing), typeof(PlayerThrowing), typeof(ToggleInventoryUI))]
[RequireComponent(typeof(Rigidbody), typeof(CharacterController), typeof(PlayerFishing))]
public class PlayerComponents : MonoBehaviour
{
    public static bool initialized = false;
    public static PlayerController playerController;
    public static PlayerFloating playerFloating;
    public static PlayerMovingObjects playerMovingObjects;
    public static PlayerPossessing playerPossessing;
    public static PlayerThrowing playerThrowing;
    public static PlayerFishing playerFishing;
    public static Camera playerCamera;
    public static Rigidbody rb;
    public static CharacterController characterController;
    public static ToggleInventoryUI inventoryToggle;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerFloating = GetComponent<PlayerFloating>();
        playerMovingObjects = GetComponent<PlayerMovingObjects>();
        playerPossessing = GetComponent<PlayerPossessing>();
        playerThrowing = GetComponent<PlayerThrowing>();
        playerFishing = GetComponent<PlayerFishing>();
        playerCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        inventoryToggle = GetComponent<ToggleInventoryUI>();
    }

    public static void InitializeComponents(GameObject player)
    {
        if (initialized) return;

        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("PlayerComponents: Attempting to initialize components, but player is null");
                return;
            }
        }
        playerController = player.GetComponent<PlayerController>();
        playerFloating = player.GetComponent<PlayerFloating>();
        playerMovingObjects = player.GetComponent<PlayerMovingObjects>();
        playerPossessing = player.GetComponent<PlayerPossessing>();
        playerThrowing = player.GetComponent<PlayerThrowing>();
        playerFishing = player.GetComponent<PlayerFishing>();
        playerCamera = Camera.main;
        rb = player.GetComponent<Rigidbody>();
        characterController = player.GetComponent<CharacterController>();
        inventoryToggle = player.GetComponent<ToggleInventoryUI>();

        initialized = true;
    }

    public static void SetAllComponents(bool enable, GameObject source = null)
    {
        if (!initialized)
        {
            InitializeComponents(source);
        }
        playerController.enabled = enable;
        playerFloating.enabled = enable;
        playerMovingObjects.enabled = enable;
        playerPossessing.enabled = enable;
        playerThrowing.enabled = enable;
        playerFishing.enabled = enable;
        inventoryToggle.enabled = enable;
        characterController.enabled = enable;
    }

    public static void SetComponentsExcept(bool enable, GameObject source = null, params Component[] excludeList)
    {
        // Use to toggle certain components on/off while excluding others
        if (!initialized)
        {
            InitializeComponents(source);
        }

        void ToggleComponent(Component component)
        {
            if (excludeList != null && excludeList.Contains(component))
            {
                return;
            }
            if (component != null)
            {
                if (component is MonoBehaviour mb)
                {
                    mb.enabled = enable;
                }
                else if (component is Behaviour b)
                {
                    b.enabled = enable;
                }
            }
        }

        ToggleComponent(playerController);
        ToggleComponent(playerFloating);
        ToggleComponent(playerMovingObjects);
        ToggleComponent(playerPossessing);
        ToggleComponent(playerThrowing);
        ToggleComponent(inventoryToggle);
        ToggleComponent(characterController);
        ToggleComponent(playerFishing);
    }

    public static void SetCertainComponents(bool enable, GameObject source = null, params Component[] componentsToToggle)
    {
        // Use to only toggle the specified components on/off
        if (!initialized)
        {
            InitializeComponents(source);
        }

        if (componentsToToggle == null || componentsToToggle.Length == 0)
        {
            // Nothing to toggle
            return;
        }

        foreach (var comp in componentsToToggle)
        {
            if (comp == null) continue;

            // Common Unity types with 'enabled' property
            if (comp is Behaviour behaviour)
            {
                behaviour.enabled = enable;
                continue;
            }
            if (comp is CharacterController charController)
            {
                charController.enabled = enable;
                continue;
            }
            if (comp is Collider collider)
            {
                collider.enabled = enable;
                continue;
            }

            // If we reach here, component type does not expose a common 'enabled' flag we can toggle.
            if (Debug.isDebugBuild)
            {
                Debug.LogWarning($"PlayerComponents.SetCertainComponents: Unable to toggle 'enabled' on component of type {comp.GetType().FullName}");
            }
        }
    }
}
