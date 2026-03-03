using UnityEngine;

public static class InteractionTutorialText
{
    public static string GetText(InteractType type)
    {
        switch (type)
        {
            case InteractType.Move:
                return "Press the X button / E key to move larger objects around the world. Move the item to the indicator to place it, or press the X button / E key again to drop it.";

            case InteractType.Mantle:
                return "Press the A button / Spacebar to clime onto mantle-able objects.";

            case InteractType.Dialogue:
                return "Press the X button / E key to engage in dialogue with characters in the world. When dialogue choices appear, hold the directional input corresponding to a choice to select it. Your response time is limited.";

            case InteractType.Pickup:
                return "With your backpack equipped, press the X button / E key to pick up smaller objects and place them in your inventory.";

            case InteractType.Float:
                return "Floating allows you to traverse gaps in the Astral Plane. Press the A button / Spacebar next to a gap to begin floating, and press it again when the indicator is in the green area to stay in the air. You can only float for little while, and failing results in becoming grounded again.";

            default:
                return "";
        }
    }
}