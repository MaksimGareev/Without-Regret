using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CutsceneTriggerVolume : MonoBehaviour
{
    [Tooltip("The cutscene Data to be played when the player enters this trigger volume")]
    [SerializeField] private CutsceneData cutsceneToPlay;

    [Tooltip("The Objective that should be ACTIVE to be able to play this cutscene")] 
    [SerializeField] private ObjectiveData linkedObjective;
    
    [Tooltip("If true, the player will need to have the linked objective active in order for the cutscene to play. If false, the cutscene will play regardless of the state of the linked objective.")]
    [SerializeField] private bool needsObjective;
    
    private bool playedOnce;
    
    private void OnTriggerEnter(Collider other)
    {
        bool shouldPlay =
            (
                other.CompareTag("Player")
                && cutsceneToPlay
                && !playedOnce
                && (linkedObjective && needsObjective)
                && CutsceneManager.Instance
            );

        if (!shouldPlay) return;
        
        playedOnce = CutsceneManager.Instance.StartCutscene(cutsceneToPlay);
    }
}
