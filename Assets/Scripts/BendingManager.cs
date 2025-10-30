using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class BendingManager : MonoBehaviour
{
    #region Constants

    private const string BENDING_FEATURE = "_ENABLE_BENDING";

    #endregion

    #region MonoBehaviour

    private void Awake()
    {
        // enable bending only during playmode
        if (Application.isPlaying)
        {
            Shader.EnableKeyword(BENDING_FEATURE);
        }
        else
        {
            Shader.DisableKeyword(BENDING_FEATURE); // disable in editor
        }
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    #endregion

    #region Rendering Hooks

    private static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType == CameraType.SceneView)
        {
            Shader.DisableKeyword(BENDING_FEATURE);
            return;
        }

        // Apply global bending keyword
        Shader.EnableKeyword(BENDING_FEATURE);

        // changes the way unity renders objects
        Matrix4x4 bendingCullingMatrix = Matrix4x4.Ortho(-99, 99, -99, 99, 0.001f, 99) * camera.worldToCameraMatrix;
        camera.cullingMatrix = bendingCullingMatrix;
    }

    private static void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        // reset camera after render
        camera.ResetCullingMatrix();
    }

    #endregion
}
