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

    [SerializeField] private bool showDebugLogs = false;

    private bool objectiveActive = false;
    
    private bool playedOnce;

    private void Awake()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        
        if (boxCollider && !boxCollider.isTrigger)
        {
            boxCollider.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs) Debug.Log(other.gameObject.name + " Entered " + gameObject.name);
        
        bool shouldPlay =
            (
                other.CompareTag("Player")
                && cutsceneToPlay
                && !playedOnce
                && CutsceneManager.Instance
                && (!needsObjective || (needsObjective && linkedObjective))
                && !CutsceneManager.Instance.isCutscenePlaying
            );

        if (!shouldPlay)
        {
            if (showDebugLogs)
            {
                if (playedOnce)
                {
                    Debug.LogWarning("Cutscene " + cutsceneToPlay.name + " has already been played once. It will not play again.");
                }

                if (!CutsceneManager.Instance)
                {
                    Debug.LogWarning("Cutscene Manager instance not found. Make sure there is a Cutscene Manager in the scene for cutscenes to play.");
                }

                if (CutsceneManager.Instance.isCutscenePlaying)
                {
                    Debug.LogWarning("Cutscene " + cutsceneToPlay.name + " cannot play because another cutscene is currently playing. Cutscenes cannot overlap, so please wait for the current cutscene to finish before triggering another one.");
                }

                if (!cutsceneToPlay)
                {
                    Debug.LogWarning("No cutscene assigned to " + gameObject.name + ". Please assign a cutscene to play in the inspector.");
                }

                if (needsObjective && !linkedObjective)
                {
                    Debug.LogWarning("Cutscene " + cutsceneToPlay.name + " is set to require an objective, but no linked objective has been assigned. Please assign a linked objective in the inspector.");
                }

                if (!other.CompareTag("Player"))
                {
                    Debug.LogWarning("Object " + other.gameObject.name + " entered " + gameObject.name + " but is not tagged as Player. Only objects tagged as Player can trigger cutscenes.");
                }
            }
            
            return;
        }
        
        Debug.Log("Called StartCutscene with" + cutsceneToPlay.name + " from " + gameObject.name);
        playedOnce = CutsceneManager.Instance.StartCutscene(cutsceneToPlay);
    }
}
