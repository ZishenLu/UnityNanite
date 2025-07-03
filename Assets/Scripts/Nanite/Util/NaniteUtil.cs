using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Meshlet
{
    public uint vertex_offset;
    public uint triangle_offset;
    public uint vertex_count;
    public uint triangle_count;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct meshopt_Bounds
{
    Vector3 center;   // ��Χ������
    float radius;      // ��Χ��뾶

    // ���ڱ����޳��İ�Χ׶
    Vector3 cone_apex;
    Vector3 cone_axis;
    float cone_cutoff; // = cos(angle/2)

    // ׶���޳�����Ч��
    Vector3 cone_axis_s8; // 8λ���Ż��汾
};

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
        int nparts, int[] options, int edgeCut, [Out] int[] partition);

    [DllImport("UnityNanite")]
    public static extern uint MeshSimplify([Out] uint[] outIndices, uint[] inIndices, uint inIndicesCount,
        float[] vertices, uint verticesCount, uint vertexStride, uint targetIndicesCount, float targetError,
        uint options, [Out] float resultError);

    [DllImport("UnityNanite")]
    public static extern uint MeshSimplifyWithAttributes([Out] uint[] destination, uint[] indices,
        uint index_count, IntPtr vertex_positions, uint vertex_count, uint vertex_positions_stride,
        IntPtr vertex_attributes, uint vertex_attributes_stride,
        IntPtr attribute_weights, uint attribute_count,  byte[] vertex_lock,
        uint target_index_count, float target_error, uint options, [Out] float result_error);

    [DllImport("UnityNanite")]
    public static extern float SimplifyScale(float[] array, uint count, uint stride);
}
