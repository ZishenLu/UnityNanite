using System.Collections.Generic;
using UnityEngine;

public class AdjacencyBuilder
{
    private AdjacencyData _adjacencyData;
    private ClusterData _clusterData;

    public AdjacencyData AdjacencyData => _adjacencyData;

    public AdjacencyBuilder(ClusterData clusterData)
    {
        _adjacencyData = new AdjacencyData();
        _clusterData = clusterData;
    }

    public static AdjacencyData CreateAdjacencyData(ClusterData clusterData)
    {
        var builder = new AdjacencyBuilder(clusterData);
        builder.BuildMeshletEdgeMap();
        builder.BuildAdjacency();
        return builder.AdjacencyData;
    }

    public void BuildMeshletEdgeMap()
    {
        for (uint meshletIndex = 0; meshletIndex < _clusterData.meshletCount; meshletIndex++)
        {
            for (uint triangleIndex = 0; triangleIndex < _clusterData.meshlets[meshletIndex].triangle_count; triangleIndex++)
            {
                for (uint index = 0; index < 3; index++)
                {
                    uint aIndex = _clusterData.GetIndex(meshletIndex, triangleIndex * 3 + index);
                    uint bIndex = _clusterData.GetIndex(meshletIndex, triangleIndex * 3 + (index + 1) % 3);
                    MeshletEdge meshletEdge = new MeshletEdge(aIndex, bIndex);

                    _adjacencyData.Meshlet2Edges.AddMeshlet2Edges(meshletIndex, meshletEdge);
                    _adjacencyData.Edge2Meshlets.AddEdge2Meshlets(meshletIndex, meshletEdge);
                }
            }
        }
        _adjacencyData.Edge2Meshlets.RemoveEdge2Meshlets();
    }

    public void BuildAdjacency()
    {
        for (uint meshletIndex = 0; meshletIndex < _clusterData.meshletCount; meshletIndex++)
        {
            int startIndex = _adjacencyData.EdgeAdjacency.Count;
            _adjacencyData.XAdjacency.Add(startIndex);
            foreach (var edge in _adjacencyData.Meshlet2Edges[meshletIndex])
            {
                if (!_adjacencyData.Edge2Meshlets.ContainsKey(edge)) continue;
                foreach (var meshlet in _adjacencyData.Edge2Meshlets[edge])
                {
                    if (meshlet != meshletIndex)
                    {
                        var curRange = _adjacencyData.EdgeAdjacency.GetRange(startIndex, _adjacencyData.EdgeAdjacency.Count - startIndex);
                        var connectionMeshletIndex = curRange.FindIndex(x => x == meshlet);
                        if (connectionMeshletIndex != -1)
                        {
                            _adjacencyData.EdgeWeights[connectionMeshletIndex + startIndex]++;

                        }
                        else
                        {
                            _adjacencyData.EdgeAdjacency.Add((int)meshlet);
                            _adjacencyData.EdgeWeights.Add(1);
                        }
                    }
                }
            }
        }
        _adjacencyData.XAdjacency.Add(_adjacencyData.EdgeAdjacency.Count);
    }
}