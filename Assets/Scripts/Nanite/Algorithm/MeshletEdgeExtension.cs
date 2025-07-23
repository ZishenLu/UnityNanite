using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public static class Edge2MeshletsExtension
{
    public static void AddEdge2Meshlets(this Dictionary<MeshletEdge, List<uint>> edge2Meshlets, uint meshletIndex, MeshletEdge meshletEdge)
    {
        if (edge2Meshlets.ContainsKey(meshletEdge))
        {
            var list = edge2Meshlets[meshletEdge];
            if (!list.Contains(meshletIndex))
            {
                list.Add(meshletIndex);
            }
        }
        else
        {
            edge2Meshlets.Add(meshletEdge, new List<uint> { meshletIndex });
        }
    }

    public static void RemoveEdge2Meshlets(this Dictionary<MeshletEdge, List<uint>> edge2Meshlets)
    {
        List<MeshletEdge> keys = new List<MeshletEdge>(edge2Meshlets.Keys);
        for (int i = keys.Count - 1; i >= 0; i--)
        {
            var key = keys[i];
            if (edge2Meshlets[key].Count < 2 || edge2Meshlets[key][0] == edge2Meshlets[key][1])
            {
                edge2Meshlets.Remove(key);
            }
        }
    }
}

public static class Meshlets2EdgeExtension
{
    public static void AddMeshlet2Edges(this Dictionary<uint, List<MeshletEdge>> meshlet2Edges, uint meshletIndex, MeshletEdge meshletEdge)
    {
        if (meshlet2Edges.ContainsKey(meshletIndex))
        {
            var list = meshlet2Edges[meshletIndex];
            if (!list.Contains(meshletEdge))
            {
                list.Add(meshletEdge);
            }
        }
        else
        {
            meshlet2Edges.Add(meshletIndex, new List<MeshletEdge> { meshletEdge });
        }
    }
}

public static class ClusterDataExtension
{
    public static uint GetIndex(this ClusterData clusterData, uint meshletIndex, uint triangleIndex)
    {
        uint indexCount = clusterData.meshlets[meshletIndex].triangle_count * 3;
        if (triangleIndex >= indexCount)
        {
            triangleIndex = indexCount - 1;
        }

        uint triOffset = clusterData.meshlets[meshletIndex].triangle_offset;
        uint verOffset = clusterData.meshlets[meshletIndex].vertex_offset;

        byte localIndex = clusterData.meshletIndices[triOffset + triangleIndex];
        uint triIndex = clusterData.meshletVertices[localIndex + verOffset];
        return triIndex;
    }

    public static uint GetMeshletIndex(this Cluster meshlet, ClusterData clusterData, int triangleIndex)
    {
        byte localIndex = clusterData.meshletIndices[meshlet.triangle_offset + triangleIndex];
        return clusterData.meshletVertices[meshlet.vertex_offset + localIndex];
    }

    public static uint[] GetGroupIndices(this MeshletGroup group, ClusterData clusterData)
    {
        List<uint> indices = new List<uint>();
        foreach (int meshletIdx in group.meshlets)
        {
            Cluster meshlet = clusterData.meshlets[meshletIdx];
            indices.AddRange(meshlet.GetMeshletIndices(clusterData));
        }
        return indices.ToArray();
    }

    public static uint[] GetMeshletIndices(this Cluster meshlet, ClusterData clusterData)
    {
        uint[] indices = new uint[meshlet.triangle_count * 3];
        for (int i = 0; i < meshlet.triangle_count * 3; i++)
        {
            indices[i] = meshlet.GetMeshletIndex(clusterData, i);
        }
        return indices;
    }

    public static ClusterData MergeClusterDatas(this ClusterData[] clusterDatas)
    {
        ClusterData clusterData = new ClusterData();
        uint meshletCount = 0;
        int meshletVertexCount = 0;
        int meshletIndexCount = 0;
        for (int i = 0; i < clusterDatas.Length; i++)
        {
            meshletCount += clusterDatas[i].meshletCount;
            meshletVertexCount += clusterDatas[i].meshletVertices.Length;
            meshletIndexCount += clusterDatas[i].meshletIndices.Length;
        }

        clusterData.meshlets = new Cluster[meshletCount];
        clusterData.meshletVertices = new uint[meshletVertexCount];
        clusterData.meshletIndices = new byte[meshletIndexCount];
        clusterData.meshletCount = meshletCount;

        uint meshletOffset = 0;
        int vertexOffset = 0;
        int indexOffset = 0;

        for (int i = 0; i < clusterDatas.Length; i++)
        {
            var tmpData = clusterDatas[i];
            for (int j = 0; j < tmpData.meshletCount; j++)
            {
                tmpData.meshlets[j].vertex_offset += (uint)vertexOffset;
                tmpData.meshlets[j].triangle_offset += (uint)indexOffset;
            }
            Array.Copy(tmpData.meshlets, 0, clusterData.meshlets, meshletOffset, tmpData.meshletCount);
            Array.Copy(tmpData.meshletVertices, 0, clusterData.meshletVertices, vertexOffset, tmpData.meshletVertices.Length);
            Array.Copy(tmpData.meshletIndices, 0, clusterData.meshletIndices, indexOffset, tmpData.meshletIndices.Length);
            meshletOffset += tmpData.meshletCount;
            vertexOffset += tmpData.meshletVertices.Length;
            indexOffset += tmpData.meshletIndices.Length;
        }
        return clusterData;
    }

    public static uint[] GetIndices(this ClusterData clusterData)
    {
        List<uint> indices = new List<uint>();
        for(int i = 0; i < clusterData.meshletCount; i++)
        {
            var meshlet = clusterData.meshlets[i];
            for (int j = 0; j < meshlet.triangle_count * 3; j++)
            {
                indices.Add(meshlet.GetMeshletIndex(clusterData, j));
            }
        }
        return indices.ToArray();
    }

    public static MeshletData[] CreateMeshletDatas(this ClusterData clusterData, uint lod, float[] vertices, uint indicesOffset)
    {
        MeshletData[] meshletDatas = new MeshletData[clusterData.meshletCount];
        uint offset = 0;
        for (int i = 0; i < clusterData.meshletCount; i++)
        {
            var meshlet = clusterData.meshlets[i];
            var indices = meshlet.GetMeshletIndices(clusterData);
            var sphere = indices.GetBoundSphere(vertices);
            meshletDatas[i] = new MeshletData()
            {
                triangleOffset = offset + indicesOffset,
                lod = lod,
                triangleCount = meshlet.triangle_count,
                sphere = sphere,
                error = meshlet.error,
                parentError = float.MaxValue,
            };
            offset += meshlet.triangle_count * 3;
        }
        return meshletDatas;
    }

    public static float[] GetVertices(this uint[] indices, float[] vertices)
    {
        float[] targetVertices = new float[indices.Length * 3];
        for (int i = 0; i < indices.Length; i++)
        {
            targetVertices[3 * i] = vertices[3 * indices[i]];
            targetVertices[3 * i + 1] = vertices[3 * indices[i] + 1];
            targetVertices[3 * i + 2] = vertices[3 * indices[i] + 2];
        }
        return targetVertices;
    }

    public static Sphere GetBoundSphere(this uint[] indices, float[] vertices) 
    { 
        Sphere sphere = new Sphere();
        var data = NaniteUtil.ComputeClusterBounds(
            indices,
            (uint)indices.Length,
            vertices,
            (uint)vertices.Length / 3,
            sizeof(float) * 3
        );

        sphere.center = data.center;
        sphere.radius = data.radius;
        return sphere;
    }

    public static Sphere MergeSpheres(this Sphere a, Sphere b)
    {
        // 计算两球心距离
        Vector3 diff = b.center - a.center;
        float distance = diff.magnitude;

        // 如果一球完全包含另一球
        if (distance + b.radius <= a.radius) return a;
        if (distance + a.radius <= b.radius) return b;

        // 计算新包围球
        float newRadius = (distance + a.radius + b.radius) * 0.5f;
        Vector3 direction = diff.normalized;
        Vector3 newCenter = a.center + direction * (newRadius - a.radius);

        return new Sphere { center = newCenter, radius = newRadius };
    }
}