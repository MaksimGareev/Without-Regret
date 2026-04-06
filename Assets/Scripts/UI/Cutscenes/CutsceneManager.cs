using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cutscenePanel; 
    [SerializeField] private Image backgroundImage;
    private CutsceneData currentCutscene;
    
    [HideInInspector] public bool isCutscenePlaying = false;

    public void StartCutscene(CutsceneData cutscene)
    {
        if (!cutscene)
        {
            Debug.LogWarning("No cutscene selected");
            return;
        }

        if (isCutscenePlaying)
        {
            Debug.LogWarning("Cutscene is already playing, ignoring second call to start cutscene");
            return;
        }
        
        currentCutscene = cutscene;

        if (!cutscenePanel.activeSelf)
        {
            cutscenePanel.SetActive(true);
        }
        
        PlayClip(currentCutscene.clips[0]);
        
        isCutscenePlaying = true;
    }

    private void SkipCurrentClip()
    {
        
    }

    private void PlayClip(CutsceneClip clip)
    {
        backgroundImage.sprite = clip.backgroundImage;
    }
    
    private void EndCutscene()
    {
        cutscenePanel.SetActive(false);
        
        currentCutscene = null;
        backgroundImage.sprite = null;
    }
}
