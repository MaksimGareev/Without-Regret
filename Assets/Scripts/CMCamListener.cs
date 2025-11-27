using UnityEngine;
using Cinemachine;

public class CMCamListener : MonoBehaviour
{
    private PlayerController player;
    private CinemachineBrain brain;

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerController>();
        brain = FindAnyObjectByType<CinemachineBrain>();

        brain.m_CameraActivatedEvent.AddListener(OnCameraActivated);
        //brain.m_CameraCutEvent.AddListener(OnCameraDeactivated);
    }

    private void OnCameraActivated(ICinemachineCamera newCam, ICinemachineCamera prevCam)
    {
        // If the new camera has the "CutsceneCam" tag, freeze movement
        if (newCam != null && newCam.VirtualCameraGameObject.CompareTag("CMCam"))
        {
            player.SetCutsceneLocked(true);
        }
        else
        {
            player.SetCutsceneLocked(false);
        }
    }

    private void OnCameraDeactivated(ICinemachineCamera oldCam, ICinemachineCamera newCam)
    {
        // If the old camera has the "CutsceneCam" tag, unfreeze movement
        if (oldCam != null && oldCam.VirtualCameraGameObject.CompareTag("CMCam"))
        {
            player.SetCutsceneLocked(false);
        }
    }
}
