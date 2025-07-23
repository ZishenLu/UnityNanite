using System.Collections.Generic;

public struct MeshletData
{
    public uint triangleOffset;
    public uint triangleCount;
    public uint lod;
    public Sphere sphere;
    public Sphere parentSphere;
    public float error;
    public float parentError;
}

public struct MeshletGroupData
{
    public uint meshletOffset;
    public uint meshletCount;
    public Sphere sphere;
    public float error;
}


public struct MeshletGroup
{
    public List<int> meshlets;
}

public struct Cluster
{
    public uint vertex_offset;
    public uint triangle_offset;
    public uint vertex_count;
    public uint triangle_count;
    public float error;
}

public class ClusterData
{
    public Cluster[] meshlets;
    public uint meshletCount;
    public uint[] meshletVertices;
    public byte[] meshletIndices;
}