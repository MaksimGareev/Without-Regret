using UnityEngine;

public static class InteractionTutorialText
{
    public static string GetText(InteractType type)
    {
        switch (type)
        {
            case InteractType.Move:
                return "Description text for moving objects.";

            case InteractType.Mantle:
                return "Description text for mantling.";

            case InteractType.Dialogue:
                return "Description text for dialogue.";

            case InteractType.Pickup:
                return "Description text for picking up smaller items.";

            default:
                return "";
        }
    }
}
