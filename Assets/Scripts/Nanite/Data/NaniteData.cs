using System;
using System.Collections.Generic;
using UnityEngine;

public struct Sphere
{
    public Vector3 center;
    public float radius;

    public override string ToString()
    {
        return $"center: ({center.x}, {center.y}, {center.z}) radius: {radius}";
    }
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

public class AdjacencyData
{
    public List<int> XAdjacency;
    public List<int> EdgeAdjacency;
    public List<int> EdgeWeights;
    public Dictionary<MeshletEdge, List<uint>> Edge2Meshlets;
    public Dictionary<uint, List<MeshletEdge>> Meshlet2Edges;

    public AdjacencyData()
    {
        XAdjacency = new List<int>();
        EdgeAdjacency = new List<int>();
        EdgeWeights = new List<int>();
        Edge2Meshlets = new Dictionary<MeshletEdge, List<uint>>();
        Meshlet2Edges = new Dictionary<uint, List<MeshletEdge>>();
    }
}
