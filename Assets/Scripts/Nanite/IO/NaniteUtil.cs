using System;
using System.Runtime.InteropServices;

public static class NaniteUtil
{
    [DllImport("UnityNanite")]
    public static extern uint BuildMeshletsBound(uint indexCount, uint maxVertices, uint maxIndices);

    [DllImport("UnityNanite")]
    public static extern uint BuildMeshlets([Out] Meshlet[] meshlets,[Out] uint[] meshletsVertices,
        [Out] byte[] meshletsIndices, uint[] indices, uint indicesCount, float[] verticesPos,
        uint verticesCount, uint verticesStride, uint maxVertices, uint maxIndices, float weight);

    [DllImport("UnityNanite")]
    public static extern void SetDefaultOptions([Out] int[] options);

    [DllImport("UnityNanite")]
    public static extern int PartGraphKway(int graphCount, int ncon, int[] xadj, int[] edgeAdj, int[] edgeWeights,
        int nparts, int[] options,out int edgeCut, [Out] int[] partition);

    [DllImport("UnityNanite")]
    public static extern uint MeshSimplify([Out] uint[] outIndices, uint[] inIndices, uint inIndicesCount,
        float[] vertices, uint verticesCount, uint vertexStride, uint targetIndicesCount, float targetError,
        uint options, out float resultError);

    [DllImport("UnityNanite")]
    public static extern uint MeshSimplifyWithAttributes([Out] uint[] destination, uint[] indices,
        uint index_count, IntPtr vertex_positions, uint vertex_count, uint vertex_positions_stride,
        IntPtr vertex_attributes, uint vertex_attributes_stride,
        IntPtr attribute_weights, uint attribute_count,  byte[] vertex_lock,
        uint target_index_count, float target_error, uint options, out float result_error);

    [DllImport("UnityNanite")]
    public static extern float SimplifyScale(float[] array, uint count, uint stride);

    [DllImport("UnityNanite")]
    public static extern MeshoptBounds ComputeClusterBounds(uint[] indices, uint indicesCount,
        float[] vertices, uint vertexCount, uint vertexStride);
}
