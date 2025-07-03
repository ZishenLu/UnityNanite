using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class NanitePass : ScriptableRenderPass
{
    public void Init(Material mat)
    {
        renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        NaniteMgr.Inst.Init(mat);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        base.RecordRenderGraph(renderGraph, frameData);
        NaniteMgr.Inst.DrawProcedural(renderGraph, frameData);
    }
}
