using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class NaniteMgr
{
    private static NaniteMgr _inst;

    public static NaniteMgr Inst
    {
        get
        {
            if( _inst == null)
            {
                _inst = new NaniteMgr();
            }
            return _inst;
        }
    }

    internal class PassData
    {
        
    }

    private Material _material;
    private GraphicsBuffer _argsBuffer;
    private GraphicsBuffer _dataBuffer;
    private GraphicsBuffer _verticesBuffer;

    public void Init(Material mat)
    {
        _material = mat;
        InitArgs();
    }

    private void InitArgs()
    {
        uint[] datas = ClusterMgr.Inst.ReadDatas();
        uint triCount = ClusterMgr.Inst.Count;
        Vector3[] vertices = ClusterMgr.Inst.ReadVerticesDatas();
        _argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 5, sizeof(uint));
        _dataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, datas.Length, sizeof(uint));
        _verticesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertices.Length, sizeof(float) * 3);
        uint[] args = new uint[5];
        args[0] = triCount * 3;
        args[1] = (uint)datas.Length / args[0];
        args[2] = 0;
        args[3] = 0;
        args[4] = 0;
        _argsBuffer.SetData(args);
        _dataBuffer.SetData(datas);
        _verticesBuffer.SetData(vertices);
    }

    public void DrawProcedural(RenderGraph graph, ContextContainer frameData)
    {
        var camTarget = frameData.Get<UniversalResourceData>().activeColorTexture;
        var depthTarget = frameData.Get<UniversalResourceData>().activeDepthTexture;
        using (var builder = graph.AddRasterRenderPass<PassData>("MapPass", out var passData))
        {
            builder.SetRenderAttachment(camTarget, 0, AccessFlags.ReadWrite);
            builder.SetRenderAttachmentDepth(depthTarget, AccessFlags.ReadWrite);
            builder.AllowGlobalStateModification(true);
            builder.SetRenderFunc<PassData>((PassData data, RasterGraphContext context) =>
            {
                DrawProcedural(data, context);
            });
        }
    }

    private void DrawProcedural(PassData data, RasterGraphContext context)
    {
        var cmd = context.cmd;

        _material.SetBuffer("_DataBuffer", _dataBuffer);
        _material.SetBuffer("_VertexBuffer", _verticesBuffer);
        cmd.DrawProceduralIndirect(Matrix4x4.identity, _material, 0, MeshTopology.Triangles, _argsBuffer);
    }
}
