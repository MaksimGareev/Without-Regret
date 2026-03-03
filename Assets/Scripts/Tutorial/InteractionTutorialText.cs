using UnityEngine;

public static class InteractionTutorialText
{
    public static string GetText(InteractType type)
    {
        switch (type)
        {
            case InteractType.Move:
                return "Press the X button or E key to move larger objects around the world.  Bring the item to the indicator to place the object or press the X button or E key to drop the object.";

            case InteractType.Mantle:
                return "Press the A button or Spacebar to clime onto mantleable objects.";

            case InteractType.Dialogue:
                return "Press the X button or E key to engage in dialogue with characters in the world.  When dialogue choices appear hold the directional input corresponding to the choice you wish to select.  Echo has a limited time to respond";

            case InteractType.Pickup:
                return "With your backpack equiped press the X button or E key to pickup smaller objects and put them into your inventory.";

            case InteractType.Float:
                return "Floating allows Echo to traverse gaps within the astarl plane.  Press the A button or spacebar in the green area to continue floating, echo can only float for little while and failing results becoming grounded again.";

            case InteractType.BossQTE:
                return "Match the correct inputs to send a burst of energy at the boss and damage it";

            default:
                return "";
        }
    }
}
