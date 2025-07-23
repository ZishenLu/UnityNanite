using UnityEngine;

public class ClusterDataBuilder
{
    public ClusterData clusterData;
    private readonly uint _maxVertices = ClusterMgr.Inst.maxVertices;
    private readonly uint _maxIndices = ClusterMgr.Inst.maxIndices;
    private float[] _vertices;
    private uint[] _indices;
    private float _error;

    public ClusterDataBuilder(float[] vertices, uint[] indices, float error)
    {
        clusterData = new ClusterData();
        _vertices = vertices;
        _indices = indices;
        _error = error;
    }

    public static ClusterData CreateClusterData(float[] vertices, uint[] indices, float error)
    {
        var builder = new ClusterDataBuilder(vertices, indices, error);
        builder.BuildMeshlets();
        return builder.clusterData;
    }

    public void BuildMeshlets()
    {
        uint maxLets = NaniteUtil.BuildMeshletsBound(
            (uint)_indices.Length,
            _maxVertices,
            _maxIndices
        );

        var meshlets = new Meshlet[maxLets];
        clusterData.meshletVertices = new uint[maxLets * _maxVertices];
        clusterData.meshletIndices = new byte[maxLets * _maxIndices * 3];

        clusterData.meshletCount = NaniteUtil.BuildMeshlets(
            meshlets,
            clusterData.meshletVertices,
            clusterData.meshletIndices,
            _indices,
            (uint)_indices.Length,
            _vertices,
            (uint)_vertices.Length / 3,
            sizeof(float) * 3,
            _maxVertices,
            _maxIndices,
            0.0f
        );

        clusterData.meshlets = new Cluster[clusterData.meshletCount];
        for (int i = 0; i < clusterData.meshletCount; i++)
        {
            clusterData.meshlets[i].triangle_offset = meshlets[i].triangle_offset;
            clusterData.meshlets[i].triangle_count = meshlets[i].triangle_count;
            clusterData.meshlets[i].vertex_offset = meshlets[i].vertex_offset;
            clusterData.meshlets[i].vertex_count = meshlets[i].vertex_count;
            clusterData.meshlets[i].error = _error;
        }
    }
}
