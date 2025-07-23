using System.Runtime.InteropServices;
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
        public ComputeShader computeShader;
        public Matrix4x4 vp;
        public BufferHandle graphicsBuffer;
        public Vector4[] frustumPlanes;
        public int meshletCount;
        public float cotHalfFov;
    }

    private Material _material;
    private ComputeShader _cShader;
    private GameObject _go;
    private int _meshletCount;
    private GraphicsBuffer _argsBuffer;
    private GraphicsBuffer _dataBuffer;
    private GraphicsBuffer _verticesBuffer;
    private GraphicsBuffer _indicesBuffer;
    private GraphicsBuffer _resultBuffer;

    public void Init(Material mat, ComputeShader shader, GameObject go)
    {
        _go = go;
        _material = mat;
        _cShader = shader;
        InitArgs();
    }

    private void InitArgs()
    {
        MeshletData[] datas = ClusterMgr.Inst.ReadDatas();
        uint triCount = ClusterMgr.Inst.Count;
        int size = Marshal.SizeOf(typeof(MeshletData));
        Vector3[] vertices = ClusterMgr.Inst.ReadVerticesDatas();
        uint[] indices = ClusterMgr.Inst.ReadIndices();
        _meshletCount = datas.Length;
        _argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 5, sizeof(uint));
        _dataBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, datas.Length, size);
        _verticesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, vertices.Length, sizeof(float) * 3);
        _indicesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, indices.Length, sizeof(uint));
        _resultBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Append, datas.Length, sizeof(uint));
        uint[] args = new uint[5];
        args[0] = triCount * 3;
        args[1] = (uint)datas.Length;
        args[2] = 0;
        args[3] = 0;
        args[4] = 0;
        _argsBuffer.SetData(args);
        _dataBuffer.SetData(datas);
        _verticesBuffer.SetData(vertices);
        _indicesBuffer.SetData(indices);
    }

    public void CullingTest(RenderGraph graph, ContextContainer frameData)
    {
        var camTarget = frameData.Get<UniversalResourceData>().activeColorTexture;
        var depthTarget = frameData.Get<UniversalResourceData>().activeDepthTexture;
        using (var builder = graph.AddComputePass<PassData>("CullingTestPass", out var passData))
        {
            builder.AllowGlobalStateModification(true);
            builder.SetRenderFunc<PassData>((PassData data, ComputeGraphContext context) =>
            {
                //var proj = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, true);
                var vp = Camera.main.worldToCameraMatrix;
                data.vp = vp * _go.transform.localToWorldMatrix;
                data.computeShader = _cShader;
                data.graphicsBuffer = graph.ImportBuffer(_resultBuffer);
                data.meshletCount = _meshletCount;
                data.cotHalfFov = Mathf.Cos(Camera.main.fieldOfView * Mathf.Deg2Rad * 0.5f) / Mathf.Sin(Camera.main.fieldOfView * Mathf.Deg2Rad * 0.5f);
                CullingTest(data, context);
            });
        }
    }

    private void CullingTest(PassData data, ComputeGraphContext context)
    {
        var cmd = context.cmd;
        var kernelId = data.computeShader.FindKernel("CSMain");
        cmd.SetBufferCounterValue(_resultBuffer, 0);
        cmd.SetComputeMatrixParam(data.computeShader, "mvp", data.vp);
        cmd.SetComputeIntParam(data.computeShader, "clustersCount", data.meshletCount);
        cmd.SetComputeBufferParam(data.computeShader, kernelId, "clusters", _dataBuffer);
        cmd.SetComputeBufferParam(data.computeShader, kernelId, "Result", _resultBuffer);
        cmd.SetComputeVectorParam(data.computeShader, "screenSize", new Vector4(Screen.width, Screen.height, data.cotHalfFov, 1));
        cmd.DispatchCompute(data.computeShader, kernelId, 1 + _meshletCount / 64, 1, 1);
        cmd.CopyCounterValue(_resultBuffer, _argsBuffer, sizeof(uint));
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
        _material.SetBuffer("_IndexBuffer", _indicesBuffer);
        _material.SetBuffer("_ResultBuffer", _resultBuffer);
        cmd.DrawProceduralIndirect(Matrix4x4.identity, _material, 0, MeshTopology.Triangles, _argsBuffer);
    }
}
