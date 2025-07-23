using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class NanitePass : ScriptableRenderPass
{
    public void Init(Material mat, ComputeShader shader, GameObject go)
    {
        renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        NaniteMgr.Inst.Init(mat, shader, go);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        NaniteMgr.Inst.CullingTest(renderGraph, frameData);
        NaniteMgr.Inst.DrawProcedural(renderGraph, frameData);
    }
}
