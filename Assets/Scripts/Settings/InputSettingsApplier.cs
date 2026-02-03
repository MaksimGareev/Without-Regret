using UnityEngine;
using UnityEngine.InputSystem;

public class InputSettingsApplier : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private void OnEnable()
    {
        GameSettings.OnSettingsChanged += Apply;
        Apply();
    }

    private void OnDisable()
    {
        GameSettings.OnSettingsChanged -= Apply;
    }

    private void Apply()
    {
        if (inputActions == null) return;

        ApplyDeadZones(inputActions, "Player", "Move", GameSettings.LeftStickDeadZone);
        ApplyDeadZones(inputActions, "Player", "Look", GameSettings.RightStickDeadZone);
        
        ApplyDeadZones(inputActions, "UI", "Navigate", GameSettings.LeftStickDeadZone);
    }

    private void ApplyDeadZones(InputActionAsset actions, string mapName, string actionName, float deadZone)
    {
        var map = actions.FindActionMap(mapName, true);
        var action = map.FindAction(actionName, true);

        string proc = $"stickDeadzone(min={deadZone},max=1)";

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (binding.isComposite || binding.isPartOfComposite) continue;

            if (!binding.effectivePath.Contains("Gamepad")) continue;

            action.ApplyBindingOverride(i, new InputBinding { overrideProcessors = proc });
        }
    }

}