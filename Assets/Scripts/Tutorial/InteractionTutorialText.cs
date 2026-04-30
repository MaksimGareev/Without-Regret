using UnityEngine;

public static class InteractionTutorialText
{
    public static string GetText(InteractType type)
    {
        switch (type)
        {
            case InteractType.Move:
                return "Press the X button / E key to move larger objects around the world. Echo can freely place items. Astral projections indicate specific locations for larger items to be placed.  Echo can also mantle on top of objects pressing the A button / Spacebar.";

            case InteractType.Mantle:
                return "Press the A button / Spacebar to climb onto mantle-able objects.";

            case InteractType.Dialogue:
                return "Press the X button / E key to engage in dialogue with characters in the world. When dialogue choices appear, hold the directional input corresponding to a choice to select it. Your response time is limited, pay attention to the timer displaying your response time or else a random choice will be selected.";

            case InteractType.Pickup:
                return "With your backpack equipped, press Y / I key to open up your inventory.  When picking up smaller and throwable items they will be added to your inventory.  Throwable items can be equipped by pressing A / Left mouse click to select them.";

            case InteractType.Float:
                return "Floating allows you to traverse gaps in the Astral Plane. Press the A button / Spacebar next to a gap to begin floating, when the blue line is within the green area, press the A button / Spacebar to continue your float. You can only float for little while, and failing results in becoming grounded again. Make sure to be careful when crossing large gaps.";

            case InteractType.BossQTE:
                return "Match the correct inputs to send a burst of energy at the boss and damage it";

            case InteractType.Collectable:
                return "Collectible journal pages can be found throughout the world and have different entries based upon current morality when they are picked up.  Press the Select button / tab key to open your journal and navigate to the collectible section to view them.";

            default:
                return "";
        }
    }
}