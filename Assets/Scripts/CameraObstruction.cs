using System.Collections.Generic;
using UnityEngine;

public class CameraObstructionFade : MonoBehaviour
{
    [Header("Setup")]
    public Transform target;             
    public LayerMask obstructionMask;   

    [Header("Fade Settings")]
    [Range(0f, 1f)] public float fadedOpacity = 0.3f;
    public float fadeSpeed = 3f;        

    Camera _cam;

    // track obstructions/their current opacity
    class Fader
    {
        public Renderer renderer;
        public float currentOpacity = 1f;
    }

    readonly Dictionary<Renderer, Fader> _activeFaders = new Dictionary<Renderer, Fader>();
    readonly List<Renderer> _hitsThisFrame = new List<Renderer>();

    void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (!target) return;

        // raycast from camera
        Vector3 camPos = _cam.transform.position;
        Vector3 dir = target.position - camPos;
        float dist = dir.magnitude;

        _hitsThisFrame.Clear();

        RaycastHit[] hits = Physics.RaycastAll(
            camPos,
            dir.normalized,
            dist,
            obstructionMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (var hit in hits)
        {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null) continue;

            _hitsThisFrame.Add(rend);

            if (!_activeFaders.ContainsKey(rend))
            {
                _activeFaders[rend] = new Fader
                {
                    renderer = rend,
                    currentOpacity = 1f
                };
            }
        }

       //fade out objects that are currently in the way
        foreach (var kvp in _activeFaders)
        {
            Fader f = kvp.Value;
            bool isHitThisFrame = _hitsThisFrame.Contains(f.renderer);

            float targetOpacity = isHitThisFrame ? fadedOpacity : 1f;
            f.currentOpacity = Mathf.MoveTowards(
                f.currentOpacity,
                targetOpacity,
                fadeSpeed * Time.deltaTime
            );

            SetOpacityOnRenderer(f.renderer, f.currentOpacity);
        }

        // remove finished faders 
        List<Renderer> toRemove = null;
        foreach (var kvp in _activeFaders)
        {
            Fader f = kvp.Value;
            bool isHitThisFrame = _hitsThisFrame.Contains(f.renderer);

            if (!isHitThisFrame && Mathf.Approximately(f.currentOpacity, 1f))
            {
                if (toRemove == null) toRemove = new List<Renderer>();
                toRemove.Add(kvp.Key);
            }
        }

        if (toRemove != null)
        {
            foreach (var r in toRemove)
            {
                _activeFaders.Remove(r);
            }
        }
    }

    static readonly int OpacityID = Shader.PropertyToID("_Opacity");

    void SetOpacityOnRenderer(Renderer rend, float opacity)
    {
        //  MaterialPropertyBlock
        var block = new MaterialPropertyBlock();
        rend.GetPropertyBlock(block);
        block.SetFloat(OpacityID, opacity);
        rend.SetPropertyBlock(block);
    }
}
