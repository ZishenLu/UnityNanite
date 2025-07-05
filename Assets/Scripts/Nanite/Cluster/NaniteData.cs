using UnityEngine;

public struct Sphere
{
    public Vector3 center;
    public float radius;
}

public struct ClusterData
{
    public uint[] indices;
    public uint lod;
    public uint triangleCount;
    public Sphere sphere;
    public Sphere parentSphere;
    public float error;
    public float parentError;
}

public struct MeshletEdge
{
    public uint first;
    public uint second;
    public MeshletEdge(uint aIndex, uint bIndex)
    {
        if (aIndex < bIndex)
        {
            first = aIndex; second = bIndex;
        }
        else
        {
            first = bIndex; second = aIndex;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is MeshletEdge edge)
        {
            return edge.first == first && edge.second == second;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(first, second);
    }
}

public struct MeshletGroup
{
    public List<int> meshlets;
}
