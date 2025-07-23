using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NaniteAuthoring : MonoBehaviour
{
    private NanitePass _pass;
    public Material material;
    public ComputeShader computeShader;

    private void Awake()
    {
        _pass = new NanitePass();
        _pass.Init(material, computeShader, gameObject);
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
    }

    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        cam.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(_pass);
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
    }
}
