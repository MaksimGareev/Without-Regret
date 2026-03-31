using UnityEngine;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image backgroundImage;
    private CutsceneData currentCutscene;

    public void StartCutscene(CutsceneData cutscene)
    {
        
    }

    private void EndCutscene()
    {
        
    }

    private void SkipToNextClip()
    {
        
    }

    private void PlayClip(CutsceneClip clip)
    {
        backgroundImage.sprite = clip.backgroundImage;
    }
}
